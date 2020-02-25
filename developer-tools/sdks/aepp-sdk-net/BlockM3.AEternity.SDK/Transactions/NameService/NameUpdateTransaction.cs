using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

namespace BlockM3.AEternity.SDK.Transactions.NameService
{
    public class NameUpdateTransaction : Transaction<UnsignedTx, NameUpdateTx>
    {
        internal NameUpdateTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            return _client.CreateDebugNameUpdateAsync(Model, token);
        }


        protected override void ValidateInput()
        {
            if (Model.NameId == null)
                throw new InvalidParameterException($"Invalid Parameter NameId: {Validation.PARAMETER_IS_NULL}");
            if (!Model.NameId.StartsWith(Constants.ApiIdentifiers.NAME))
                throw new InvalidParameterException($"Invalid Parameter NameId: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.NAME)}");
        }

        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_NAME_SERVICE_UPDATE_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.AccountId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Nonce);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.NameId, Constants.SerializationTags.ID_TAG_NAME));
            enc.AddNumber(Model.NameTtl);
            RLPEncoder sublist = new RLPEncoder();
            foreach (NamePointer pointer in Model.Pointers)
            {
                sublist.AddString("account_pubkey");
                sublist.AddByteArray(Encoding.DecodeCheckAndTag(pointer.Id, Constants.SerializationTags.ID_TAG_ACCOUNT));
            }

            enc.AddList(sublist);
            enc.AddNumber(Model.ClientTtl);
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            return enc.Encode();
        }
    }
}