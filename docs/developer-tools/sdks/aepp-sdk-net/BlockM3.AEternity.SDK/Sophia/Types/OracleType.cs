using System;
using System.Numerics;
using BlockM3.AEternity.SDK.Utils;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class OracleType : SophiaType
    {
        public OracleType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "oracle('a, 'b)";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o is string s)
            {
                if (s.StartsWith("\"") && s.EndsWith("\""))
                    s = s.Substring(1, s.Length - 2);
                if (!s.StartsWith(Constants.ApiIdentifiers.ORACLE_PUBKEY + "_"))
                    throw new ArgumentException($"Invalid oracle, all oracles should start with '{Constants.ApiIdentifiers.ORACLE_PUBKEY}' value: '{s}'");
                return s;
            }

            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            if (typeof(string) == t)
            {
                if (!value.StartsWith(Constants.ApiIdentifiers.ORACLE_PUBKEY + "_"))
                    throw new ArgumentException($"Invalid oracle, all oracles should start with '{Constants.ApiIdentifiers.ORACLE_PUBKEY}' value: '{value}'");
                return value;
            }

            return base.Deserialize(value, t);
        }

        public override object FromBigInteger(BigInteger b)
        {
            byte[] data = BigIntegerToByteArray(b);
            return Encoding.EncodeCheck(data, Constants.ApiIdentifiers.ORACLE_PUBKEY);
        }
    }
}