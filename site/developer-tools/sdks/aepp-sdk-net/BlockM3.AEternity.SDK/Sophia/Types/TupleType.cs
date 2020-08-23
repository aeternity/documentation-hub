using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class TupleType : SophiaType
    {
        public TupleType(string mapname, List<SophiaType> types) : base(mapname)
        {
            TupleTypes = types ?? new List<SophiaType>();
        }

        public override string SophiaBaseName => "tuple";
        public override string SophiaName => SophiaBaseName + "(" + string.Join(", ", TupleTypes.Select(a => a.SophiaName)) + ")";

        public List<SophiaType> TupleTypes { get; }

        public override string Serialize(object o, Type t)
        {
            string fullname = t.FullName;
            if (fullname != null && fullname.StartsWith("System.ValueTuple"))
            {
                StringBuilder bld = new StringBuilder();
                bld.Append("(");
                bool add = false;
                int x = 0;
                if (TupleTypes.Count > 0 && TupleTypes.Count != t.GetFields().Length)
                    throw new ArgumentException($"The object has {t.GetFields().Length} in the tuple, the sophia type has {TupleTypes.Count}");
                foreach (FieldInfo f in t.GetFields())
                {
                    add = true;
                    if (TupleTypes.Count > 0)
                        bld.Append(TupleTypes[x].Serialize(f.GetValue(o), f.FieldType));
                    else
                    {
                        SophiaType sp = SophiaMapper.GetSophiaPossibleTypeFromType(f.FieldType);
                        bld.Append(sp.Serialize(f.GetValue(o), f.FieldType));
                    }

                    bld.Append(", ");
                }

                if (add)
                    bld.Remove(bld.Length - 2, 2);
                bld.Append(")");
                return bld.ToString();
            }

            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            string fullname = t.FullName;
            if (fullname != null && fullname.StartsWith("System.ValueTuple"))
            {
                value = value.Trim();
                if (value.StartsWith("[") && value.EndsWith("]"))
                    value = value.Substring(1, value.Length - 2).Trim();
                string[] items = SophiaMapper.SplitByComma(value);
                if (TupleTypes.Count > 0 && TupleTypes.Count != items.Length)
                    throw new ArgumentException($"The object has {t.GetFields().Length} in the tuple, the sophia type has {items.Length}");
                List<object> objs = new List<object>();

                for (int x = 0; x < items.Length; x++)
                {
                    if (TupleTypes.Count > 0)
                    {
                        objs.Add(TupleTypes[x].Deserialize(items[x], t.GetGenericArguments()[x]));
                    }
                    else
                    {
                        SophiaType sp = SophiaMapper.GetSophiaPossibleTypeFromType(t.GetGenericArguments()[x]);
                        objs.Add(sp.Deserialize(items[x], t.GetGenericArguments()[x]));
                    }
                }

                MethodInfo method = typeof(ValueType).GetMethods(BindingFlags.Static | BindingFlags.Public).ToList().First(a => a.Name == "Create" && a.GetParameters().Length == objs.Count);
                return method.Invoke(null, objs.ToArray());
            }

            return base.Deserialize(value, t);
        }
    }
}