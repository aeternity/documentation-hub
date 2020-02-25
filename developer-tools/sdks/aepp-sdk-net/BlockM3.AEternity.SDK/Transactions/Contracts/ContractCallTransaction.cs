using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.Contracts
{
    public class ContractCallTransaction : Transaction<UnsignedTx, ContractCallTx>
    {
        internal ContractCallTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugContractCallAsync(Model, token);
        }


        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_CONTRACT_CALL_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.CallerId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Nonce);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.ContractId, Constants.SerializationTags.ID_TAG_CONTRACT));
            enc.AddNumber(Model.AbiVersion);
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            enc.AddNumber(Model.Amount);
            enc.AddNumber(Model.Gas);
            enc.AddNumber(Model.GasPrice);
            enc.AddByteArray(Encoding.DecodeCheckWithIdentifier(Model.CallData));
            return enc.Encode();
        }
    }
}