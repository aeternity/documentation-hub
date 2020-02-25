using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.Channels
{
    public class ChannelDepositTransaction : Transaction<UnsignedTx, ChannelDepositTx>
    {
        internal ChannelDepositTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugChannelDepositAsync(Model, token);
        }


        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_CHANNEL_DEPOSIT_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.ChannelId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.FromId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Amount);
            enc.AddNumber(Model.Ttl);
            enc.AddNumber(Model.Fee);
            enc.AddString(Model.StateHash);
            enc.AddNumber(Model.Round);
            enc.AddNumber(Model.Nonce);
            return enc.Encode();
        }
    }
}