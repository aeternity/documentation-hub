using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.NameService
{
    public class NamePreclaimTransaction : Transaction<UnsignedTx, NamePreclaimTx>
    {
        internal NamePreclaimTransaction(ILoggerFactory factory, FlatClient client, string name) : base(factory, client)
        {
            NameForValidation = name;
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        public string NameForValidation { get; }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugNamePreClaimAsync(Model, token);
        }


        protected override void ValidateInput()
        {
            Validation.CheckNamespace(NameForValidation);
        }

        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_NAME_SERVICE_PRECLAIM_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.AccountId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Nonce);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.CommitmentID, Constants.SerializationTags.ID_TAG_COMMITMENT));
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            return enc.Encode();
        }
    }
}