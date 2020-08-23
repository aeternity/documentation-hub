using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.Oracles
{
    public class OracleQueryTransaction : Transaction<UnsignedTx, OracleQueryTx>
    {
        internal OracleQueryTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }


        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugOracleQueryAsync(Model, token);
        }

        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_ORACLE_QUERY_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.SenderId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Nonce);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.OracleId, Constants.SerializationTags.ID_TAG_ORACLE));
            enc.AddString(Model.Query);
            enc.AddNumber(Model.QueryFee);
            enc.AddNumber((int) Model.QueryTtl.Type);
            enc.AddNumber(Model.QueryTtl.Value);
            enc.AddNumber((int) Model.ResponseTtl.Type);
            enc.AddNumber(Model.ResponseTtl.Value);
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            return enc.Encode();
        }
    }
}