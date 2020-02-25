using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Generated.Models;

namespace BlockM3.AEternity.SDK.Progress
{
    public class WaitForHash : ITransactionProgress
    {
        private readonly BaseFluent _ret;

        internal WaitForHash(BaseFluent ret)
        {
            _ret = ret;
        }

        public string TXHash => _ret.TxHash;

        public async Task<(object result, bool done)> CheckForFinishAsync(object input, CancellationToken token = default(CancellationToken))
        {
            GenericSignedTx tx = await _ret.Account.Client.GetTransactionByHashAsync(_ret.TxHash, token).ConfigureAwait(false);
            if (tx.BlockHeight == -1)
                return (null, false);
            _ret.Tx = tx;
            return (_ret, true);
        }
    }
}