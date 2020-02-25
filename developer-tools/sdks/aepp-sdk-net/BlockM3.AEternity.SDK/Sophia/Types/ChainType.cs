using System;
using System.Text.RegularExpressions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using Type = System.Type;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class ChainType : SophiaType
    {
        public ChainType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "Chain.ttl";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o is Ttl ft)
            {
                switch (ft.Type)
                {
                    case TTLType.Block:
                        return "FixedTTL(" + ft.Value + ")";
                    case TTLType.Delta:
                        return "RelativeTTL(" + ft.Value + ")";
                }
            }

            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            if (t == typeof(Ttl))
            {

                if (value == null)
                    value = "Empty";
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                if (!value.StartsWith("fixedttl") && !value.StartsWith("relativettl"))
                    throw new ArgumentException($"Expecting FixedTtl or RelativeTtl, Obtained {value}");
                Match m = numReg.Match(value);
                if (m.Success)
                    return value.StartsWith("fixedttl") ? new Ttl(TTLType.Delta, ulong.Parse(m.Groups[1].Value)) : new Ttl(TTLType.Block, ulong.Parse(m.Groups[1].Value));
            }

            return base.Deserialize(value, t);
        }
    }
}