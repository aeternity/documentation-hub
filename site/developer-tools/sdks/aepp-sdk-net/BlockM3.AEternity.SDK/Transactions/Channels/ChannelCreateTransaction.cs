using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.Channels
{
    public class ChannelCreateTransaction : Transaction<UnsignedTx, ChannelCreateTx>
    {
        internal ChannelCreateTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugChannelCreateAsync(Model, token);
        }


        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_CHANNEL_CREATE_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.InitiatorId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.InitiatorAmount);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.ResponderId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.ResponderAmount);
            enc.AddNumber(Model.PushAmount);
            enc.AddNumber(Model.ChannelReserve);
            enc.AddNumber(Model.LockPeriod);
            enc.AddNumber(Model.Ttl);
            enc.AddNumber(Model.Fee);
            RLPEncoder sublist = new RLPEncoder();
            foreach (string id in Model.DelegateIds)
                sublist.AddByteArray(Encoding.DecodeCheckAndTag(id, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddList(sublist);
            enc.AddString(Model.StateHash);
            enc.AddNumber(Model.Nonce);
            return enc.Encode();
        }
    }
}