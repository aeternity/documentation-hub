using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Sophia;

namespace BlockM3.AEternity.SDK.Progress
{
    public class WaitForQueryId<T, S> : ITransactionProgress
    {
        public string TXHash => null;

        public async Task<(object result, bool done)> CheckForFinishAsync(object input, CancellationToken token = default(CancellationToken))
        {
            OracleClient<T, S> r = (OracleClient<T, S>) input;


            OracleQuery q = await r.Account.Client.GetOracleAnswerAsync(r.Id, r.QueryId, token).ConfigureAwait(false);
            if (q == null || q.Response == null || q.Response == "or_Xfbg4g==")
                return (null, false);
            string json = Encoding.UTF8.GetString(Utils.Encoding.DecodeCheckWithIdentifier(q.Response));
            return (SophiaMapper.DeserializeOracleClass<S>(json), true);
        }
    }
}