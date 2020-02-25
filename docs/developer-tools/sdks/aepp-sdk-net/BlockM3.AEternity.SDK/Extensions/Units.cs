using System.Numerics;
using BlockM3.AEternity.SDK.Models;

namespace BlockM3.AEternity.SDK.Extensions
{
    public static class Units
    {
        public static decimal FromAettos(this string number, Unit unit)
        {
            return FromAettos(decimal.Parse(number), unit);
        }

        public static decimal FromAettos(this BigInteger number, Unit unit)
        {
            return FromAettos((decimal) number, unit);
        }

        public static decimal FromAettos(this decimal number, Unit unit)
        {
            return number / unit.AettosFactor;
        }


        public static decimal ToAettos(this string number, Unit unit)
        {
            return ToAettos(decimal.Parse(number), unit);
        }

        public static decimal ToAettos(this decimal number, Unit unit)
        {
            return number * unit.AettosFactor;
        }
    }
}