using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Progress;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Transactions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Account = BlockM3.AEternity.SDK.Generated.Models.Account;
using Contract = BlockM3.AEternity.SDK.ClientModels.Contract;
using Type = BlockM3.AEternity.SDK.Generated.Models.Type;

namespace BlockM3.AEternity.SDK.Extensions
{
    public static class Sync
    {
        public static Account GetAccount(this FlatClient client, string base58PublicKey) => client.GetAccountAsync(base58PublicKey).RunAndUnwrap();
        public static Account GetAccount(this FlatClient client, string hash, string base58PublicKey) => client.GetAccountAsync(base58PublicKey, hash).RunAndUnwrap();
        public static Account GetAccount(this FlatClient client, string base58PublicKey, ulong height) => client.GetAccountAsync(base58PublicKey, height).RunAndUnwrap();
        public static NameEntry GetNameId(this FlatClient client, string name) => client.GetNameIdAsync(name).RunAndUnwrap();
        public static PubKey GetNodeBeneficiary(this FlatClient client) => client.GetNodeBeneficiaryAsync().RunAndUnwrap();
        public static PubKey GetNodePublicKey(this FlatClient client) => client.GetNodePublicKeyAsync().RunAndUnwrap();
        public static Generation GetGeneration(this FlatClient client, ulong height) => client.GetGenerationAsync(height).RunAndUnwrap();
        public static Generation GetGeneration(this FlatClient client, string hash) => client.GetGenerationAsync(hash).RunAndUnwrap();
        public static Generation GetCurrentGeneration(this FlatClient client) => client.GetCurrentGenerationAsync().RunAndUnwrap();
        public static uint? GetMicroBlockTransactionsCount(this FlatClient client, string microBlockHash) => client.GetMicroBlockTransactionsCountAsync(microBlockHash).RunAndUnwrap();
        public static GenericSignedTx GetMicroBlockTransaction(this FlatClient client, string microBlockHash, BigInteger index) => client.GetMicroBlockTransactionAsync(microBlockHash, index).RunAndUnwrap();
        public static MicroBlockHeader GetMicroBlockHeader(this FlatClient client, string microBlockHash) => client.GetMicroBlockHeaderAsync(microBlockHash).RunAndUnwrap();
        public static KeyBlock GetKeyBlock(this FlatClient client, ulong height) => client.GetKeyBlockAsync(height).RunAndUnwrap();
        public static KeyBlock GetKeyBlock(this FlatClient client, string hash) => client.GetKeyBlockAsync(hash).RunAndUnwrap();
        public static KeyBlock GetPendingKeyBlock(this FlatClient client) => client.GetPendingKeyBlockAsync().RunAndUnwrap();
        public static ulong? GetCurrentKeyBlockHeight(this FlatClient client) => client.GetCurrentKeyBlockHeightAsync().RunAndUnwrap();
        public static string GetCurrentKeyBlockHash(this FlatClient client) => client.GetCurrentKeyBlockHashAsync().RunAndUnwrap();
        public static KeyBlockOrMicroBlockHeader GetTopBlock(this FlatClient client) => client.GetTopBlockAsync().RunAndUnwrap();
        public static void PostKeyBlock(this FlatClient client, KeyBlock block) => client.PostKeyBlockAsync(block).RunAndUnwrap();
        public static KeyBlock GetCurrentKeyBlock(this FlatClient client) => client.GetCurrentKeyBlockAsync().RunAndUnwrap();
        
        
        
        
        
        public static Calldata EncodeCallData(this FlatClient client, string sourceCode, string function, List<string> arguments) => client.EncodeCallDataAsync(sourceCode, function, arguments).RunAndUnwrap();
        public static Calldata EncodeCallData(this FlatClient client, string sourceCode, string function, params string[] arguments) => client.EncodeCallDataAsync(sourceCode, function, arguments).RunAndUnwrap();
        public static Calldata EncodeCallData(this FlatClient client, string sourceCode, string function, List<string> arguments, CompileOptsBackend opts) => client.EncodeCallDataAsync(sourceCode, function, arguments, opts).RunAndUnwrap();
        public static Calldata EncodeCallData(this FlatClient client, string sourceCode, string function, CompileOptsBackend opts, params string[] arguments) => client.EncodeCallDataAsync(sourceCode, function, opts, arguments).RunAndUnwrap();

        
        public static SophiaJsonData DecodeCallData(this FlatClient client, string calldata, string sophiaType) => client.DecodeDataAsync(calldata, sophiaType).RunAndUnwrap();
        public static JToken DecodeCallResult(this FlatClient client, string sourceCode, string function, string callResult, string callValue,CompileOptsBackend? opts=null) => client.DecodeCallResultAsync(sourceCode, function, callResult, callValue,opts).RunAndUnwrap();
        public static DecodedCalldata DecodeCallDataWithByteCode(this FlatClient client, string calldata, string byteCode,DecodeCalldataBytecodeBackend? opts=null) => client.DecodeCallDataWithByteCodeAsync(calldata, byteCode, opts).RunAndUnwrap();
        public static DecodedCalldata DecodeCallDataWithSource(this FlatClient client, string calldata, string sourceCode,CompileOptsBackend? opts=null) => client.DecodeCallDataWithSourceAsync(calldata, sourceCode, opts).RunAndUnwrap();
        public static CompilerVersion GetCompilerVersion(this FlatClient client) => client.GetCompilerVersionAsync().RunAndUnwrap();
        public static APIVersion GetCompilerAPIVersion(this FlatClient client) => client.GetCompilerAPIVersionAsync().RunAndUnwrap();
        public static API GetCompilerApi(this FlatClient client) => client.GetCompilerApiAsync().RunAndUnwrap();

        public static ACI GenerateACI(this FlatClient client, string contractCode,CompileOptsBackend? opts=null) => client.GenerateACIAsync(contractCode, opts).RunAndUnwrap();
        public static ByteCode Compile(this FlatClient client, string contractCode, string srcFile, object fileSystem, CompileOptsBackend? opts=null) => client.CompileAsync(contractCode, srcFile, fileSystem, opts).RunAndUnwrap();
        
        public static CommitmentId CreateDebugCommitmentId(this FlatClient client, string name, BigInteger salt) => client.CreateDebugCommitmentIdAsync(name, salt).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelWithdraw(this FlatClient client, ChannelWithdrawTx tx) => client.CreateDebugChannelWithdrawAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelCloseMutual(this FlatClient client, ChannelCloseMutualTx tx) => client.CreateDebugChannelCloseMutualAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelCloseSolo(this FlatClient client, ChannelCloseSoloTx tx) => client.CreateDebugChannelCloseSoloAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelCreate(this FlatClient client, ChannelCreateTx tx) => client.CreateDebugChannelCreateAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelDeposit(this FlatClient client, ChannelDepositTx tx) => client.CreateDebugChannelDepositAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelSettle(this FlatClient client, ChannelSettleTx tx) => client.CreateDebugChannelSettleAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelSlash(this FlatClient client, ChannelSlashTx tx) => client.CreateDebugChannelSlashAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugChannelSnapshotSolo(this FlatClient client, ChannelSnapshotSoloTx tx) => client.CreateDebugChannelSnapshotSoloAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugContractCall(this FlatClient client, ContractCallTx tx) => client.CreateDebugContractCallAsync(tx).RunAndUnwrap();
        public static CreateContractUnsignedTx CreateDebugContractCreate(this FlatClient client, ContractCreateTx tx) => client.CreateDebugContractCreateAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugNameClaim(this FlatClient client, NameClaimTx tx) => client.CreateDebugNameClaimAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugNamePreClaim(this FlatClient client, NamePreclaimTx tx) => client.CreateDebugNamePreClaimAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugNameRevoke(this FlatClient client, NameRevokeTx tx) => client.CreateDebugNameRevokeAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugNameTransfer(this FlatClient client, NameTransferTx tx) => client.CreateDebugNameTransferAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugNameUpdate(this FlatClient client, NameUpdateTx tx) => client.CreateDebugNameUpdateAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugOracleExtend(this FlatClient client, OracleExtendTx tx) => client.CreateDebugOracleExtendAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugOracleQuery(this FlatClient client, OracleQueryTx tx) => client.CreateDebugOracleQueryAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugOracleRegister(this FlatClient client, OracleRegisterTx tx) => client.CreateDebugOracleRegisterAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugOracleRespond(this FlatClient client, OracleRespondTx tx) => client.CreateDebugOracleRespondAsync(tx).RunAndUnwrap();
        public static UnsignedTx CreateDebugSpend(this FlatClient client, SpendTx tx) => client.CreateDebugSpendAsync(tx).RunAndUnwrap();
        public static RegisteredOracle GetRegisteredOracle(this FlatClient client, string oraclePublicKey) => client.GetRegisteredOracleAsync(oraclePublicKey).RunAndUnwrap();
        public static OracleQueries GetOracleQueries(this FlatClient client, string oraclePublicKey, string from, ushort? limit, Type? type) => client.GetOracleQueriesAsync(oraclePublicKey, from, limit, type).RunAndUnwrap();
        public static OracleQuery GetOracleAnswer(this FlatClient client, string oraclePublicKey, string queryid) => client.GetOracleAnswerAsync(oraclePublicKey, queryid).RunAndUnwrap();
        public static UnsignedTx OracleAsk(this FlatClient client, OracleQueryTx tx) => client.OracleAskAsync(tx).RunAndUnwrap();
        public static UnsignedTx OracleRegister(this FlatClient client, OracleRegisterTx tx) => client.CreateDebugOracleRegisterAsync(tx).RunAndUnwrap();
        public static UnsignedTx OracleRespond(this FlatClient client, OracleRespondTx tx) => client.CreateDebugOracleRespondAsync(tx).RunAndUnwrap();
        public static UnsignedTx OracleExtend(this FlatClient client, OracleExtendTx tx) => client.CreateDebugOracleExtendAsync(tx).RunAndUnwrap();
        public static ContractObject GetContract(this FlatClient client, string contractPubKey) => client.GetContractAsync(contractPubKey).RunAndUnwrap();
        public static ByteCode GetContractCode(this FlatClient client, string contractPubKey) => client.GetContractCodeAsync(contractPubKey).RunAndUnwrap();
        public static ContractStore GetContractStore(this FlatClient client, string contractPubKey) => client.GetContractStoreAsync(contractPubKey).RunAndUnwrap();
        public static PoI GetContractPoI(this FlatClient client, string contractPubKey) => client.GetContractPoIAsync(contractPubKey).RunAndUnwrap();
        public static Channel GetChannel(this FlatClient client, string channelPublicKey) => client.GetChannelAsync(channelPublicKey).RunAndUnwrap();
        public static PeerPubKey GetPeerPublicKey(this FlatClient client) => client.GetPeerPublicKeyAsync().RunAndUnwrap();
        public static Peers GetPeers(this FlatClient client) => client.GetPeersAsync().RunAndUnwrap();
        public static TokenSupply GetTokenSupply(this FlatClient client, ulong height) => client.GetTokenSupplyAsync(height).RunAndUnwrap();
        public static Status GetStatus(this FlatClient client) => client.GetStatusAsync().RunAndUnwrap();
        public static PostTxResponse PostTransaction(this FlatClient client, Tx tx) => client.PostTransactionAsync(tx).RunAndUnwrap();
        public static GenericSignedTx GetTransactionByHash(this FlatClient client, string txHash) => client.GetTransactionByHashAsync(txHash).RunAndUnwrap();
        public static TxInfoObject GetTransactionInfoByHash(this FlatClient client, string txHash) => client.GetTransactionInfoByHashAsync(txHash).RunAndUnwrap();
        public static DryRunResults DryRunTransactions(this FlatClient client, List<Dictionary<AccountParameter, object>> accounts, BigInteger? block, List<UnsignedTx> unsignedTransactions) => client.DryRunTransactionsAsync(accounts, block, unsignedTransactions).RunAndUnwrap();
        public static GenericTxs GetMicroBlockTransactions(this FlatClient client, string microBlockHash) => client.GetMicroBlockTransactionsAsync(microBlockHash).RunAndUnwrap();

        public static T CreateUnsignedTransaction<T>(this Transaction<T> transaction, bool nativeMode, long minimalGasPrice) where T : UnsignedTx, new() => transaction.CreateUnsignedTransactionAsync(nativeMode, minimalGasPrice).RunAndUnwrap();
        public static T CreateUnsignedTransaction<T>(this Transaction<T> transaction) where T : UnsignedTx, new() => transaction.CreateUnsignedTransactionAsync().RunAndUnwrap();

        public static ClientModels.Account ConstructAccount(this Client client, BaseKeyPair keypair) => ClientModels.Account.CreateAsync(client.FlatClient, keypair).RunAndUnwrap();
        public static ClientModels.Account ConstructAccount(this Client client, string publickey, string privatekey) => ClientModels.Account.CreateAsync(client.FlatClient, new BaseKeyPair(publickey, privatekey)).RunAndUnwrap();
        public static ClientModels.Account ConstructAccount(this Client client, string publickey) => ClientModels.Account.CreateAsync(client.FlatClient, new BaseKeyPair(publickey, string.Empty)).RunAndUnwrap();
        public static InProgress<PreClaim> PreClaimDomain(this ClientModels.Account account, string domain) => account.PreClaimDomainAsync(domain).RunAndUnwrap();
        public static Claim QueryDomain(this ClientModels.Account account, string domain) => account.QueryDomainAsync(domain).RunAndUnwrap();
        public static InProgress<bool> SendAmount(this ClientModels.Account account, string recipientPublicKey, BigInteger amount, string payload = "") => account.SendAmountAsync(recipientPublicKey, amount, payload).RunAndUnwrap();
        public static void Refresh(this ClientModels.Account account) => account.RefreshAsync().RunAndUnwrap();
        public static Contract ConstructContractWithContractId(this ClientModels.Account account, string sourcecode, string bytecode, string contractId, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION) => Contract.CreateAsync(account, sourcecode, bytecode, contractId, vmVersion, abiVersion).RunAndUnwrap();
        public static Contract ConstructContractWithContractId(this ClientModels.Account account, string sourcecode, string contractId, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION) => Contract.CreateAsync(account, sourcecode, null, contractId, vmVersion, abiVersion).RunAndUnwrap();
        public static Contract ConstructContract(this ClientModels.Account account, string sourcecode, string bytecode, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION) => Contract.CreateAsync(account, sourcecode, bytecode, null, vmVersion, abiVersion).RunAndUnwrap();
        public static Contract ConstructContract(this ClientModels.Account account, string sourcecode, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION) => Contract.CreateAsync(account, sourcecode, null, null, vmVersion, abiVersion).RunAndUnwrap();
        public static InProgress<OracleServer<T, S>> RegisterOracle<T, S>(this ClientModels.Account account, ulong queryFee = Constants.BaseConstants.ORACLE_QUERY_FEE, ulong fee = Constants.BaseConstants.FEE, Ttl ttl = default(Ttl), ushort abiVersion = Constants.BaseConstants.ORACLE_VM_VERSION) => account.RegisterOracleAsync<T, S>(queryFee, fee, ttl, abiVersion).RunAndUnwrap();
        public static OracleServer<T, S> GetOwnOracle<T, S>(this ClientModels.Account account) => account.GetOwnOracleAsync<T, S>().RunAndUnwrap();
        public static OracleClient<T, S> GetOracle<T, S>(this ClientModels.Account account, string oraclepubkey) => account.GetOracleAsync<T, S>(oraclepubkey).RunAndUnwrap();
        public static InProgress<Claim> Update(this Claim claim, ulong name_ttl = Constants.BaseConstants.NAME_TTL, ulong client_ttl = Constants.BaseConstants.NAME_CLIENT_TTL) => claim.UpdateAsync(name_ttl, client_ttl).RunAndUnwrap();
        public static InProgress<bool> Revoke(this Claim claim) => claim.RevokeAsync().RunAndUnwrap();
        public static InProgress<bool> Transfer(this Claim claim, string recipientPublicKey) => claim.TransferAsync(recipientPublicKey).RunAndUnwrap();
        public static InProgress<Claim> ClaimDomain(this PreClaim preclaim) => preclaim.ClaimDomainAsync().RunAndUnwrap();
        public static T CheckForFinish<T>(this InProgress<T> inprogress) => inprogress.CheckForFinishAsync().RunAndUnwrap();
        public static T WaitForFinish<T>(this InProgress<T> inprogress, TimeSpan span) => inprogress.WaitForFinishAsync(span).RunAndUnwrap();
        public static T WaitForFinish<T>(this InProgress<T> inprogress) => inprogress.WaitForFinishAsync().RunAndUnwrap();
        public static DryRunContractReturn MeasureDeploy(this Contract contract, BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", params object[] constructorpars) => contract.MeasureDeployAsync(amount, deposit, gasPrice, constructorFunction, CancellationToken.None, constructorpars).RunAndUnwrap();
        public static DryRunContractReturn<T> MeasureDeploy<T>(this Contract contract, BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", params object[] constructorpars) => contract.MeasureDeployAsync<T>(amount, deposit, gasPrice, constructorFunction, CancellationToken.None, constructorpars).RunAndUnwrap();
        public static DryRunContractReturn MeasureCall(this Contract contract, string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, params object[] pars) => contract.MeasureCallAsync(function, gasPrice, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static DryRunContractReturn<T> MeasureCall<T>(this Contract contract, string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, params object[] pars) => contract.MeasureCallAsync<T>(function, gasPrice, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static ContractReturn StaticCall(this Contract contract, string function, ulong amount, params object[] pars) => contract.StaticCallAsync(function, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static ContractReturn<T> StaticCall<T>(this Contract contract, string function, ulong amount, params object[] pars) => contract.StaticCallAsync<T>(function, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn> Deploy(this Contract contract, BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, string constructorFunction = "init", params object[] pars) => contract.DeployAsync(amount, deposit, gasPrice, gas, constructorFunction, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn<T>> Deploy<T>(this Contract contract, BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, string constructorFunction = "init", params object[] pars) => contract.DeployAsync<T>(amount, deposit, gasPrice, gas, constructorFunction, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn> Call(this Contract contract, string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, ulong amount = 0, params object[] pars) => contract.CallAsync(function, gasPrice, gas, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn<T>> Call<T>(this Contract contract, string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, ulong amount = 0, params object[] pars) => contract.CallAsync<T>(function, gasPrice, gas, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn> MeasureAndDeploy(this Contract contract, BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", params object[] pars) => contract.MeasureAndDeployAsync(amount, deposit, gasPrice, constructorFunction, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn<T>> MeasureAndDeploy<T>(this Contract contract, BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", params object[] pars) => contract.MeasureAndDeployAsync<T>(amount, deposit, gasPrice, constructorFunction, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn> MeasureAndCall(this Contract contract, string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, params object[] pars) => contract.MeasureAndCallAsync(function, gasPrice, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static InProgress<ContractReturn<T>> MeasureAndCall<T>(this Contract contract, string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, params object[] pars) => contract.MeasureAndCallAsync<T>(function, gasPrice, amount, CancellationToken.None, pars).RunAndUnwrap();
        public static List<OracleQuestion<T, S>> Query<T, S>(this OracleServer<T, S> oracleQuery, ushort? limit, string lastQueryId = null) => oracleQuery.QueryAsync(limit, lastQueryId).RunAndUnwrap();
        public static InProgress<bool> Extend<T, S>(this OracleServer<T, S> oracleQuery, ulong fee = Constants.BaseConstants.FEE, ulong extendTtl = Constants.BaseConstants.ORACLE_RESPONSE_TTL_VALUE) => oracleQuery.ExtendAsync(fee, extendTtl).RunAndUnwrap();
        public static InProgress<bool> Respond<T, S>(this OracleQuestion<T, S> oracleQuestion, S answer, ulong respondTtl = Constants.BaseConstants.ORACLE_RESPONSE_TTL_VALUE) => oracleQuestion.RespondAsync(answer, respondTtl).RunAndUnwrap();
        public static InProgress<S> Ask<T, S>(this OracleClient<T, S> oracle, T query, ulong fee = Constants.BaseConstants.ORACLE_QUERY_FEE, TTLType queryTtlType = TTLType.Delta, ulong queryTtl = Constants.BaseConstants.ORACLE_QUERY_TTL_VALUE, ulong responseRelativeTtl = Constants.BaseConstants.ORACLE_RESPONSE_TTL_VALUE) => oracle.AskAsync(query, fee, queryTtlType, queryTtl, responseRelativeTtl).RunAndUnwrap();


        public static T RunAndUnwrap<T>(this Task<T> func)
        {
            try
            {
                return func.GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                throw e.Flatten().InnerExceptions.First();
            }
        }

        public static void RunAndUnwrap(this Task func)
        {
            try
            {
                func.GetAwaiter().GetResult();
            }
            catch (AggregateException e)
            {
                throw e.Flatten().InnerExceptions.First();
            }
        }


        public static async Task<TResult> TimeoutAsync<TResult>(this Task<TResult> task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            using (var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var firsttask = await Task.WhenAny(task, Task.Delay(timeout, cancelSource.Token)).ConfigureAwait(false);
                if (firsttask == task)
                {
                    cancelSource.Cancel();
                    return await task.ConfigureAwait(false); //Propagate Exceptions
                }

                throw new TimeoutException("The operation has timed out.");
            }
        }

        public static async Task TimeoutAsync(this Task task, TimeSpan timeout, CancellationToken token = default(CancellationToken))
        {
            using (var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                var firsttask = await Task.WhenAny(task, Task.Delay(timeout, cancelSource.Token)).ConfigureAwait(false);
                if (firsttask == task)
                {
                    cancelSource.Cancel();
                    await task.ConfigureAwait(false); //Propagate Exceptions
                }
                else
                    throw new TimeoutException("The operation has timed out.");
            }
        }

        public static async Task<TResult> TimeoutAsync<TResult>(this Task<TResult> task, int numTrials, ILogger logger = null, CancellationToken token = default(CancellationToken))
        {
            using (var cancelSource = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                Task<TResult> ntask = Task.Run(() => task, cancelSource.Token);

                for (int x = 0; x < numTrials; x++)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancelSource.Token).ConfigureAwait(false);
                    if (task.IsCompleted || task.IsFaulted || task.IsCanceled)
                    {
                        if (task.IsFaulted)
                        {
                            Exception ex = task.Exception?.Flatten().InnerExceptions.First();
                            logger?.LogError(ex, ex?.Message ?? "");
                        }
                        else if (task.IsCanceled)
                        {
                            throw new OperationCanceledException();
                        }

                        return task.Result;
                    }


                    logger.LogWarning($"Unable to receive object of type {typeof(TResult).Name}, trying again in 1 second ({x + 1} of {numTrials})...");
                }

                cancelSource.Cancel();
                await ntask.ConfigureAwait(false); //Propagate Exceptions
            }

            throw new TimeoutException("Max number of function call trials exceeded, aborting");
        }
    }
}