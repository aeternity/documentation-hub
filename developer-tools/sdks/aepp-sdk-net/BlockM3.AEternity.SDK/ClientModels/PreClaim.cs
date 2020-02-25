using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Progress;
using BlockM3.AEternity.SDK.Utils;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class PreClaim : BaseFluent
    {
        internal PreClaim(string domain, Account account) : base(account)
        {
            Salt = Crypto.GenerateNamespaceSalt();
            Domain = domain;
        }

        public string Domain { get; }

        public BigInteger Salt { get; }

        public async Task<InProgress<Claim>> ClaimDomainAsync(CancellationToken token = default(CancellationToken))
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            Claim cl = new Claim(Domain, Account);
            await cl.SignAndSendAsync(Account.Client.CreateNameClaimTransaction(Account.KeyPair.PublicKey, Domain, Salt, Account.Nonce, Account.Ttl), token).ConfigureAwait(false);
            return new InProgress<Claim>(new WaitForHash(cl), new GetNameClaim());
        }
    }
}