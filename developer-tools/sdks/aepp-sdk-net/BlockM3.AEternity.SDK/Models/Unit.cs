using System;

namespace BlockM3.AEternity.SDK.Models
{
    public class Unit
    {
        public static Unit AETTOS = new Unit("ættos", 0);
        public static Unit AE = new Unit("AE", 18);

        public Unit(string name, int factor)
        {
            Name = name;
            AettosFactor = Convert.ToDecimal(Math.Pow(10, factor));
        }

        public string Name { get; }
        public decimal AettosFactor { get; }

        public override string ToString() => Name;

        public static Unit FromString(string name)
        {
            if (name.Equals(AETTOS.Name, StringComparison.InvariantCultureIgnoreCase))
                return AETTOS;
            return AE;
        }
    }
}