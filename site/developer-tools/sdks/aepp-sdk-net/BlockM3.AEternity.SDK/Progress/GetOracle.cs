using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Generated.Models;

namespace BlockM3.AEternity.SDK.Progress
{
    public class GetOracle<T, S> : ITransactionProgress
    {
        public string TXHash => null;

        public async Task<(object result, bool done)> CheckForFinishAsync(object input, CancellationToken token = default(CancellationToken))
        {
            OracleServer<T, S> c = (OracleServer<T, S>) input;
            RegisteredOracle oracle = await c.Account.Client.GetRegisteredOracleAsync(c.OracleId, token).ConfigureAwait(false);
            c.AbiVersion = oracle.AbiVersion;
            c.QueryFee = oracle.QueryFee;
            c.QueryFormat = oracle.QueryFormat;
            c.ResponseFormat = oracle.ResponseFormat;
            c.Ttl = oracle.Ttl;
            return (c, true);
        }
    }
}