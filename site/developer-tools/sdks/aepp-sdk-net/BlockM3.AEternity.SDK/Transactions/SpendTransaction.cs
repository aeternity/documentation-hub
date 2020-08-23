using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions
{
    public class SpendTransaction : Transaction<UnsignedTx, SpendTx>
    {
        internal SpendTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugSpendAsync(Model, token);
        }

        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_SPEND_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.SenderId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.RecipientId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Amount);
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            enc.AddNumber(Model.Nonce);
            enc.AddString(Model.Payload);
            return enc.Encode();
        }
    }
}