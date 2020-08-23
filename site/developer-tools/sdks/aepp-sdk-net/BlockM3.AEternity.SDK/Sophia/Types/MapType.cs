using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class MapType : SophiaType
    {
        public MapType(string mapname, SophiaType keytype, SophiaType valuetype) : base(mapname)
        {
            KeyType = keytype;
            ValueType = valuetype;
        }

        public override string SophiaBaseName => "map";
        public override string SophiaName => SophiaBaseName + "(" + KeyType.SophiaName + ", " + ValueType.SophiaBaseName + ")";

        public SophiaType KeyType { get; }
        public SophiaType ValueType { get; }

        public override string Serialize(object o, Type t)
        {
            StringBuilder bld = new StringBuilder();
            if (o is IDictionary en)
            {
                bld.Append("{");
                bool add = false;
                foreach (object on in en.Keys)
                {
                    add = true;
                    bld.Append("[");
                    bld.Append(KeyType.Serialize(on, t.GetGenericArguments()[0]));
                    bld.Append("] = ");
                    bld.Append(ValueType.Serialize(en[on], t.GetGenericArguments()[1]));
                    bld.Append(", ");
                }

                if (add)
                    bld.Remove(bld.Length - 2, 2);
                bld.Append("}");
                return bld.ToString();
            }

            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            value = value.Trim();
            if (value.StartsWith("{") && value.EndsWith("}"))
                value = value.Substring(1, value.Length - 2).Trim();
            if (value.StartsWith("[") && value.EndsWith("]"))
                value = value.Substring(1, value.Length - 2).Trim();
            string[] dicsplits = SophiaMapper.SplitByComma(value);
            
            IDictionary dica = (IDictionary) Activator.CreateInstance(t);
            foreach (string s in dicsplits)
            {
                string h = s.Trim();
                if (h.StartsWith("[") && h.EndsWith("]"))
                    h = h.Substring(1, h.Length - 2).Trim();
                string[] dta = SophiaMapper.SplitByComma(h);
                if (dta.Length!=2)
                    throw new ArgumentException($"Unable to parse dictionary item {s}");
                dica.Add(KeyType.Deserialize(dta[0].Trim(), t.GetGenericArguments()[0]), ValueType.Deserialize(dta[1].Trim(), t.GetGenericArguments()[1]));
            }

            return dica;
        }
    }
}