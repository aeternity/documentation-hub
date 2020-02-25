using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.Oracles
{
    public class OracleRegisterTransaction : Transaction<UnsignedTx, OracleRegisterTx>
    {
        internal OracleRegisterTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugOracleRegisterAsync(Model, token);
        }

        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_ORACLE_REGISTER_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.AccountId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Nonce);
            enc.AddString(Model.QueryFormat);
            enc.AddString(Model.ResponseFormat);
            enc.AddNumber(Model.QueryFee);
            enc.AddNumber((int) Model.OracleTtl.Type);
            enc.AddNumber(Model.OracleTtl.Value);
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            enc.AddNumber(Model.AbiVersion);
            return enc.Encode();
        }
    }
}