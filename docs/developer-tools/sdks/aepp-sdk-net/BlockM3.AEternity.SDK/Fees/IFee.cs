using System.Numerics;

namespace BlockM3.AEternity.SDK.Fees
{
    public interface IFee
    {
        /**
         * Calculates the fee based on the calculation model if the calculation needs informations from
         * the transaction object, they need to be passed via constructor of the fee calculation model
         * implementation class
         *
         * @param tx_byte_size transaction size in bytes
         * @param minimalGasPrice minimal gas price
         * @return the actual fee
         */
        BigInteger Calculate(int txByteSize, long minimalGasPrice);
    }
}