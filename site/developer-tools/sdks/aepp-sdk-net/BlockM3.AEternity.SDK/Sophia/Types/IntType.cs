using System;
using System.Numerics;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class IntType : SophiaType
    {
        public IntType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "int";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o is byte || o is ushort || o is short || o is int || o is uint || o is long || o is ulong || o is BigInteger)
                return o.ToString();
            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            if (typeof(BigInteger).IsAssignableFrom(t))
            {
                return BigInteger.Parse(value);
            }

            if (typeof(ulong).IsAssignableFrom(t))
            {
                return ulong.Parse(value);
            }

            if (typeof(long).IsAssignableFrom(t))
            {
                return long.Parse(value);
            }

            if (typeof(uint).IsAssignableFrom(t))
            {
                return uint.Parse(value);
            }

            if (typeof(int).IsAssignableFrom(t))
            {
                return int.Parse(value);
            }

            if (typeof(ushort).IsAssignableFrom(t))
            {
                return ushort.Parse(value);
            }

            if (typeof(short).IsAssignableFrom(t))
            {
                return short.Parse(value);
            }

            if (typeof(byte).IsAssignableFrom(t))
            {
                return byte.Parse(value);
            }

            return base.Deserialize(value, t);
        }

        public override object FromBigInteger(BigInteger b)
        {
            return b;
        }
    }
}