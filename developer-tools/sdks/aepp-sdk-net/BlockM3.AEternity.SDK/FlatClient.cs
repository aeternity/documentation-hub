using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Fees;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Security.Store;
using BlockM3.AEternity.SDK.Transactions;
using BlockM3.AEternity.SDK.Transactions.Channels;
using BlockM3.AEternity.SDK.Transactions.Contracts;
using BlockM3.AEternity.SDK.Transactions.NameService;
using BlockM3.AEternity.SDK.Transactions.Oracles;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Type = BlockM3.AEternity.SDK.Generated.Models.Type;

namespace BlockM3.AEternity.SDK
{
    public class FlatClient
    {
        private readonly ILoggerFactory _factory;
        private readonly ILogger _logger;

        public FlatClient(Configuration config)
        {
            Configuration = config;
            _factory = config.GetLoggerFactory();
            _logger = _factory.CreateLogger<FlatClient>();
            _apiClient = config.GetApiClient();
            _compilerClient = config.GetCompilerApiClient();
        }

        private string _backend = null;


        internal Configuration Configuration { get; }

        private Generated.Api.Client _apiClient { get; }

        private Generated.Compiler.Client _compilerClient { get; }

        //Account

        public Task<Account> GetAccountAsync(string base58PublicKey, CancellationToken token = default(CancellationToken))
        {
            if (!base58PublicKey.StartsWith(Constants.ApiIdentifiers.ACCOUNT_PUBKEY))
                throw new ArgumentException($"Invalid Account Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.ACCOUNT_PUBKEY)}");
            return _apiClient.GetAccountByPubkeyAsync(base58PublicKey, token);
        }

        public Task<Account> GetAccountAsync(string base58PublicKey, string hash, CancellationToken token = default(CancellationToken))
        {
            if (!base58PublicKey.StartsWith(Constants.ApiIdentifiers.ACCOUNT_PUBKEY))
                throw new ArgumentException($"Invalid Account Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.ACCOUNT_PUBKEY)}");
            return _apiClient.GetAccountByPubkeyAndHashAsync(base58PublicKey, hash, token);
        }

        public Task<Account> GetAccountAsync(string base58PublicKey, ulong height, CancellationToken token = default(CancellationToken))
        {
            if (!base58PublicKey.StartsWith(Constants.ApiIdentifiers.ACCOUNT_PUBKEY))
                throw new ArgumentException($"Invalid Account Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.ACCOUNT_PUBKEY)}");
            return _apiClient.GetAccountByPubkeyAndHeightAsync(base58PublicKey, height, token);
        }


        //Name Service

        public Task<NameEntry> GetNameIdAsync(string name, CancellationToken token = default(CancellationToken)) => _apiClient.GetNameEntryByNameAsync(name, token);

        //Node

        public Task<PubKey> GetNodeBeneficiaryAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetNodeBeneficiaryAsync(token);

        public Task<PubKey> GetNodePublicKeyAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetNodePubkeyAsync(token);

        //Generation
        public Task<Generation> GetGenerationAsync(ulong height, CancellationToken token = default(CancellationToken)) => _apiClient.GetGenerationByHeightAsync(height, token);

        public Task<Generation> GetGenerationAsync(string hash, CancellationToken token = default(CancellationToken))
        {
            if (!hash.StartsWith(Constants.ApiIdentifiers.KEY_BLOCK_HASH))
                throw new ArgumentException($"Invalid Parameter Key Block Hash: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.KEY_BLOCK_HASH)}");
            return _apiClient.GetGenerationByHashAsync(hash, token);
        }

        public Task<Generation> GetCurrentGenerationAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetCurrentGenerationAsync(token);


        //Chain
        public Task<KeyBlock> GetCurrentKeyBlockAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetCurrentKeyBlockAsync(token);

        public Task<GenericTxs> GetMicroBlockTransactionsAsync(string microBlockHash, CancellationToken token = default(CancellationToken))
        {
            if (!microBlockHash.StartsWith(Constants.ApiIdentifiers.MICRO_BLOCK_HASH))
                throw new ArgumentException($"Invalid Parameter microBlockHash: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.MICRO_BLOCK_HASH)}");
            return _apiClient.GetMicroBlockTransactionsByHashAsync(microBlockHash, token);
        }

        public async Task<uint?> GetMicroBlockTransactionsCountAsync(string microBlockHash, CancellationToken token = default(CancellationToken))
        {
            if (!microBlockHash.StartsWith(Constants.ApiIdentifiers.MICRO_BLOCK_HASH))
                throw new ArgumentException($"Invalid Parameter microBlockHash: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.MICRO_BLOCK_HASH)}");
            var n = await _apiClient.GetMicroBlockTransactionsCountByHashAsync(microBlockHash, token).ConfigureAwait(false);
            return n.Count;
        }

        public Task<GenericSignedTx> GetMicroBlockTransactionAsync(string microBlockHash, BigInteger index, CancellationToken token = default(CancellationToken))
        {
            if (!microBlockHash.StartsWith(Constants.ApiIdentifiers.MICRO_BLOCK_HASH))
                throw new ArgumentException($"Invalid Parameter microBlockHash: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.MICRO_BLOCK_HASH)}");
            return _apiClient.GetMicroBlockTransactionByHashAndIndexAsync(microBlockHash, index, token);
        }

        public Task<MicroBlockHeader> GetMicroBlockHeaderAsync(string microBlockHash, CancellationToken token = default(CancellationToken))
        {
            if (!microBlockHash.StartsWith(Constants.ApiIdentifiers.MICRO_BLOCK_HASH))
                throw new ArgumentException($"Invalid Parameter microBlockHash: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.MICRO_BLOCK_HASH)}");
            return _apiClient.GetMicroBlockHeaderByHashAsync(microBlockHash, token);
        }

        public Task<KeyBlock> GetKeyBlockAsync(ulong height, CancellationToken token = default(CancellationToken)) => _apiClient.GetKeyBlockByHeightAsync(height, token);

        public Task<KeyBlock> GetKeyBlockAsync(string hash, CancellationToken token = default(CancellationToken))
        {
            if (!hash.StartsWith(Constants.ApiIdentifiers.KEY_BLOCK_HASH))
                throw new ArgumentException($"Invalid Parameter Key Block Hash: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.KEY_BLOCK_HASH)}");
            return _apiClient.GetKeyBlockByHashAsync(hash, token);
        }

        public Task<KeyBlock> GetPendingKeyBlockAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetPendingKeyBlockAsync(token);

        public async Task<ulong?> GetCurrentKeyBlockHeightAsync(CancellationToken token = default(CancellationToken))
        {
            var n = await _apiClient.GetCurrentKeyBlockHeightAsync(token).ConfigureAwait(false);
            return n.Height;
        }

        public async Task<string> GetCurrentKeyBlockHashAsync(CancellationToken token = default(CancellationToken))
        {
            var n = await _apiClient.GetCurrentKeyBlockHashAsync(token).ConfigureAwait(false);
            return n.Hash;
        }

        public Task<KeyBlockOrMicroBlockHeader> GetTopBlockAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetTopBlockAsync(token);

        public Task PostKeyBlockAsync(KeyBlock block, CancellationToken token = default(CancellationToken)) => _apiClient.PostKeyBlockAsync(block, token);
        //KeyStore

        public Keystore GenerateKeystore(RawKeyPair rawKeyPair, string walletPassword, string walletName) => Keystore.Generate(Configuration, rawKeyPair, walletPassword, walletName);

        //Keypair
        public MnemonicKeyPair GenerateMasterMnemonicKeyPair(string mnemonicSeedPassword) => MnemonicKeyPair.Generate(mnemonicSeedPassword, Configuration.EntropySizeInByte);


        public byte[] EncryptPrivateKey(string password, byte[] binaryKey) => EncryptKey(password, binaryKey.LeftPad(64));

        public byte[] EncryptPublicKey(string password, byte[] binaryKey) => EncryptKey(password, binaryKey.RightPad(32));

        public byte[] DecryptPrivateKey(string password, byte[] encryptedBinaryKey) => DecryptKey(password, encryptedBinaryKey);

        public byte[] DecryptPublicKey(string password, byte[] encryptedBinaryKey)
        {
            byte[] data = DecryptKey(password, encryptedBinaryKey);
            if (data.Length <= 32)
                return data;
            byte[] data2 = new byte[32];
            Buffer.BlockCopy(data, 0, data2, 0, 32);
            return data2;
        }

        public RawKeyPair EncryptRawKeyPair(RawKeyPair keyPairRaw, string password)
        {
            byte[] encryptedPublicKey = EncryptPublicKey(password, keyPairRaw.PublicKey);
            byte[] encryptedPrivateKey = EncryptPrivateKey(password, keyPairRaw.PrivateKey);
            return new RawKeyPair(encryptedPublicKey, encryptedPrivateKey);
        }

        private byte[] EncryptKey(string password, byte[] binaryData)
        {
            SHA256Managed sha = new SHA256Managed();

            byte[] hashedPassword = sha.ComputeHash(System.Text.Encoding.Default.GetBytes(password));
            IBufferedCipher cipher = CipherUtilities.GetCipher(Configuration.CipherAlgorithm);
            KeyParameter secretKey = new KeyParameter(hashedPassword);
            cipher.Init(true, secretKey);
            return cipher.DoFinal(binaryData);
        }

        private byte[] DecryptKey(string password, byte[] encryptedBinaryData)
        {
            SHA256Managed sha = new SHA256Managed();
            byte[] hashedPassword = sha.ComputeHash(System.Text.Encoding.Default.GetBytes(password));
            IBufferedCipher cipher = CipherUtilities.GetCipher(Configuration.CipherAlgorithm);
            KeyParameter secretKey = new KeyParameter(hashedPassword);
            cipher.Init(false, secretKey);
            return cipher.DoFinal(encryptedBinaryData);
        }

        //Compiler

        private async Task CheckCompilerBackendAsync()
        {
            if (_backend == null)
            { 
                APIVersion version=await _compilerClient.APIVersionAsync();
                string v = version.ApiVersion.Replace(".", "");
                if (v.Length == 1)
                    v += "0";
                if (v.Length == 2)
                    v += "0";
                int v2 = int.Parse(v);
                if (v2 > 399)
                {
                    _backend = "fate";
                }
                else
                {
                    _backend = "aevm";
                }
            }
        }
        public async Task<Calldata> EncodeCallDataAsync(string sourceCode, string function, List<string> arguments, CompileOptsBackend? opts = null, CancellationToken token = default(CancellationToken))
        {
            await CheckCompilerBackendAsync();
            FunctionCallInput body = new FunctionCallInput();
            body.Source = sourceCode;
            body.Function = function;
            if (opts == null)
                opts = _backend == "fate" ? CompileOptsBackend.Fate : CompileOptsBackend.Aevm;
            body.Options = new CompileOpts() {Backend = opts.Value};
            if (arguments != null)
            {
                foreach (string arg in arguments)
                    body.Arguments.Add(arg);
            }

            return await _compilerClient.EncodeCalldataAsync(body, token);
        }

        public Task<Calldata> EncodeCallDataAsync(string sourceCode, string function, params string[] arguments) => EncodeCallDataAsync(sourceCode, function, arguments.ToList());
        public Task<Calldata> EncodeCallDataAsync(string sourceCode, string function, CompileOptsBackend opts, params string[] arguments) => EncodeCallDataAsync(sourceCode, function, arguments.ToList(), opts);
        public Task<Calldata> EncodeCallDataAsync(string sourceCode, string function, CancellationToken token, params string[] arguments) => EncodeCallDataAsync(sourceCode, function, arguments.ToList(), null, token);
        public Task<Calldata> EncodeCallDataAsync(string sourceCode, string function, CompileOptsBackend opts, CancellationToken token, params string[] arguments) => EncodeCallDataAsync(sourceCode, function, arguments.ToList(), opts, token);

        public Task<SophiaJsonData> DecodeDataAsync(string calldata, string sophiaType, CancellationToken token = default(CancellationToken))
        {
            SophiaBinaryData body = new SophiaBinaryData();
            body.Data = calldata;
            body.SophiaType = sophiaType;
            return _compilerClient.DecodeDataAsync(body, token);
        }

        public async Task<JToken> DecodeCallResultAsync(string sourceCode, string function, string callResult, string callValue,  CompileOptsBackend? opts = null, CancellationToken token = default(CancellationToken))
        {
            await CheckCompilerBackendAsync();
            SophiaCallResultInput body = new SophiaCallResultInput();
            body.Source = sourceCode;
            body.Function = function;
            if (opts == null)
                opts = _backend == "fate" ? CompileOptsBackend.Fate : CompileOptsBackend.Aevm;
            body.Options = new CompileOpts() {Backend = opts.Value};
            body.CallResult = callResult;
            body.CallValue = callValue;
            return await _compilerClient.DecodeCallResultFixedAsync(body, token);
        }


        public async Task<DecodedCalldata> DecodeCallDataWithByteCodeAsync(string calldata, string byteCode, DecodeCalldataBytecodeBackend? opts=null, CancellationToken token = default(CancellationToken))
        {
            await CheckCompilerBackendAsync();
            DecodeCalldataBytecode body = new DecodeCalldataBytecode();
            body.CallData = calldata;
            body.Bytecode = byteCode;
            if (opts == null)
                opts = _backend == "fate" ? DecodeCalldataBytecodeBackend.Fate : DecodeCalldataBytecodeBackend.Aevm;
            body.Backend = opts.Value;
            return await _compilerClient.DecodeCalldataBytecodeAsync(body, token);
        }

        public async Task<DecodedCalldata> DecodeCallDataWithSourceAsync(string calldata, string sourceCode, CompileOptsBackend? opts = null, CancellationToken token = default(CancellationToken))
        {
            await CheckCompilerBackendAsync();
            DecodeCalldataSource body = new DecodeCalldataSource();
            body.CallData = calldata;
            body.Source = sourceCode;
            if (opts == null)
                opts = _backend == "fate" ? CompileOptsBackend.Fate : CompileOptsBackend.Aevm;
            body.Options = new CompileOpts() {Backend = opts.Value};
            return await _compilerClient.DecodeCalldataSourceAsync(body, token);
        }

        public Task<CompilerVersion> GetCompilerVersionAsync(CancellationToken token = default(CancellationToken)) => _compilerClient.VersionAsync(token);

        public Task<APIVersion> GetCompilerAPIVersionAsync(CancellationToken token = default(CancellationToken)) => _compilerClient.APIVersionAsync(token);

        public Task<API> GetCompilerApiAsync(CancellationToken token = default(CancellationToken)) => _compilerClient.ApiAsync(token);

        public async Task<ACI> GenerateACIAsync(string contractCode, CompileOptsBackend? opts = null, CancellationToken token = default(CancellationToken))
        {
            await CheckCompilerBackendAsync();
            if (opts == null)
                opts = _backend == "fate" ? CompileOptsBackend.Fate : CompileOptsBackend.Aevm;
            Contract body = new Contract {Code = contractCode, Options = new CompileOpts { Backend = opts.Value}};
            return await _compilerClient.GenerateACIAsync(body, token);
        }

        public async Task<ByteCode> CompileAsync(string contractCode, string srcFile, object fileSystem, CompileOptsBackend? opts = null, CancellationToken token = default(CancellationToken))
        {
            await CheckCompilerBackendAsync();
            if (opts == null)
                opts = _backend == "fate" ? CompileOptsBackend.Fate : CompileOptsBackend.Aevm;
            Contract body = new Contract {Code = contractCode, Options = new CompileOpts() {Backend = opts.Value}};
            if (!string.IsNullOrEmpty(srcFile))
                body.Options.SrcFile = srcFile;
            if (fileSystem != null)
                body.Options.FileSystem = fileSystem;
            return await _compilerClient.CompileContractAsync(body, token);
        }

        //Transactions

        public SpendTransaction CreateSpendTransaction(string sender, string recipient, BigInteger amount, string payload, ulong ttl, ulong nonce)
        {
            return new SpendTransaction(_factory, this)
            {
                Model = new SpendTx
                {
                    SenderId = sender,
                    RecipientId = recipient,
                    Amount = amount,
                    Payload = payload,
                    Ttl = ttl,
                    Nonce = nonce,
                }
            };
        }

        public SpendTransaction CreateSpendTransaction() => new SpendTransaction(_factory, this) {Model = new SpendTx()};


        public ContractCreateTransaction CreateContractCreateTransaction(ushort abiVersion, BigInteger amount, string callData, string contractByteCode, BigInteger deposit, ulong gas, BigInteger gasPrice, ulong nonce, string ownerId, ulong ttl, ushort vmVersion)
        {
            return new ContractCreateTransaction(_factory, this)
            {
                Model = new ContractCreateTx
                {
                    AbiVersion = abiVersion,
                    Amount = amount,
                    CallData = callData,
                    Code = contractByteCode,
                    Deposit = deposit,
                    Gas = gas,
                    GasPrice = gasPrice,
                    Nonce = nonce,
                    OwnerId = ownerId,
                    Ttl = ttl,
                    VmVersion = vmVersion,
                },
                FeeModel = new ContractCreateFee()
            };
        }

        public ContractCreateTransaction CreateContractCreateTransaction() => new ContractCreateTransaction(_factory, this) {Model = new ContractCreateTx(), FeeModel = new ContractCreateFee()};

        public ContractCallTransaction CreateContractCallTransaction(ushort abiVersion, string callData, string contractId, ulong gas, BigInteger gasPrice, ulong nonce, string callerId, ulong ttl)
        {
            return new ContractCallTransaction(_factory, this)
            {
                Model = new ContractCallTx
                {
                    AbiVersion = abiVersion,
                    Amount = BigInteger.Zero,
                    CallData = callData,
                    CallerId = callerId,
                    ContractId = contractId,
                    Gas = gas,
                    GasPrice = gasPrice,
                    Nonce = nonce,
                    Ttl = ttl
                },
                FeeModel = new ContractCallFee()
            };
        }

        public ContractCallTransaction CreateContractCallTransaction() => new ContractCallTransaction(_factory, this) {Model = new ContractCallTx(), FeeModel = new ContractCreateFee()};


        public NamePreclaimTransaction CreateNamePreClaimTransaction(string accountId, string name, BigInteger salt, ulong nonce, ulong ttl)
        {
            return new NamePreclaimTransaction(_factory, this, name)
            {
                Model = new NamePreclaimTx
                {
                    AccountId = accountId, CommitmentID = Encoding.GenerateCommitmentHash(name, salt), Nonce = nonce, Ttl = ttl,
                }
            };
        }

        public NamePreclaimTransaction CreateNamePreClaimTransaction(string name, BigInteger salt) => new NamePreclaimTransaction(_factory, this, name) {Model = new NamePreclaimTx {CommitmentID = Encoding.GenerateCommitmentHash(name, salt)}};


        public NameRevokeTransaction CreateNameRevokeTransaction(string accountId, string nameId, ulong nonce, ulong ttl)
        {
            return new NameRevokeTransaction(_factory, this) {Model = new NameRevokeTx {AccountId = accountId, NameId = nameId, Nonce = nonce, Ttl = ttl}};
        }

        public NameRevokeTransaction CreateNameRevokeTransaction() => new NameRevokeTransaction(_factory, this) {Model = new NameRevokeTx()};

        public NameTransferTransaction CreateNameTransferTransaction(string accountId, string nameId, string recipientId, ulong nonce, ulong ttl)
        {
            return new NameTransferTransaction(_factory, this)
            {
                Model = new NameTransferTx
                {
                    AccountId = accountId,
                    NameId = nameId,
                    RecipientId = recipientId,
                    Nonce = nonce,
                    Ttl = ttl
                }
            };
        }

        public NameTransferTransaction CreateNameTransferTransaction() => new NameTransferTransaction(_factory, this);


        public NameUpdateTransaction CreateNameUpdateTransaction(string accountId, string nameId, ulong nonce, ulong ttl, ulong clientTtl, ulong nameTtl, List<NamePointer> pointers)
        {
            return new NameUpdateTransaction(_factory, this)
            {
                Model = new NameUpdateTx
                {
                    AccountId = accountId,
                    NameId = nameId,
                    Nonce = nonce,
                    Ttl = ttl,
                    ClientTtl = clientTtl,
                    NameTtl = nameTtl,
                    Pointers = pointers
                }
            };
        }

        public NameUpdateTransaction CreateNameUpdateTransaction() => new NameUpdateTransaction(_factory, this);


        public NameClaimTransaction CreateNameClaimTransaction(string accountId, string name, BigInteger nameSalt, ulong nonce, ulong ttl)
        {
            return new NameClaimTransaction(_factory, this)
            {
                Model = new NameClaimTx
                {
                    AccountId = accountId,
                    Name = name,
                    NameSalt = nameSalt,
                    Nonce = nonce,
                    Ttl = ttl
                }
            };
        }

        public NameClaimTransaction CreateNameClaimTransaction() => new NameClaimTransaction(_factory, this) {Model = new NameClaimTx()};

        public OracleRegisterTransaction CreateOracleRegisterTransaction(string queryFormat, string responseFormat, string accountId, BigInteger queryFee, BigInteger fee, TTLType oracleTtlType, ulong oraclTtl, ushort abiVersion, ulong nonce, ulong ttl)
        {
            return new OracleRegisterTransaction(_factory, this)
            {
                Model = new OracleRegisterTx
                {
                    AccountId = accountId,
                    QueryFee = fee,
                    Fee = fee,
                    AbiVersion = abiVersion,
                    OracleTtl = new TTL {Type = oracleTtlType, Value = oraclTtl},
                    Nonce = nonce,
                    QueryFormat = queryFormat,
                    ResponseFormat = responseFormat,
                    Ttl = ttl,
                },
                FeeModel = new OracleFee(oraclTtl)
            };
        }

        public OracleQueryTransaction CreateOracleQueryTransaction(string query, string accountId, string oracleid, BigInteger queryFee, BigInteger fee, TTLType queryTtlType, ulong queryTtl, ulong responseRelativeTtl, ulong nonce, ulong ttl)
        {
            return new OracleQueryTransaction(_factory, this)
            {
                Model = new OracleQueryTx
                {
                    Query = query,
                    SenderId = accountId,
                    OracleId = oracleid,
                    QueryFee = queryFee,
                    Fee = fee,
                    QueryTtl = new TTL {Type = queryTtlType, Value = queryTtl},
                    ResponseTtl = new RelativeTTL {Type = RelativeTTLType.Delta, Value = responseRelativeTtl},
                    Nonce = nonce,
                    Ttl = ttl,
                },
                FeeModel = new OracleFee(queryTtl)
            };
        }

        public OracleExtendTransaction CreateOracleExtendTransaction(string oracleid, ulong relativeTtl, BigInteger fee, ulong nonce, ulong ttl)
        {
            return new OracleExtendTransaction(_factory, this)
            {
                Model = new OracleExtendTx
                {
                    OracleId = oracleid,
                    OracleTtl = new RelativeTTL {Type = RelativeTTLType.Delta, Value = relativeTtl},
                    Fee = fee,
                    Nonce = nonce,
                    Ttl = ttl,
                },
                FeeModel = new OracleFee(relativeTtl)
            };
        }

        public OracleRespondTransaction CreateOracleRespondTransaction(string response, string oracleid, string queryid, ulong responseTtl, BigInteger fee, ulong nonce, ulong ttl)
        {
            return new OracleRespondTransaction(_factory, this)
            {
                Model = new OracleRespondTx
                {
                    OracleId = oracleid,
                    QueryId = queryid,
                    ResponseTtl = new RelativeTTL {Type = RelativeTTLType.Delta, Value = responseTtl},
                    Response = response,
                    Fee = fee,
                    Nonce = nonce,
                    Ttl = ttl,
                },
                FeeModel = new OracleFee(responseTtl)
            };
        }


        public ChannelDepositTransaction CreateChannelDepositTransaction(string channelId, string fromId, BigInteger amount, ulong ttl, string stateHash, ulong round, ulong nonce)
        {
            return new ChannelDepositTransaction(_factory, this)
            {
                Model = new ChannelDepositTx
                {
                    ChannelId = channelId,
                    FromId = fromId,
                    Amount = amount,
                    Ttl = ttl,
                    StateHash = stateHash,
                    Round = round,
                    Nonce = nonce
                }
            };
        }

        public ChannelDepositTransaction CreateChannelDepositTransaction() => new ChannelDepositTransaction(_factory, this) {Model = new ChannelDepositTx()};

        public ChannelCloseMutualTransaction CreateChannelCloseMutualTransaction(string channelId, string fromId, BigInteger initiatorAmountFinal, BigInteger responderAmountFinal, ulong ttl, ulong nonce)
        {
            return new ChannelCloseMutualTransaction(_factory, this)
            {
                Model = new ChannelCloseMutualTx
                {
                    ChannelId = channelId,
                    FromId = fromId,
                    InitiatorAmountFinal = initiatorAmountFinal,
                    ResponderAmountFinal = responderAmountFinal,
                    Ttl = ttl,
                    Nonce = nonce
                }
            };
        }

        public ChannelCloseMutualTransaction CreateChannelCloseMutualTransaction() => new ChannelCloseMutualTransaction(_factory, this) {Model = new ChannelCloseMutualTx()};

        public ChannelCloseSoloTransaction CreateChannelCloseSoloTransaction(string channelId, string fromId, string payload, string poi, ulong ttl, ulong nonce)
        {
            return new ChannelCloseSoloTransaction(_factory, this)
            {
                Model = new ChannelCloseSoloTx
                {
                    ChannelId = channelId,
                    FromId = fromId,
                    Payload = payload,
                    Poi = poi,
                    Ttl = ttl,
                    Nonce = nonce
                }
            };
        }

        public ChannelCloseSoloTransaction CreateChannelCloseSoloTransaction() => new ChannelCloseSoloTransaction(_factory, this) {Model = new ChannelCloseSoloTx()};

        public ChannelCreateTransaction CreateChannelCreateTransaction(string initiatorId, string responderId, BigInteger initiatorAmount, BigInteger responderAmount, BigInteger pushAmount, BigInteger channelReserve, ulong lockPeriod, string stateHash, List<string> delegateids, ulong ttl, ulong nonce)
        {
            return new ChannelCreateTransaction(_factory, this)
            {
                Model = new ChannelCreateTx
                {
                    InitiatorId = initiatorId,
                    InitiatorAmount = initiatorAmount,
                    ResponderId = responderId,
                    ResponderAmount = responderAmount,
                    PushAmount = pushAmount,
                    ChannelReserve = channelReserve,
                    LockPeriod = lockPeriod,
                    StateHash = stateHash,
                    DelegateIds = delegateids,
                    Ttl = ttl,
                    Nonce = nonce
                }
            };
        }

        public ChannelCreateTransaction CreateChannelCreateTransaction() => new ChannelCreateTransaction(_factory, this) {Model = new ChannelCreateTx()};

        public ChannelSettleTransaction CreateChannelSettleTransaction(string channelId, string fromId, BigInteger initiatorAmountFinal, BigInteger responderAmountFinal, ulong ttl, ulong nonce)
        {
            return new ChannelSettleTransaction(_factory, this)
            {
                Model = new ChannelSettleTx
                {
                    ChannelId = channelId,
                    FromId = fromId,
                    InitiatorAmountFinal = initiatorAmountFinal,
                    ResponderAmountFinal = responderAmountFinal,
                    Ttl = ttl,
                    Nonce = nonce
                }
            };
        }

        public ChannelSettleTransaction CreateChannelSettleTransaction() => new ChannelSettleTransaction(_factory, this) {Model = new ChannelSettleTx()};

        public ChannelSlashTransaction CreateChannelSlashTransaction(string channelId, string fromId, string payload, string poi, ulong ttl, ulong nonce)
        {
            return new ChannelSlashTransaction(_factory, this)
            {
                Model = new ChannelSlashTx
                {
                    ChannelId = channelId,
                    FromId = fromId,
                    Payload = payload,
                    Poi = poi,
                    Ttl = ttl,
                    Nonce = nonce
                }
            };
        }

        public ChannelSlashTransaction CreateChannelSlashTransaction() => new ChannelSlashTransaction(_factory, this) {Model = new ChannelSlashTx()};


        public ChannelSnapshotSoloTransaction CreateChannelSnapshotSoloTransaction(string channelId, string fromId, string payload, ulong ttl, ulong nonce)
        {
            return new ChannelSnapshotSoloTransaction(_factory, this)
            {
                Model = new ChannelSnapshotSoloTx
                {
                    ChannelId = channelId,
                    FromId = fromId,
                    Payload = payload,
                    Ttl = ttl,
                    Nonce = nonce
                }
            };
        }

        public ChannelSnapshotSoloTransaction CreateChannelSnapshotSoloTransaction() => new ChannelSnapshotSoloTransaction(_factory, this) {Model = new ChannelSnapshotSoloTx()};

        public ChannelWithdrawTransaction CreateChannelWithdrawTransaction(string channelId, string toId, BigInteger amount, ulong ttl, string stateHash, ulong round, ulong nonce)
        {
            return new ChannelWithdrawTransaction(_factory, this)
            {
                Model = new ChannelWithdrawTx
                {
                    ChannelId = channelId,
                    ToId = toId,
                    Amount = amount,
                    Ttl = ttl,
                    StateHash = stateHash,
                    Round = round,
                    Nonce = nonce
                }
            };
        }

        public ChannelWithdrawTransaction CreateChannelWithdrawTransaction() => new ChannelWithdrawTransaction(_factory, this) {Model = new ChannelWithdrawTx()};


        public Task<GenericTxs> GetPendingAccountTransactionsAsync(string base58PublicKey, CancellationToken token = default(CancellationToken))
        {
            if (!base58PublicKey.StartsWith(Constants.ApiIdentifiers.ACCOUNT_PUBKEY))
                throw new ArgumentException($"Invalid Account Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.ACCOUNT_PUBKEY)}");
            return _apiClient.GetPendingAccountTransactionsByPubkeyAsync(base58PublicKey, token);
        }

        public Task<GenericTxs> GetPendingTransactionsAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetPendingTransactionsAsync(token);

        public Task<PostTxResponse> PostTransactionAsync(Tx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostTransactionAsync(tx, token);

        public Task<GenericSignedTx> GetTransactionByHashAsync(string txHash, CancellationToken token = default(CancellationToken)) => _apiClient.GetTransactionByHashAsync(txHash, token);

        public Task<TxInfoObject> GetTransactionInfoByHashAsync(string txHash, CancellationToken token = default(CancellationToken))
        {
            return _apiClient.GetTransactionInfoByHashAsync(txHash, token);
        }

        public Tx SignTransaction(UnsignedTx unsignedTx, string privateKey)
        {
            byte[] networkData = System.Text.Encoding.UTF8.GetBytes(Configuration.Network);
            byte[] binaryTx = Encoding.DecodeCheckWithIdentifier(unsignedTx.TX);
            byte[] txAndNetwork = networkData.Concatenate(binaryTx);
            byte[] sig = Signing.Sign(txAndNetwork, privateKey);
            Tx tx = new Tx();
            tx.TX = Encoding.EncodeSignedTransaction(sig, binaryTx);
            return tx;
        }

        public Task<CommitmentId> CreateDebugCommitmentIdAsync(string name, BigInteger salt, CancellationToken token = default(CancellationToken)) => _apiClient.GetCommitmentIdAsync(name, salt, token);
        public Task<UnsignedTx> CreateDebugChannelWithdrawAsync(ChannelWithdrawTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelWithdrawAsync(tx, token);
        public Task<UnsignedTx> CreateDebugChannelCloseMutualAsync(ChannelCloseMutualTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelCloseMutualAsync(tx, token);
        public Task<UnsignedTx> CreateDebugChannelCloseSoloAsync(ChannelCloseSoloTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelCloseSoloAsync(tx, token);
        public Task<UnsignedTx> CreateDebugChannelCreateAsync(ChannelCreateTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelCreateAsync(tx, token);
        public Task<UnsignedTx> CreateDebugChannelDepositAsync(ChannelDepositTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelDepositAsync(tx, token);
        public Task<UnsignedTx> CreateDebugChannelSettleAsync(ChannelSettleTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelSettleAsync(tx, token);
        public Task<UnsignedTx> CreateDebugChannelSlashAsync(ChannelSlashTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelSlashAsync(tx, token);
        public Task<UnsignedTx> CreateDebugChannelSnapshotSoloAsync(ChannelSnapshotSoloTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostChannelSnapshotSoloAsync(tx, token);
        public Task<UnsignedTx> CreateDebugContractCallAsync(ContractCallTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostContractCallAsync(tx, token);
        public Task<CreateContractUnsignedTx> CreateDebugContractCreateAsync(ContractCreateTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostContractCreateAsync(tx, token);
        public Task<UnsignedTx> CreateDebugNameClaimAsync(NameClaimTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostNameClaimAsync(tx, token);
        public Task<UnsignedTx> CreateDebugNamePreClaimAsync(NamePreclaimTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostNamePreclaimAsync(tx, token);
        public Task<UnsignedTx> CreateDebugNameRevokeAsync(NameRevokeTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostNameRevokeAsync(tx, token);
        public Task<UnsignedTx> CreateDebugNameTransferAsync(NameTransferTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostNameTransferAsync(tx, token);
        public Task<UnsignedTx> CreateDebugNameUpdateAsync(NameUpdateTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostNameUpdateAsync(tx, token);
        public Task<UnsignedTx> CreateDebugOracleExtendAsync(OracleExtendTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleExtendAsync(tx, token);
        public Task<UnsignedTx> CreateDebugOracleQueryAsync(OracleQueryTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleQueryAsync(tx, token);
        public Task<UnsignedTx> CreateDebugOracleRegisterAsync(OracleRegisterTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleRegisterAsync(tx, token);
        public Task<UnsignedTx> CreateDebugOracleRespondAsync(OracleRespondTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleRespondAsync(tx, token);
        public Task<UnsignedTx> CreateDebugSpendAsync(SpendTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostSpendAsync(tx, token);


        public Task<DryRunResults> DryRunTransactionsAsync(List<Dictionary<AccountParameter, object>> accounts, BigInteger? block, List<UnsignedTx> unsignedTransactions, CancellationToken token = default(CancellationToken))
        {
            DryRunInput body = new DryRunInput();
            if (accounts == null || accounts.Count == 0)
                throw new ArgumentException($"Invalid Parameter accounts: {Validation.NO_ENTRIES}");
            if (unsignedTransactions == null || unsignedTransactions.Count == 0)
                throw new ArgumentException($"Invalid Parameter unsignedTransactions: {Validation.NO_ENTRIES}");
            if (accounts.Count != unsignedTransactions.Count)
                throw new ArgumentException($"Invalid Parameter unsignedTransactions: {Validation.LIST_NOT_SAME_SIZE}");
            List<DryRunAccount> dryRunAccounts = new List<DryRunAccount>();
            foreach (Dictionary<AccountParameter, object> txParams in accounts)
            {
                DryRunAccount currAccount = new DryRunAccount();
                if (txParams.Count == 0)
                    throw new ArgumentException($"Invalid Parameter accounts.Dictionary: {Validation.NO_ENTRIES}");
                if (!txParams.ContainsKey(AccountParameter.PUBLIC_KEY))
                    throw new ArgumentException($"Invalid Parameter accounts.Dictionary missing value: {string.Format(Validation.MAP_MISSING_VALUE, AccountParameter.PUBLIC_KEY)}");
                currAccount.PublicKey = txParams[AccountParameter.PUBLIC_KEY].ToString();
                currAccount.Amount = txParams.ContainsKey(AccountParameter.AMOUNT) ? BigInteger.Parse(txParams[AccountParameter.AMOUNT].ToString()) : BigInteger.Zero;
                dryRunAccounts.Add(currAccount);
            }

            body.Accounts = dryRunAccounts;
            body.Top = block?.ToString();
            unsignedTransactions.ForEach(item => body.TXs.Add(new DryRunInputItem { CallReq = null, TX=item.TX }));
            _logger.LogDebug("Calling dry run on block {0} with body {1}", block, body);
            return _apiClient.DryRunTxsAsync(body, token);
        }


        //Oracles

        public Task<RegisteredOracle> GetRegisteredOracleAsync(string oraclePublicKey, CancellationToken token = default(CancellationToken))
        {
            if (!oraclePublicKey.StartsWith(Constants.ApiIdentifiers.ORACLE_PUBKEY))
                throw new ArgumentException($"Invalid Oracle Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.ORACLE_PUBKEY)}");
            return _apiClient.GetOracleByPubkeyAsync(oraclePublicKey, token);
        }

        public Task<OracleQueries> GetOracleQueriesAsync(string oraclePublicKey, string from, ushort? limit, Type? type, CancellationToken token = default(CancellationToken))
        {
            if (!oraclePublicKey.StartsWith(Constants.ApiIdentifiers.ORACLE_PUBKEY))
                throw new ArgumentException($"Invalid Oracle Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.ORACLE_PUBKEY)}");
            return _apiClient.GetOracleQueriesByPubkeyAsync(oraclePublicKey, from, limit, type, token);
        }

        public Task<OracleQuery> GetOracleAnswerAsync(string oraclePublicKey, string queryid, CancellationToken token = default(CancellationToken))
        {
            if (!oraclePublicKey.StartsWith(Constants.ApiIdentifiers.ORACLE_PUBKEY))
                throw new ArgumentException($"Invalid Oracle Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.ORACLE_PUBKEY)}");
            return _apiClient.GetOracleQueryByPubkeyAndQueryIdAsync(oraclePublicKey, queryid, token);
        }

        public Task<UnsignedTx> OracleAskAsync(OracleQueryTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleQueryAsync(tx, token);

        public Task<UnsignedTx> OracleRegisterAsync(OracleRegisterTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleRegisterAsync(tx, token);

        public Task<UnsignedTx> OracleRespondAsync(OracleRespondTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleRespondAsync(tx, token);

        public Task<UnsignedTx> OracleExtendAsync(OracleExtendTx tx, CancellationToken token = default(CancellationToken)) => _apiClient.PostOracleExtendAsync(tx, token);

        //Contracts

        public Task<ContractObject> GetContractAsync(string contractPubKey, CancellationToken token = default(CancellationToken))
        {
            if (!contractPubKey.StartsWith(Constants.ApiIdentifiers.CONTRACT_PUBKEY))
                throw new ArgumentException($"Invalid Contract Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.CONTRACT_PUBKEY)}");
            return _apiClient.GetContractAsync(contractPubKey, token);
        }

        public Task<ByteCode> GetContractCodeAsync(string contractPubKey, CancellationToken token = default(CancellationToken))
        {
            if (!contractPubKey.StartsWith(Constants.ApiIdentifiers.CONTRACT_PUBKEY))
                throw new ArgumentException($"Invalid Contract Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.CONTRACT_PUBKEY)}");
            return _apiClient.GetContractCodeAsync(contractPubKey, token);
        }

        public Task<ContractStore> GetContractStoreAsync(string contractPubKey, CancellationToken token = default(CancellationToken))
        {
            if (!contractPubKey.StartsWith(Constants.ApiIdentifiers.CONTRACT_PUBKEY))
                throw new ArgumentException($"Invalid Contract Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.CONTRACT_PUBKEY)}");
            return _apiClient.GetContractStoreAsync(contractPubKey, token);
        }

        public Task<PoI> GetContractPoIAsync(string contractPubKey, CancellationToken token = default(CancellationToken))
        {
            if (!contractPubKey.StartsWith(Constants.ApiIdentifiers.CONTRACT_PUBKEY))
                throw new ArgumentException($"Invalid Contract Public Key: {Validation.MissingApiIdentifier(Constants.ApiIdentifiers.CONTRACT_PUBKEY)}");
            return _apiClient.GetContractPoIAsync(contractPubKey, token);
        }

        //Channels
        public Task<Channel> GetChannelAsync(string channelPublicKey, CancellationToken token = default(CancellationToken)) => _apiClient.GetChannelByPubkeyAsync(channelPublicKey, token);

        //Peers
        public Task<PeerPubKey> GetPeerPublicKeyAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetPeerPubkeyAsync(token);

        public Task<Peers> GetPeersAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetPeersAsync(token);

        //Tokens

        public Task<TokenSupply> GetTokenSupplyAsync(ulong height, CancellationToken token = default(CancellationToken)) => _apiClient.GetTokenSupplyByHeightAsync(height, token);
        //Status

        public Task<Status> GetStatusAsync(CancellationToken token = default(CancellationToken)) => _apiClient.GetStatusAsync(token);
    }
}