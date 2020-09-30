using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Progress;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Sophia;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class Account
    {
        private Account()
        {
        }

        public ulong Nonce { get; set; }

        public BigInteger Balance { get; set; }

        public BaseKeyPair KeyPair { get; set; }

        public FlatClient Client { get; set; }

        public ulong Ttl { get; set; } = Constants.BaseConstants.TX_TTL;

        public bool IsAnonymous => KeyPair?.PublicKey == null;

        public bool HasPrivateKey => KeyPair?.PrivateKey != null;

        internal static async Task<Account> CreateAsync(FlatClient client, BaseKeyPair keypair, CancellationToken token = default(CancellationToken))
        {
            Account fac = new Account();
            Generated.Models.Account ac = null;
            try
            {
                ac = await client.GetAccountAsync(keypair.PublicKey, token).ConfigureAwait(false);
            }
            catch (ApiException<Error> e)
            {
                if (!e.Result.Reason.ToLowerInvariant().Contains("not found"))
                    throw;
            }

            fac.Nonce = ac?.Nonce ?? 0;
            fac.Balance = ac?.Balance ?? 0;
            fac.KeyPair = keypair;
            fac.Client = client;
            return fac;
        }

        internal static Account Create(FlatClient client, Generated.Models.Account account, BaseKeyPair keypair)
        {
            Account fac = new Account();
            fac.Nonce = account?.Nonce ?? 0;
            fac.Balance = account?.Balance ?? 0;
            fac.KeyPair = keypair;
            fac.Client = client;
            return fac;
        }

        public async Task RefreshAsync(CancellationToken token = default(CancellationToken))
        {
            Generated.Models.Account ac;
            try
            {
                ac = await Client.GetAccountAsync(KeyPair.PublicKey, token).ConfigureAwait(false);
            }
            catch (ApiException<Error> e)
            {
                if (!e.Result.Reason.ToLowerInvariant().Contains("not found"))
                    throw;
                return;
            }

            Nonce = ac.Nonce;
            Balance = ac.Balance;
        }

        internal void ValidatePrivateKey()
        {
            if (KeyPair?.PrivateKey == null)
                throw new ArgumentException("Cannot call this method from an Account without a Private Key");
        }

        internal void ValidatePublicKey()
        {
            if (KeyPair?.PublicKey == null)
                throw new ArgumentException("Cannot call this method from an Anonymous Account without a Public Key");
        }

        public async Task<InProgress<PreClaim>> PreClaimDomainAsync(string domain, CancellationToken token = default(CancellationToken))
        {
            ValidatePrivateKey();
            PreClaim c = new PreClaim(domain, this);
            Nonce++;
            await c.SignAndSendAsync(Client.CreateNamePreClaimTransaction(KeyPair.PublicKey, domain, c.Salt, Nonce, Ttl), token).ConfigureAwait(false);
            return new InProgress<PreClaim>(new WaitForHash(c));
        }

        public async Task<Claim> QueryDomainAsync(string domain, CancellationToken token = default(CancellationToken))
        {
            Claim cl = new Claim(domain, this);
            NameEntry ne = await Client.GetNameIdAsync(domain, token).ConfigureAwait(false);
            cl.Pointers = ne.Pointers.Select(a => (a.Key, a.Id)).ToList();
            cl.NameTtl = ne.Ttl;
            cl.Id = ne.Id;
            return cl;
        }

        public async Task<InProgress<bool>> SendAmountAsync(string recipientPublicKey, BigInteger amount, string payload = "", CancellationToken token = default(CancellationToken))
        {
            if (payload == null)
                payload = "";
            ValidatePrivateKey();
            BaseFluent c = new BaseFluent(this);
            Nonce++;
            await c.SignAndSendAsync(Client.CreateSpendTransaction(KeyPair.PublicKey, recipientPublicKey, amount, payload, Ttl, Nonce), token).ConfigureAwait(false);
            return new InProgress<bool>(new WaitForHash(c));
        }

        public Task<Contract> ConstructContractWithContractIdAsync(string sourcecode, string bytecode, string contractId, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION, CancellationToken token = default(CancellationToken)) => Contract.CreateAsync(this, sourcecode, bytecode, contractId, vmVersion, abiVersion, token);
        public Task<Contract> ConstructContractWithContractIdAsync(string sourcecode, string contractId, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION, CancellationToken token = default(CancellationToken)) => Contract.CreateAsync(this, sourcecode, null, contractId, vmVersion, abiVersion, token);
        public Task<Contract> ConstructContractAsync(string sourcecode, string bytecode, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION, CancellationToken token = default(CancellationToken)) => Contract.CreateAsync(this, sourcecode, bytecode, null, vmVersion, abiVersion, token);
        public Task<Contract> ConstructContractAsync(string sourcecode, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION, CancellationToken token = default(CancellationToken)) => Contract.CreateAsync(this, sourcecode, null, null, vmVersion, abiVersion, token);

        public InProgress<bool> CreateBoolProgressFromTx(string txhash)
        {
            BaseFluent fl = new BaseFluent(this);
            fl.TxHash = txhash;
            return new InProgress<bool>(new WaitForHash(fl));
        }

        public InProgress<Claim> CreateClaimProgressFromTx(string domain, string txhash)
        {
            Claim cl = new Claim(domain, this);
            cl.TxHash = txhash;
            return new InProgress<Claim>(new WaitForHash(cl));
        }

        public InProgress<PreClaim> CreatePreClaimProgressFromTx(string domain, string txhash)
        {
            PreClaim cl = new PreClaim(domain, this);
            cl.TxHash = txhash;
            return new InProgress<PreClaim>(new WaitForHash(cl));
        }

        public async Task<InProgress<OracleServer<T, S>>> RegisterOracleAsync<T, S>(ulong queryFee = Constants.BaseConstants.ORACLE_QUERY_FEE, ulong fee = Constants.BaseConstants.FEE, Ttl ttl = default(Ttl), ushort abiVersion = Constants.BaseConstants.ORACLE_VM_VERSION, CancellationToken token = default(CancellationToken))
        {
            ValidatePrivateKey();
            string queryformat = SophiaMapper.ClassToOracleFormat<T>();
            string responseformat = SophiaMapper.ClassToOracleFormat<S>();
            OracleServer<T, S> c = new OracleServer<T, S>(this);
            Nonce++;
            await c.SignAndSendAsync(Client.CreateOracleRegisterTransaction(queryformat, responseformat, KeyPair.PublicKey, queryFee, fee, ttl?.Type ?? TTLType.Delta, ttl?.Value ?? Constants.BaseConstants.ORACLE_TTL_VALUE, abiVersion, Nonce, Ttl), token).ConfigureAwait(false);
            return new InProgress<OracleServer<T, S>>(new WaitForHash(c), new GetOracle<T, S>());
        }

        public async Task<OracleServer<T, S>> GetOwnOracleAsync<T, S>(CancellationToken token = default(CancellationToken))
        {
            ValidatePrivateKey();
            OracleServer<T, S> c = new OracleServer<T, S>(this);
            RegisteredOracle oracle = await Client.GetRegisteredOracleAsync(c.OracleId, token).ConfigureAwait(false);
            c.AbiVersion = oracle.AbiVersion;
            c.QueryFee = oracle.QueryFee;
            c.QueryFormat = oracle.QueryFormat;
            c.ResponseFormat = oracle.ResponseFormat;
            c.Ttl = oracle.Ttl;
            return c;
        }

        public InProgress<OracleServer<T, S>> CreateOracleQueryProgressFromTx<T, S>(string txhash)
        {
            OracleServer<T, S> c = new OracleServer<T, S>(this);
            c.TxHash = txhash;
            return new InProgress<OracleServer<T, S>>(new WaitForHash(c));
        }

        public async Task<OracleClient<T, S>> GetOracleAsync<T, S>(string oraclepubkey, CancellationToken token = default(CancellationToken))
        {
            RegisteredOracle oracle = await Client.GetRegisteredOracleAsync(oraclepubkey, token).ConfigureAwait(false);
            OracleClient<T, S> re = new OracleClient<T, S>(this);
            re.AbiVersion = oracle.AbiVersion;
            re.Id = oracle.Id;
            re.QueryFee = oracle.QueryFee;
            re.QueryFormat = oracle.QueryFormat;
            re.ResponseFormat = oracle.ResponseFormat;
            re.Ttl = oracle.Ttl;
            return re;
        }
    }
}