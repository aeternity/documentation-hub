using System.Numerics;

namespace BlockM3.AEternity.SDK.Fees
{
    public class ContractCreateFee : IFee
    {
        /**
         * the fee is calculated according to the following formula
         *
         * <p>(BASE_GAS * 5 + (byte_size * GAS_PER_BYTE)) * MINIMAL_GAS_PRICE
         */

        public BigInteger Calculate(int txByteSize, long minimalGasPrice)
        {
            return new BigInteger(5 * Constants.BaseConstants.BASE_GAS + txByteSize * Constants.BaseConstants.GAS_PER_BYTE) * new BigInteger(minimalGasPrice);
        }
    }
}