using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Progress;
using BlockM3.AEternity.SDK.Sophia;
using BlockM3.AEternity.SDK.Utils;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class OracleClient<T, S> : BaseFluent
    {
        internal OracleClient(Account account) : base(account)
        {
        }

        public string Id { get; set; }

        public string QueryFormat { get; set; }

        public string ResponseFormat { get; set; }

        public BigInteger QueryFee { get; set; }

        public ulong Ttl { get; set; }

        public ushort AbiVersion { get; set; }

        public string QueryId { get; set; }

        public async Task<InProgress<S>> AskAsync(T query, ulong fee = Constants.BaseConstants.ORACLE_QUERY_FEE, TTLType queryTtlType = TTLType.Delta, ulong queryTtl = Constants.BaseConstants.ORACLE_QUERY_TTL_VALUE, ulong responseRelativeTtl = Constants.BaseConstants.ORACLE_RESPONSE_TTL_VALUE, CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            await SignAndSendAsync(Account.Client.CreateOracleQueryTransaction(SophiaMapper.SerializeOracleClass(query), Account.KeyPair.PublicKey, Id, QueryFee, fee, queryTtlType, queryTtl, responseRelativeTtl, Account.Nonce, Account.Ttl), token).ConfigureAwait(false);
            QueryId = Encoding.EncodeQueryId(Account.KeyPair.PublicKey, Id, Account.Nonce);
            return new InProgress<S>(new WaitForHash(this), new WaitForQueryId<T, S>());
        }
    }
}