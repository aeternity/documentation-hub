using System;
using System.Numerics;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class BoolType : SophiaType
    {
        public BoolType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "bool";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o is bool s)
            {
                return s ? "true" : "false";
            }

            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            if (typeof(bool) == t)
            {
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                return bool.Parse(value);
            }

            return base.Deserialize(value, t);
        }

        public override object FromBigInteger(BigInteger b)
        {
            return b != 0;
        }
    }
}