using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Security.KeyPairs;

namespace BlockM3.AEternity.SDK
{
    public class Client
    {
        public Client(Configuration cfg)
        {
            FlatClient = new FlatClient(cfg);
        }

        public FlatClient FlatClient { get; set; }

        public Task<Account> ConstructAccountAsync(BaseKeyPair keypair, CancellationToken token = default(CancellationToken)) => Account.CreateAsync(FlatClient, keypair, token);
        public Task<Account> ConstructAccountAsync(string publickey, string privatekey, CancellationToken token = default(CancellationToken)) => Account.CreateAsync(FlatClient, new BaseKeyPair(publickey, privatekey), token);
        public Task<Account> ConstructAccountAsync(string publickey, CancellationToken token = default(CancellationToken)) => Account.CreateAsync(FlatClient, new BaseKeyPair(publickey, string.Empty), token);

        public Account ConstructAnonymousAccount() => Account.Create(FlatClient, null, null);
        public Account ConstructAccount(Generated.Models.Account account, BaseKeyPair keypair) => Account.Create(FlatClient, account, keypair);
    }
}