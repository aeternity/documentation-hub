using System;
using System.Numerics;
using BlockM3.AEternity.SDK.Utils;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class ContractType : SophiaType
    {
        public ContractType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "contract";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o is string s)
            {
                if (s.StartsWith("\"") && s.EndsWith("\""))
                    s = s.Substring(1, s.Length - 2);
                if (!s.StartsWith(Constants.ApiIdentifiers.CONTRACT_PUBKEY + "_"))
                    throw new ArgumentException($"Invalid contract, all contracts should start with '{Constants.ApiIdentifiers.CONTRACT_PUBKEY}' value: '{s}'");
                return s;
            }

            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            if (typeof(string) == t)
            {
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                if (!value.StartsWith(Constants.ApiIdentifiers.CONTRACT_PUBKEY + "_"))
                    throw new ArgumentException($"Invalid contract, all contracts should start with '{Constants.ApiIdentifiers.CONTRACT_PUBKEY}' value: '{value}'");
                return value;
            }

            return base.Deserialize(value, t);
        }

        public override object FromBigInteger(BigInteger b)
        {
            byte[] data = BigIntegerToByteArray(b);
            return Encoding.EncodeCheck(data, Constants.ApiIdentifiers.CONTRACT_PUBKEY);
        }
    }
}