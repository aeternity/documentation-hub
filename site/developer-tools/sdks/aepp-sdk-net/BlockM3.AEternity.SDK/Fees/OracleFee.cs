using System;
using System.Numerics;

namespace BlockM3.AEternity.SDK.Fees
{
    public class OracleFee : IFee
    {
        private readonly ulong _relativeTTL;

        public OracleFee(ulong relativeTTL)
        {
            _relativeTTL = relativeTTL;
        }


        public BigInteger Calculate(int txByteSize, long minimalGasPrice)
        {
            BigInteger ttlcost = new BigInteger(Math.Ceiling(32000 * (double) _relativeTTL / Math.Floor((double) 60 * 24 * 365 / Constants.BaseConstants.KEY_BLOCK_INTERVAL)));
            BigInteger fee = (new BigInteger((Constants.BaseConstants.BASE_GAS + txByteSize) * Constants.BaseConstants.GAS_PER_BYTE) + ttlcost) * new BigInteger(minimalGasPrice);
            return fee;
        }
    }
}