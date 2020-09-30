using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Progress;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class Claim : BaseFluent
    {
        internal Claim(string domain, Account account) : base(account)
        {
            Domain = domain;
        }

        public string Domain { get; }
        public ulong NameTtl { get; internal set; }
        public List<(string, string)> Pointers { get; set; }

        public string Id { get; internal set; }

        public async Task<InProgress<Claim>> UpdateAsync(ulong name_ttl = Constants.BaseConstants.NAME_TTL, ulong client_ttl = Constants.BaseConstants.NAME_CLIENT_TTL, CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            await SignAndSendAsync(Account.Client.CreateNameUpdateTransaction(Account.KeyPair.PublicKey, Id, Account.Nonce, Account.Ttl, client_ttl, name_ttl, Pointers.Select(a => new NamePointer() {Key = a.Item1, Id = a.Item2}).ToList()), token).ConfigureAwait(false);
            return new InProgress<Claim>(new WaitForHash(this), new GetNameClaim());
        }

        public async Task<InProgress<bool>> RevokeAsync(CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            await SignAndSendAsync(Account.Client.CreateNameRevokeTransaction(Account.KeyPair.PublicKey, Id, Account.Nonce, Account.Ttl), token).ConfigureAwait(false);
            return new InProgress<bool>(new WaitForHash(this));
        }

        public async Task<InProgress<bool>> TransferAsync(string recipientPublicKey, CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            await SignAndSendAsync(Account.Client.CreateNameTransferTransaction(Account.KeyPair.PublicKey, Id, recipientPublicKey, Account.Nonce, Account.Ttl), token).ConfigureAwait(false);
            return new InProgress<bool>(new WaitForHash(this));
        }
    }
}