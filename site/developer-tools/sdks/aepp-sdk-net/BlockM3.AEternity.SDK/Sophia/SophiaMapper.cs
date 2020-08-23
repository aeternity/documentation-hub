using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Sophia.Attributes;
using BlockM3.AEternity.SDK.Sophia.Types;
using Newtonsoft.Json.Linq;

namespace BlockM3.AEternity.SDK.Sophia
{
    public class Function
    {
        public string Name { get; set; }

        public List<SophiaType> InputTypes { get; set; }

        public SophiaType OutputType { get; set; }

        public bool StateFul { get; set; }
    }


    public static class SophiaMapper
    {
    

        public static string[] SplitByComma(string input) => SplitBy(input, ',', ('(', ')'), ('[', ']'), ('{', '}'), ('"', '"')).ToArray();
        public static string[] SplitByTwoPoints(string input) => SplitBy(input, ':', ('(', ')'), ('[', ']'), ('{', '}'), ('"', '"')).ToArray();
        public static string[] SplitByPipe(string input) => SplitBy(input, '|', ('(', ')'), ('[', ']'), ('{', '}'), ('"', '"')).ToArray();

        public static List<string> SplitBy(string input, char separator, params (char start, char end)[] pars)
        {
            List<(char start, char end)> ls = pars.ToList();

            List<string> result=new List<string>();
            int start = -1;
            int accum = 0;
            string constr = string.Empty;
            foreach(char c in input)
            {
                if (c == separator && start==-1)
                {
                    if (constr.Trim() != string.Empty)
                    {
                        result.Add(constr);
                        constr = string.Empty;
                    }
                    continue;
                }
                constr += c;
                if (ls.Any(a => a.end == c))
                {

                    int pos = ls.IndexOf(ls.First(a => a.end == c));
                    if (start == pos)
                    {
                        accum--;
                        if (accum == 0)
                            start = -1;
                        continue;
                    }
                }
                if (ls.Any(a => a.start == c))
                {
                    int pos = ls.IndexOf(ls.First(a => a.start == c));
                    if (start == -1)
                    {
                        start = pos;
                        accum = 1;
                    }
                    else if (start == pos)
                    {
                        accum++;
                    }
                }


            }
            if (constr.Trim()!=string.Empty)
                result.Add(constr);
            return result;
        }

        public static string ClassToOracleFormat<T>()
        {
            TypeInfo info = typeof(T).GetTypeInfo();
            StringBuilder bld = new StringBuilder();
            bld.Append("{ ");
            bool add = false;
            foreach ((string s, PropertyInfo p) in SophiaType.OrderProps(info))
            {
                add = true;
                SophiaType tp = p.PropertyType.ToSophiaType(p.GetCustomAttributes().ToList());
                bld.Append("\"" + s + "\"");
                bld.Append(":");
                bld.Append(tp.MapName);
                bld.Append(", ");
            }

            if (add)
                bld.Remove(bld.Length - 2, 2);
            bld.Append("}");
            return bld.ToString();
        }

        public static string SerializeOracleClass<T>(T cls)
        {
            TypeInfo info = typeof(T).GetTypeInfo();
            StringBuilder bld = new StringBuilder();
            bld.Append("{ ");
            bool add = false;
            foreach ((string s, PropertyInfo p) in SophiaType.OrderProps(info))
            {
                add = true;
                SophiaType tp = p.PropertyType.ToSophiaType(p.GetCustomAttributes().ToList());
                bld.Append("\"" + s + "\"");
                bld.Append(":");
                bld.Append(tp.Serialize(p.GetValue(cls), p.PropertyType));
                bld.Append(", ");
            }

            if (add)
                bld.Remove(bld.Length - 2, 2);
            bld.Append("}");
            return bld.ToString();
        }

        public static T DeserializeOracleClass<T>(string t)
        {
            TypeInfo info = typeof(T).GetTypeInfo();
            string s = t.Trim();
            if (s.StartsWith("{") && s.EndsWith("}"))
                s = s.Substring(1, s.Length - 2);
            string[] items = SplitByComma(s);
            List<(string s, PropertyInfo p)> props = SophiaType.OrderProps(info);
            object o = Activator.CreateInstance(typeof(T));

            foreach (string n in items)
            {
                string[] grp = SplitByTwoPoints(n);
                if (grp.Length != 2)
                    throw new ArgumentException($"Unable to deserialize Oracle response parameter {n}");
                string key = grp[0].Trim();
                string value = grp[1].Trim();
                if (key.StartsWith("\"") && key.EndsWith("\""))
                    key = key.Substring(1, key.Length - 2);
                if (key.StartsWith("'") && key.EndsWith("'"))
                    key = key.Substring(1, key.Length);
                (string k, PropertyInfo p) = props.FirstOrDefault(a => a.s == key);
                if (k == null)
                    throw new ArgumentException($"Unable to deserialize Oracle response parameter key {key} is missing in class {info.Name}");
                SophiaType tp = p.PropertyType.ToSophiaType(p.GetCustomAttributes().ToList());
                p.SetValue(o, tp.Deserialize(value, p.PropertyType));
            }

            return (T) o;
        }

        private static List<Attribute> GetAttributeFromGeneric(SophiaGenericAttribute attr, int idx)
        {
            if (attr == null)
                return new List<Attribute>();
            if (!attr.Attributes.ContainsKey(idx))
                return new List<Attribute>();
            return new List<Attribute> {(Attribute) attr.Attributes.First(a => a.Key == idx).Value};
        }

        public static bool ImplementsGenericInterface(this Type type, Type iface)
        {
            return type.GetTypeInfo().ImplementedInterfaces.Any(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == iface);
        }

        public static SophiaType ToSophiaType(this Type t, List<Attribute> attrs)
        {
            if (typeof(byte[]).IsAssignableFrom(t))
            {
                if (attrs.Any(a => a.GetType() == typeof(SophiaHashAttribute)))
                    return new HashType("hash");
                if (attrs.Any(a => a.GetType() == typeof(SophiaSignatureAttribute)))
                    return new SignatureType("signature");
                SophiaBytesAttribute sofb = (SophiaBytesAttribute) attrs.FirstOrDefault(a => a.GetType() == typeof(SophiaBytesAttribute)) ?? t.GetCustomAttribute<SophiaBytesAttribute>();
                if (sofb != null)
                    return new BytesType("bytes", sofb.Length);
                return new BytesType("bytes", null);
            }

            if (typeof(string).IsAssignableFrom(t))
            {
                if (attrs.Any(a => a.GetType() == typeof(SophiaAddressAttribute)))
                    return new AddressType("address");
                if (attrs.Any(a => a.GetType() == typeof(SophiaOracleQueryAttribute)))
                    return new OracleQueryType("oracle_query('a, 'b)");
                if (attrs.Any(a => a.GetType() == typeof(SophiaOracleAttribute)))
                    return new OracleType("oracle('a, 'b)");
                if (attrs.Any(a => a.GetType() == typeof(SophiaContractAttribute)))
                    return new ContractType("contract");
                return new StringType("string");
            }

            SophiaGenericAttribute generic = (SophiaGenericAttribute) attrs.FirstOrDefault(a => a.GetType() == typeof(SophiaGenericAttribute));

            if (t.ImplementsGenericInterface(typeof(IEnumerable)))
            {
                if (t.ImplementsGenericInterface(typeof(IDictionary)))
                {
                    SophiaType key = ToSophiaType(t.GetGenericArguments()[0], GetAttributeFromGeneric(generic, 0));
                    SophiaType value = ToSophiaType(t.GetGenericArguments()[0], GetAttributeFromGeneric(generic, 1));
                    return new MapType("map", key, value);
                }

                SophiaType k = ToSophiaType(t.GetGenericArguments()[0], GetAttributeFromGeneric(generic, 0));
                return new ListType("list", k);
            }

            if (typeof(bool).IsAssignableFrom(t))
                return new BoolType("bool");

            string fullname = t.FullName;
            if (fullname != null && fullname.StartsWith("System.ValueTuple"))
            {
                List<SophiaType> types = new List<SophiaType>();
                int x = 0;
                foreach (FieldInfo f in t.GetFields())
                {
                    types.Add(ToSophiaType(f.FieldType, GetAttributeFromGeneric(generic, x)));
                    x++;
                }

                return new TupleType("tuple", types);
            }

            if (t.IsEnum || SophiaType.GetUnderlyingEnum(t) != null)
                return new OptionType("option('a)");
            if (typeof(byte).IsAssignableFrom(t) || typeof(ushort).IsAssignableFrom(t) || typeof(short).IsAssignableFrom(t) || typeof(int).IsAssignableFrom(t) || typeof(uint).IsAssignableFrom(t) || typeof(long).IsAssignableFrom(t) || typeof(ulong).IsAssignableFrom(t) || typeof(BigInteger).IsAssignableFrom(t))
            {
                return new IntType("int");
            }

            if (typeof(Ttl).IsAssignableFrom(t))
                return new ChainType("Chain.ttl");
            throw new ArgumentException($"Unsupported type mapping, type: {t.Name}");
        }

        public static List<Event> GetEvents(Contract c, List<Generated.Models.Event> log)
        {
            List<Event> evs = new List<Event>();
            foreach (Generated.Models.Event e in log)
            {
                string payload = null;

                List<BigInteger> b = e.Topics.ToList();
                Event ev = new Event();
                if (e.Data != "cb_Xfbg4g==")
                    payload = Encoding.UTF8.GetString(Utils.Encoding.DecodeCheck(e.Data, EncodingType.BASE64));
                ev.Address = e.Address;
                if (!c.Events.ContainsKey(b[0]))
                    continue;
                EventType sop = (EventType) c.Events[b[0]];
                ev.Name = sop.Name;
                if (sop.FieldTypes == null || sop.FieldTypes.Count == 0)
                {
                    evs.Add(ev);
                    continue;
                }

                List<SophiaType> types = sop.FieldTypes.ToList();
                ev.Parameters = new List<object>();
                for (int x = 1; x < b.Count; x++)
                {
                    SophiaType type = types.First(a => a.SophiaBaseName != "string");
                    ev.Parameters.Add(type.FromBigInteger(b[x]));
                    types.Remove(type);
                }

                if (types.Count > 0 && types[0].SophiaBaseName == "string" && payload != null)
                    ev.Parameters.Add(payload);
                evs.Add(ev);
            }

            return evs;
        }

        public static (List<Function> functions, Dictionary<BigInteger, SophiaType> events) ParseACI(string encodedaci, string iface)
        {
            MapperInfo m = new MapperInfo();

            string[] lines = iface.Split('\n');
            foreach (string s in lines)
            {
                string n = s.Trim();
                if (n.StartsWith("contract"))
                {
                    string k = n.Substring(9);
                    int eq = k.IndexOf('=');
                    if (eq > -1)
                        k = k.Substring(0, eq);
                    m.ContractName = k.Trim();
                }
                else if (n.StartsWith("record"))
                {
                    m.Records.Add(n.Substring(6).Trim());
                }
                else if (n.StartsWith("datatype event"))
                {
                    string k = n.Substring(14);
                    int eq = k.IndexOf('=');
                    if (eq > -1)
                        k = k.Substring(eq + 1);
                    string[] sp = SplitByPipe(k);
                    foreach (string l in sp)
                    {
                        m.Events.Add(l.Trim());
                    }
                }
                else if (n.StartsWith("type"))
                {
                    m.TypeDefs.Add(n.Substring(5).Trim());
                }
                else if (n.StartsWith("entrypoint") || n.StartsWith("function"))
                {
                    string k;
                    if (n.StartsWith("entrypoint"))
                        k = n.Substring(11);
                    else
                        k = n.Substring(8);
                    int idx = k.IndexOf(':');
                    string name = k.Substring(0, idx).Trim();
                    k = k.Substring(idx + 1);
                    string[] sp = k.Split(new[] {"=>"}, StringSplitOptions.None);
                    if (sp.Length != 2)
                        throw new ArgumentException($"Unable to parse function from '{n}'");
                    string inputs = sp[0].Trim();
                    string output = sp[1].Trim();
                    List<string> inps = new List<string>();
                    if (inputs != "()")
                    {
                        if (inputs.StartsWith("(") && inputs.EndsWith(")"))
                            inputs = inputs.Substring(1, inputs.Length - 2);
                        string[] items = SplitByComma(inputs);
                        foreach (string l in items)
                        {
                            inps.Add(l.Trim());
                        }
                    }

                    if (output == "()")
                    {
                        output = null;
                    }

                    m.Functions.Add(name, (inps, output));
                }
            }


            List<Function> fns = new List<Function>();
            Dictionary<BigInteger, SophiaType> events = new Dictionary<BigInteger, SophiaType>();
            JObject j = JObject.Parse(encodedaci);
            foreach (string n in m.Functions.Keys)
            {
                Function f = new Function();
                f.Name = n;
                f.InputTypes = new List<SophiaType>();
                (List<string> inputs, string output) = m.Functions[n];
                if (inputs != null && inputs.Count > 0)
                {
                    foreach (string inp in inputs)
                        f.InputTypes.Add(ParseEntry(inp, m));
                }

                if (output != null)
                {
                    f.OutputType = ParseEntry(output, m);
                }

                foreach (JToken function in j["contract"]["functions"].Children())
                {
                    bool stateful = false;
                    bool found = false;
                    foreach (JToken dta in function.Children())
                    {
                        JProperty prop = dta as JProperty;
                        if (prop != null)
                        {
                            if (prop.Name == "name")
                            {
                                if (prop.Value.ToString() == n)
                                    found = true;
                            }

                            if (prop.Name == "stateful")
                            {
                                stateful = bool.Parse(prop.Value.ToString());
                            }
                        }
                    }

                    if (found)
                    {
                        f.StateFul = stateful;
                        break;
                    }
                }

                fns.Add(f);
            }

            if (!fns.Any(a => a.Name == "init"))
            {
                fns.Add(new Function {InputTypes = new List<SophiaType>(), Name = "init", StateFul = true});
            }

            foreach (string s in m.Events)
            {
                EventType ev = ParseEvent(s, m);

                byte[] blake = Utils.Encoding.Hash(Encoding.UTF8.GetBytes(ev.Name));
                if (BitConverter.IsLittleEndian)
                    blake = blake.Reverse().ToArray();
                byte[] b2 = new byte[blake.Length + 1];


                Buffer.BlockCopy(blake, 0, b2, 0, blake.Length);
                events.Add(new BigInteger(b2), ev);
            }

            return (fns, events);
        }

        public static SophiaType GetSophiaPossibleTypeFromType(Type t)
        {
            if (typeof(byte[]).IsAssignableFrom(t))
            {
                return new BytesType("bytes", null);
            }

            if (typeof(string).IsAssignableFrom(t))
            {
                return new StringType("string");
            }

            if (typeof(bool).IsAssignableFrom(t))
                return new BoolType("bool");
            if (typeof(byte).IsAssignableFrom(t) || typeof(ushort).IsAssignableFrom(t) || typeof(short).IsAssignableFrom(t) || typeof(int).IsAssignableFrom(t) || typeof(uint).IsAssignableFrom(t) || typeof(long).IsAssignableFrom(t) || typeof(ulong).IsAssignableFrom(t) || typeof(BigInteger).IsAssignableFrom(t))
            {
                return new IntType("int");
            }

            throw new ArgumentException($"Unsupported type mapping, type: {t.Name}");
        }

        public static EventType ParseEvent(string evnt, MapperInfo mapper)
        {
            if (!evnt.Contains("("))
                return new EventType("event", evnt.Trim(), null);
            int idx = evnt.IndexOf("(", StringComparison.InvariantCulture);
            string name = evnt.Substring(0, idx);
            evnt = evnt.Substring(idx).Trim();
            if (evnt.StartsWith("(") && evnt.EndsWith(")"))
                evnt = evnt.Substring(1, evnt.Length - 2);
            string[] typs = SplitByComma(evnt);
            List<SophiaType> types = new List<SophiaType>();
            foreach (string t in typs)
            {
                types.Add(ParseEntry(t.Trim(), mapper));
            }

            return new EventType("event", name, types);
        }

        public static SophiaType ParseEntry(string entry, MapperInfo mapper)
        {
            if (entry.StartsWith("map"))
            {
                string n = entry.Substring(3).Trim();
                if (n.StartsWith("(") && n.EndsWith(")"))
                    n = n.Substring(1, n.Length - 2);
                string[] types = SplitByComma(n);
                if (types.Length != 2)
                    throw new ArgumentException($"Unable to parse map from '{entry}'");
                SophiaType key = ParseEntry(types[0].Trim(), mapper);
                SophiaType value = ParseEntry(types[1].Trim(), mapper);
                return new MapType(entry, key, value);
            }

            if (entry.StartsWith("list"))
            {
                string n = entry.Substring(4).Trim();
                if (n.StartsWith("(") && n.EndsWith(")"))
                    n = n.Substring(1, n.Length - 2);
                if (n.Length != 0)
                {
                    SophiaType key = ParseEntry(n.Trim(), mapper);
                    return new ListType(entry, key);
                }

                return new ListType(entry, null);
            }

            if (entry.StartsWith("tuple"))
            {
                string n = entry.Substring(5).Trim();
                if (n.StartsWith("(") && n.EndsWith(")"))
                    n = n.Substring(1, n.Length - 2);
                if (n.Length > 0)
                {
                    string[] types = SplitByComma(n);
                    List<SophiaType> ft = types.Select(a => ParseEntry(a, mapper)).ToList();
                    return new TupleType(entry, ft);
                }

                return new TupleType(entry, null);
            }

            if (entry.StartsWith("bytes"))
            {
                string n = entry.Substring(5).Trim();
                if (n.StartsWith("(") && n.EndsWith(")"))
                    n = n.Substring(1, n.Length - 2);
                //n = n.Substring(1, n.Length - 2);
                if (n.Length != 0)
                    return new BytesType(entry, int.Parse(n));
                return new BytesType(entry, null);
            }

            if (entry == "int" || entry=="unit" || entry=="uint")
                return new IntType(entry);
            if (entry == "string")
                return new StringType(entry);
            if (entry == "bits")
                return new BitsType(entry);
            if (entry == "bool")
                return new BoolType(entry);
            if (entry == "address")
                return new AddressType(entry);
            if (entry == "Chain.ttl")
                return new ChainType(entry);
            if (entry == "contract")
                return new ContractType(entry);
            if (entry == "hash")
                return new HashType(entry);
            if (entry.StartsWith("option"))
                return new OptionType(entry);
            if (entry.StartsWith("oracle_query"))
                return new OracleQueryType(entry);
            if (entry.StartsWith("oracle"))
                return new OracleType(entry);
            if (entry.StartsWith("signature"))
                return new SignatureType(entry);
            if (entry.StartsWith(mapper.ContractName))
            {
                string subl = entry.Substring(mapper.ContractName.Length + 1);
                foreach (string s in mapper.Records)
                {
                    int id = s.IndexOf(' ');
                    string name = s.Substring(0, id).Trim();
                    if (name == subl)
                    {
                        id = s.IndexOf('=');
                        string grp = s.Substring(id + 1).Trim();
                        if (grp.StartsWith("{") && grp.EndsWith("}"))
                            grp = grp.Substring(1, grp.Length - 2);
                        string[] spl = SplitByComma(grp);
                        Dictionary<string, SophiaType> types = new Dictionary<string, SophiaType>();
                        foreach (string n in spl)
                        {
                            string[] sp2 = SplitByTwoPoints(n);
                            if (sp2.Length != 2)
                                throw new ArgumentException($"Unable to record item {n}");
                            string key = sp2[0].Trim();
                            string value = sp2[1].Trim();
                            if (key.StartsWith("\"") && key.EndsWith("\""))
                                key = key.Substring(1, key.Length - 2);
                            if (key.StartsWith("'") && key.EndsWith("'"))
                                key = key.Substring(1, key.Length);
                            types.Add(key, ParseEntry(value, mapper));
                        }

                        return new RecordType(entry, name, types);
                    }
                }

                foreach (string s in mapper.TypeDefs)
                {
                    int id = s.IndexOf(' ');
                    string name = s.Substring(0, id).Trim();
                    if (name == subl)
                    {
                        id = s.IndexOf('=');
                        string grp = s.Substring(id + 1).Trim();
                        SophiaType sp = ParseEntry(grp, mapper);
                        sp.MapName = entry;
                        return sp;
                    }
                }
            }

            throw new ArgumentException($"Unknown type : {entry}");
        }


        public class MapperInfo
        {
            public string ContractName { get; set; }
            public List<string> Records { get; set; } = new List<string>();
            public List<string> TypeDefs { get; set; } = new List<string>();
            public List<string> Events { get; set; } = new List<string>();
            public Dictionary<string, (List<string> inputs, string outputs)> Functions { get; set; } = new Dictionary<string, (List<string> inputs, string outputs)>();
        }
    }
}