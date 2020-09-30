using System;
using System.Collections;
using System.Text;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class ListType : SophiaType
    {
        public ListType(string mapname, SophiaType keytype) : base(mapname)
        {
            KeyType = keytype;
        }

        public override string SophiaBaseName => "list";
        public override string SophiaName => SophiaBaseName + "(" + KeyType.SophiaName + ")";

        public SophiaType KeyType { get; }

        public override string Serialize(object o, Type t)
        {
            StringBuilder bld = new StringBuilder();
            if (o is IEnumerable en)
            {
                bld.Append("[");
                bool add = false;
                foreach (object on in en)
                {
                    add = true;
                    if (KeyType != null)
                    {
                        bld.Append(KeyType.Serialize(on, t.GetGenericArguments()[0]));
                    }
                    else
                    {
                        SophiaType sp = SophiaMapper.GetSophiaPossibleTypeFromType(t.GetGenericArguments()[0]);
                        bld.Append(sp.Serialize(on, t.GetGenericArguments()[0]));
                    }

                    bld.Append(", ");
                }

                if (add)
                    bld.Remove(bld.Length - 2, 2);
                bld.Append("]");
                return bld.ToString();
            }

            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            value = value.Trim();
            if (value.StartsWith("[") && value.EndsWith("]"))
                value = value.Substring(1, value.Length - 2).Trim();
            string[] items = SophiaMapper.SplitByComma(value);
            IList en = (IList) Activator.CreateInstance(t);
            foreach (string s in items)
            {
                if (KeyType != null)
                    en.Add(KeyType.Deserialize(s, t.GetGenericArguments()[0]));
                else
                {
                    SophiaType sp = SophiaMapper.GetSophiaPossibleTypeFromType(t.GetGenericArguments()[0]);
                    en.Add(sp.Deserialize(s, t.GetGenericArguments()[0]));
                }
            }

            return en;
        }
    }
}