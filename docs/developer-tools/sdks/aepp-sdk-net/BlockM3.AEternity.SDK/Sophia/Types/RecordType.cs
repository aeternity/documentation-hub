using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BlockM3.AEternity.SDK.Sophia.Attributes;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class RecordType : SophiaType
    {
        public RecordType(string mapname, string name, Dictionary<string, SophiaType> types) : base(mapname)
        {
            Name = name;
            FieldTypes = types;
        }

        public override string SophiaBaseName => "record";
        public override string SophiaName => SophiaBaseName + " " + Name + " {" + string.Join(", ", FieldTypes.Select(a => a.Key + " : " + a.Value.SophiaName)) + "}";

        public Dictionary<string, SophiaType> FieldTypes { get; }
        public string Name { get; }

        public override string Serialize(object o, Type t)
        {
            SophiaRecordAttribute rec = t.GetCustomAttribute<SophiaRecordAttribute>();
            if (!t.IsClass)
                throw new ArgumentException("Record can be only classes");
            if (rec == null)
                throw new ArgumentException("Record classes should have the SophiaRecord Attribute");
            StringBuilder bld = new StringBuilder();
            bld.Append(Name);
            bld.Append("{ ");
            List<(string, PropertyInfo)> props = OrderProps(t.GetTypeInfo());
            bool add = false;
            foreach (string s in FieldTypes.Keys)
            {
                add = true;
                (string, PropertyInfo) prop = props.FirstOrDefault(a => a.Item1 == s);
                if (prop.Item1 == null)
                    throw new ArgumentException($"Unable to serialize record, missing property {s} in class {t.Name}");
                bld.Append(s);
                bld.Append(" = ");
                string res = FieldTypes[s].Serialize(prop.Item2.GetValue(o), prop.Item2.PropertyType);
                bld.Append(res);
                bld.Append(", ");
            }

            if (add)
                bld.Remove(bld.Length - 2, 2);
            bld.Append("}");
            return bld.ToString();
        }

        public override object Deserialize(string value, Type t)
        {
            if (!t.IsClass)
                throw new ArgumentException("Record can be only classes");
            value = value.Trim();
            int idx = value.IndexOf('{');
            if (idx > -1)
                value = value.Substring(idx);
            if (value.StartsWith("{") && value.EndsWith("}"))
                value = value.Substring(1, value.Length - 2).Trim();
            string[] dicsplits = SophiaMapper.SplitByComma(value);
            List<(string, PropertyInfo)> props = OrderProps(t.GetTypeInfo());
            object o = Activator.CreateInstance(t);
            foreach (string s in dicsplits)
            {
                string[] spl = SophiaMapper.SplitByTwoPoints(s);
                if (spl.Length != 2)
                    throw new ArgumentException($"Unable to record item {s}");
                string name = spl[0].Trim();
                if (name.StartsWith("\"") && name.EndsWith("\""))
                    name = name.Substring(1, name.Length - 2);
                (string, PropertyInfo) prop = props.FirstOrDefault(a => a.Item1 == name);
                if (prop.Item1 == null)
                    throw new ArgumentException($"Unable to deserialize record, missing property {name} in class {t.Name}");
                if (!FieldTypes.ContainsKey(name))
                    throw new ArgumentException($"Unable to deserialize record, missing property {name} in record definition {SophiaName}");
                SophiaType tp = FieldTypes[name];
                object ob = tp.Deserialize(spl[1].Trim(), prop.Item2.PropertyType);
                prop.Item2.SetValue(o, ob);
            }

            return o;
        }
    }
}