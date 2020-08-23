using System;
using System.Globalization;
using System.Numerics;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class BitsType : SophiaType
    {
        public BitsType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "bits";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (!(o is BigInteger))
                throw new ArgumentException($"Bits parameter should be a BigInteger");
            BigInteger n = (BigInteger) o;
            if (n == 0)
                return "Bits.none";
            if (n >= BigInteger.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", NumberStyles.HexNumber))
                return "Bits.all";
            return o.ToString();
        }

        public override object Deserialize(string value, Type t)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            if (value.ToLowerInvariant() == "bits.none")
                return new BigInteger(0);
            if (value.ToLowerInvariant() == "bits.all")
                return BigInteger.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", NumberStyles.HexNumber);
            return BigInteger.Parse(value);
        }
    }
}