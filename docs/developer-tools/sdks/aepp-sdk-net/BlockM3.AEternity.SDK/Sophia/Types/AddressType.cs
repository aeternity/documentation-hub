using System;
using System.Numerics;
using BlockM3.AEternity.SDK.Utils;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class AddressType : SophiaType
    {
        public AddressType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "address";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o is string s)
            {
                if (s.StartsWith("\"") && s.EndsWith("\""))
                    s = s.Substring(1, s.Length - 2);
                if (!s.StartsWith(Constants.ApiIdentifiers.ACCOUNT_PUBKEY + "_"))
                    throw new ArgumentException($"Invalid address, all addresses should start with '{Constants.ApiIdentifiers.ACCOUNT_PUBKEY}' value: '{s}'");
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
                if (!value.StartsWith(Constants.ApiIdentifiers.ACCOUNT_PUBKEY + "_"))
                    throw new ArgumentException($"Invalid address, all addresses should start with '{Constants.ApiIdentifiers.ACCOUNT_PUBKEY}' value: '{value}'");
                return value;
            }

            return base.Deserialize(value, t);
        }

        public override object FromBigInteger(BigInteger b)
        {
            byte[] data = BigIntegerToByteArray(b);
            return Encoding.EncodeCheck(data, Constants.ApiIdentifiers.ACCOUNT_PUBKEY);
        }
    }
}