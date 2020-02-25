using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Exceptions;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Transactions;
using BlockM3.AEternity.SDK.Utils;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class BaseFluent
    {
        internal BaseFluent(Account account)
        {
            Account = account;
        }

        public Account Account { get; private set; }
        public GenericSignedTx Tx { get; set; }
        public string TxHash { get; set; }

        internal async Task SignAndSendAsync<T>(Transaction<T> trans, CancellationToken token) where T : UnsignedTx
        {
            T tx = await trans.CreateUnsignedTransactionAsync(token).ConfigureAwait(false);
            if (trans.Fee > Account.Balance)
                throw new InsufficientFundsException($"Required: {trans.Fee.FromAettos(Unit.AE)} {Unit.AE.Name} Actual: {Account.Balance.FromAettos(Unit.AE)} {Unit.AE.Name}");
            Tx signed = Account.Client.SignTransaction(tx, Account.KeyPair.PrivateKey);
            PostTxResponse resp = await Account.Client.PostTransactionAsync(signed, token).ConfigureAwait(false);
            string comp = Encoding.ComputeTxHash(signed.TX);
            if (comp != resp.TXHash)
                throw new TransactionHashMismatchException($"Response Transaction Hash Mismatch Expected: {comp} Resulted: {resp.TXHash}");
            TxHash = resp.TXHash;
        }
    }
}