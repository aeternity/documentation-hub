using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Fees;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions
{
/**
 * this abstract class is the base of all transactions and wraps the calculation of transaction fees
 * by using a default fee, generating the RLP and gaining the byte size of the transaction
 *
 * @param <TxModel>
 */
    public abstract class Transaction<T> where T : UnsignedTx
    {
        public abstract BigInteger Fee { get; set; }
        public abstract Task<T> CreateUnsignedTransactionAsync(bool nativeMode, long minimalGasPrice, CancellationToken token = default(CancellationToken));
        public abstract Task<T> CreateUnsignedTransactionAsync(CancellationToken token = default(CancellationToken));
    }

    public abstract class Transaction<T, S> : Transaction<T> where T : UnsignedTx, new() where S : class, new()
    {
        protected readonly FlatClient _client;
        protected readonly ILogger _logger;
        public S Model;

        public Transaction(ILoggerFactory factory, FlatClient client)
        {
            _logger = factory.CreateLogger(GetType());
            _client = client;
        }


        /** fee calculation model for this transaction type, one of {@link FeeCalculationModel} */
        public IFee FeeModel { get; set; } = new BaseFee();

        /**
         * generates a Bytes object from the attributes. this is necessary for calculating the fee based
         * on RLP encoding
         *
         * @return {@link Bytes}
         */
        public abstract byte[] Serialize();

        /**
         * this method needs to be implemented for testing purposes (non native mode)
         *
         * @return a single-wrapped unsignedTx object
         */
        protected abstract Task<T> CreateDebugAsync(CancellationToken token);


        /**
         * this method creates an unsigned transaction whether in native or debug mode if no fee is
         * defined (null), the fee will be calculated based on the transactions byte size and maybe other
         * transaction attributes using the fee calculation model
         *
         * @param nativeMode native or debug mode
         * @param minimalGasPrice the minimal gas price, which the fee is multiplied with *
         * @return a single-wrapped unsignedTx object
         */
        public override Task<T> CreateUnsignedTransactionAsync(bool nativeMode, long minimalGasPrice, CancellationToken token = default(CancellationToken))
        {
            /** before creating the unsigned transaction we validate the input */
            ValidateInput();
            /** if no fee is given - use default fee to create a tx and get its size */
            if (Fee == 0)
            {
                BigInteger calcfee;
                do
                {
                    calcfee = Fee;
                    Fee = FeeModel.Calculate(Serialize().Length, minimalGasPrice);
                } while (calcfee != Fee);

                _logger.LogInformation("Using calculation model {0} the following fee was calculated {1}", FeeModel.GetType().Name, Fee);
            }
            else
            {
                _logger.LogWarning("You defined a custom transaction fee which might be not sufficient to execute the transaction!");
            }

            if (nativeMode)
            {
                /** create final RLP encoded representation of the transaction */
                byte[] encodedRLPArrayWithFee = Serialize();
                T tx = new T();
                tx.TX = Encoding.EncodeCheck(encodedRLPArrayWithFee, Constants.ApiIdentifiers.TRANSACTION);
                return Task.FromResult(tx);
            }

            return CreateDebugAsync(token);
        }

        public override Task<T> CreateUnsignedTransactionAsync(CancellationToken token = default(CancellationToken))
        {
            return CreateUnsignedTransactionAsync(_client.Configuration.NativeMode, _client.Configuration.MinimalGasPrice);
        }

        /** this method can be used to perform transaction specific validations that will */
        protected virtual void ValidateInput()
        {
        }
    }
}