using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Progress;
using BlockM3.AEternity.SDK.Sophia;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class OracleQuestion<T, S> : BaseFluent
    {
        internal OracleQuestion(Account account, OracleQuery q) : base(account)
        {
            Ttl = q.Ttl;
            SenderNonce = q.SenderNonce;
            Fee = q.Fee;
            ResponseTtl = q.ResponseTtl;
            OracleId = q.OracleId;
            Id = q.Id;
            SenderId = q.SenderId;
            Query = SophiaMapper.DeserializeOracleClass<T>(Encoding.UTF8.GetString(Utils.Encoding.DecodeCheckWithIdentifier(q.Query)));
        }

        public string Id { get; }
        public T Query { get; }
        public string SenderId { get; set; }
        public ulong SenderNonce { get; set; }
        public string OracleId { get; set; }
        public ulong Ttl { get; set; }
        public TTL ResponseTtl { get; set; }
        public BigInteger Fee { get; set; }

        public async Task<InProgress<bool>> RespondAsync(S answer, ulong respondTtl = Constants.BaseConstants.ORACLE_RESPONSE_TTL_VALUE, CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            string response = SophiaMapper.SerializeOracleClass(answer);
            await SignAndSendAsync(Account.Client.CreateOracleRespondTransaction(response, OracleId, Id, respondTtl, Fee, Account.Nonce, Account.Ttl), token).ConfigureAwait(false);
            return new InProgress<bool>(new WaitForHash(this));
        }

        public InProgress<bool> CreateOracleRespondProgressFromTx(string txhash)
        {
            TxHash = txhash;
            return new InProgress<bool>(new WaitForHash(this));
        }
    }
}