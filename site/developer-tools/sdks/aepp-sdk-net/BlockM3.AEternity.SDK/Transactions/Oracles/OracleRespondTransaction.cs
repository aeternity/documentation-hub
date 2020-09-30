using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.Oracles
{
    public class OracleRespondTransaction : Transaction<UnsignedTx, OracleRespondTx>
    {
        internal OracleRespondTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugOracleRespondAsync(Model, token);
        }


        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_ORACLE_RESPONSE_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.OracleId, Constants.SerializationTags.ID_TAG_ORACLE));
            enc.AddNumber(Model.Nonce);
            enc.AddByteArray(Encoding.DecodeCheckWithIdentifier(Model.QueryId));
            enc.AddString(Model.Response);
            enc.AddNumber((int) Model.ResponseTtl.Type);
            enc.AddNumber(Model.ResponseTtl.Value);
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            return enc.Encode();
        }
    }
}