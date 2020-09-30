using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Progress;

namespace BlockM3.AEternity.SDK.ClientModels
{
    //T Input Format
    //S Output Format
    public class OracleServer<T, S> : BaseFluent
    {
        internal OracleServer(Account account) : base(account)
        {
            OracleId = Constants.ApiIdentifiers.ORACLE_PUBKEY + "_" + account.KeyPair.PublicKey.Substring(3);
        }

        public string OracleId { get; }


        public string QueryFormat { get; internal set; }

        public string ResponseFormat { get; internal set; }

        public BigInteger QueryFee { get; internal set; }

        public ulong Ttl { get; internal set; }

        public ushort AbiVersion { get; internal set; }

        public async Task<List<OracleQuestion<T, S>>> QueryAsync(ushort? limit, string lastQueryId = null, CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            OracleQueries queries = await Account.Client.GetOracleQueriesAsync(OracleId, lastQueryId, limit, Type.Open, token).ConfigureAwait(false);
            if (queries.OracleQueriesCollection == null || queries.OracleQueriesCollection.Count == 0)
                return new List<OracleQuestion<T, S>>();
            return queries.OracleQueriesCollection.Select(a => new OracleQuestion<T, S>(Account, a)).ToList();
        }


        public async Task<InProgress<bool>> ExtendAsync(ulong fee = Constants.BaseConstants.FEE, ulong extendTtl = Constants.BaseConstants.ORACLE_RESPONSE_TTL_VALUE, CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            await SignAndSendAsync(Account.Client.CreateOracleExtendTransaction(OracleId, extendTtl, fee, Account.Nonce, Account.Ttl), token).ConfigureAwait(false);
            return new InProgress<bool>(new WaitForHash(this));
        }

        public InProgress<bool> CreateOracleExtendProgressFromTx(string txhash)
        {
            TxHash = txhash;
            return new InProgress<bool>(new WaitForHash(this));
        }
    }
}