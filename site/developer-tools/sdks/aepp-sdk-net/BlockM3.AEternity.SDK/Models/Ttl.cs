using BlockM3.AEternity.SDK.Generated.Models;

namespace BlockM3.AEternity.SDK.Models
{
    public class Ttl
    {
        public Ttl(TTLType type, ulong value)
        {
            Type = type;
            Value = value;
        }

        public Ttl()
        {
            Type = TTLType.Delta;
            Value = Constants.BaseConstants.ORACLE_TTL_VALUE;
        }

        public TTLType Type { get; }
        public ulong Value { get; set; }
    }
}