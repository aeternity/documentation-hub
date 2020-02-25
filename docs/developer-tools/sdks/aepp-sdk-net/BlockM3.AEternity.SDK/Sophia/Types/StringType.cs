using System;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class StringType : SophiaType
    {
        public StringType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "string";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o is string s)
                return "\"" + s + "\"";
            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            if (typeof(string) == t)
            {
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                return value;
            }

            return base.Deserialize(value, t);
        }
    }
}