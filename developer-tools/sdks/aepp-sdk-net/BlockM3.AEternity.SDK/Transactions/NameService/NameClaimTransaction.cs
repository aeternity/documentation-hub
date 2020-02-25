using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;

namespace BlockM3.AEternity.SDK.Transactions.NameService
{
    public class NameClaimTransaction : Transaction<UnsignedTx, NameClaimTx>
    {
        internal NameClaimTransaction(ILoggerFactory factory, FlatClient client) : base(factory, client)
        {
        }

        public override BigInteger Fee
        {
            get => Model.Fee;
            set => Model.Fee = value;
        }

        protected override Task<UnsignedTx> CreateDebugAsync(CancellationToken token)
        {
            NameClaimTx tx = new NameClaimTx
            {
                AccountId = Model.AccountId,
                Fee = Model.Fee,
                NameSalt = Model.NameSalt,
                Nonce = Model.Nonce,
                Ttl = Model.Ttl,
                Name = Encoding.EncodeCheck(System.Text.Encoding.UTF8.GetBytes(Model.Name), Constants.ApiIdentifiers.NAME)
            };
            return _client.CreateDebugNameClaimAsync(tx, token);
        }


        public override byte[] Serialize()
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_NAME_SERVICE_CLAIM_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            enc.AddByteArray(Encoding.DecodeCheckAndTag(Model.AccountId, Constants.SerializationTags.ID_TAG_ACCOUNT));
            enc.AddNumber(Model.Nonce);
            enc.AddString(Model.Name);
            enc.AddNumber(Model.NameSalt);
            enc.AddNumber(Model.Fee);
            enc.AddNumber(Model.Ttl);
            return enc.Encode();
        }
    }
}