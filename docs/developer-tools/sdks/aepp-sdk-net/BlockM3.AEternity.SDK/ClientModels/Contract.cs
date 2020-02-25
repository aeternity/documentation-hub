using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Progress;
using BlockM3.AEternity.SDK.Sophia;
using BlockM3.AEternity.SDK.Sophia.Types;
using BlockM3.AEternity.SDK.Transactions.Contracts;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class Contract : BaseFluent
    {
        private string _aci;

        private string _interface;

        public Contract(Account account) : base(account)
        {
        }

        public string ByteCode { get; private set; }

        internal CompileOptsBackend CompileOpts => AbiVersion == Constants.BaseConstants.ABI_FATE && VmVersion == Constants.BaseConstants.VM_FATE ? CompileOptsBackend.Fate : CompileOptsBackend.Aevm;
        internal DecodeCalldataBytecodeBackend DecodeOpts => AbiVersion == Constants.BaseConstants.ABI_FATE && VmVersion == Constants.BaseConstants.VM_FATE ? DecodeCalldataBytecodeBackend.Fate : DecodeCalldataBytecodeBackend.Aevm;
        internal BytecodeCallResultInputBackend BytecodeOpts => AbiVersion == Constants.BaseConstants.ABI_FATE && VmVersion == Constants.BaseConstants.VM_FATE ? BytecodeCallResultInputBackend.Fate : BytecodeCallResultInputBackend.Aevm;

        public List<Function> Functions { get; private set; }

        public Dictionary<BigInteger, SophiaType> Events { get; private set; }

        public string ACI
        {
            get => _aci;
            set
            {
                _aci = value;
                MayGenerateFunctions();
            }
        }

        public string Interface
        {
            get => _interface;
            set
            {
                _interface = value;
                MayGenerateFunctions();
            }
        }


        public string SourceCode { get; private set; }

        public ushort AbiVersion { get; set; }

        public ushort VmVersion { get; set; }

        public string ContractId { get; internal set; }

        private void MayGenerateFunctions()
        {
            if (!string.IsNullOrEmpty(ACI) && !string.IsNullOrEmpty(Interface))
            {
                (List<Function> f, Dictionary<BigInteger, SophiaType> ev) = SophiaMapper.ParseACI(ACI, Interface);
                Functions = f;
                Events = ev;
            }
        }

        public Contract CloneWithOtherAccount(Account ac)
        {
            Contract c=new Contract(ac);
            c.ContractId = ContractId;
            c.SourceCode = SourceCode;
            c.ByteCode = ByteCode;
            c.ACI = ACI;
            c.Interface = Interface;
            c.AbiVersion = AbiVersion;
            c.VmVersion = VmVersion;
            c.Tx = Tx;
            c.TxHash = TxHash;
            return c;
        }

        public async Task<DryRunContractReturn> MeasureDeployAsync(BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", CancellationToken token = default(CancellationToken), params object[] constructorpars)
        {
            DryRunResult res = await MeasureDeployInternalAsync(amount, deposit, gasPrice, constructorFunction, constructorpars, token).ConfigureAwait(false);
            return await DryRunContractReturn.DeserializeAsync(Account, this, constructorFunction, res, token).ConfigureAwait(false);
        }

        public async Task<DryRunContractReturn<T>> MeasureDeployAsync<T>(BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", CancellationToken token = default(CancellationToken), params object[] constructorpars)
        {
            DryRunResult res = await MeasureDeployInternalAsync(amount, deposit, gasPrice, constructorFunction, constructorpars, token).ConfigureAwait(false);
            return await DryRunContractReturn<T>.CreateAsync(Account, this, constructorFunction, res, token).ConfigureAwait(false);
        }

        public async Task<DryRunContractReturn> MeasureCallAsync(string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunResult res = await MeasureCallInternalAsync(true, gasPrice, function, amount, pars, token).ConfigureAwait(false);
            return  await DryRunContractReturn.DeserializeAsync(Account, this, function, res, token).ConfigureAwait(false);
        }

        public async Task<DryRunContractReturn<T>> MeasureCallAsync<T>(string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunResult res = await MeasureCallInternalAsync(true, gasPrice, function, amount, pars, token).ConfigureAwait(false);
            return await DryRunContractReturn<T>.CreateAsync(Account, this, function, res, token).ConfigureAwait(false);
        }

        public async Task<ContractReturn> StaticCallAsync(string function, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunResult res = await MeasureCallInternalAsync(false, Constants.BaseConstants.MINIMAL_GAS_PRICE, function, amount, pars, token).ConfigureAwait(false);
            return await ContractReturn.CreateAsync(Account, this, function, res.CallObj, null, token).ConfigureAwait(false);
        }

        public async Task<ContractReturn<T>> StaticCallAsync<T>(string function, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunResult res = await MeasureCallInternalAsync(false, Constants.BaseConstants.MINIMAL_GAS_PRICE, function, amount, pars, token).ConfigureAwait(false);
            return await ContractReturn<T>.CreateAsync(Account, this, function, res.CallObj, null, token).ConfigureAwait(false);
        }


        public async Task<InProgress<ContractReturn>> DeployAsync(BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, string constructorFunction = "init", CancellationToken token = default(CancellationToken), params object[] pars)
        {
            await DeployAsyncInternal(amount, deposit, gasPrice, gas, constructorFunction, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn>(new WaitForHash(this), new GetContractReturn(constructorFunction));
        }

        public async Task<InProgress<ContractReturn<T>>> DeployAsync<T>(BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, string constructorFunction = "init", CancellationToken token = default(CancellationToken), params object[] pars)
        {
            await DeployAsyncInternal(amount, deposit, gasPrice, gas, constructorFunction, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn<T>>(new WaitForHash(this), new GetContractReturn<T>(constructorFunction));
        }

        public async Task<InProgress<ContractReturn>> MeasureAndDeployAsync(BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunContractReturn dry = await MeasureDeployAsync(amount, deposit, gasPrice, constructorFunction, token, pars).ConfigureAwait(false);
            await DeployAsyncInternal(amount, deposit, (ulong) dry.GasPrice, dry.GasUsed, constructorFunction, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn>(new WaitForHash(this), new GetContractReturn(constructorFunction));
        }

        public async Task<InProgress<ContractReturn<T>>> MeasureAndDeployAsync<T>(BigInteger amount, BigInteger deposit, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, string constructorFunction = "init", CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunContractReturn<T> dry = await MeasureDeployAsync<T>(amount, deposit, gasPrice, constructorFunction, token, pars).ConfigureAwait(false);
            await DeployAsyncInternal(amount, deposit, (ulong) dry.GasPrice, dry.GasUsed, constructorFunction, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn<T>>(new WaitForHash(this), new GetContractReturn<T>(constructorFunction));
        }


        public async Task<InProgress<ContractReturn>> MeasureAndCallAsync(string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunContractReturn dry = await MeasureCallAsync(function, gasPrice, amount, token, pars).ConfigureAwait(false);
            await CallAsyncInternal((ulong) dry.GasPrice, dry.GasUsed, function, amount, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn>(new WaitForHash(this), new GetContractReturn(function));
        }

        public async Task<InProgress<ContractReturn<T>>> MeasureAndCallAsync<T>(string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            DryRunContractReturn<T> dry = await MeasureCallAsync<T>(function, gasPrice, amount, token, pars).ConfigureAwait(false);
            await CallAsyncInternal((ulong) dry.GasPrice, dry.GasUsed, function, amount, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn<T>>(new WaitForHash(this), new GetContractReturn<T>(function));
        }


        public async Task<InProgress<ContractReturn>> CallAsync(string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            await CallAsyncInternal(gasPrice, gas, function, amount, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn>(new WaitForHash(this), new GetContractReturn(function));
        }

        public async Task<InProgress<ContractReturn<T>>> CallAsync<T>(string function, ulong gasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE, ulong gas = Constants.BaseConstants.CONTRACT_GAS, ulong amount = 0, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            await CallAsyncInternal(gasPrice, gas, function, amount, token, pars).ConfigureAwait(false);
            return new InProgress<ContractReturn<T>>(new WaitForHash(this), new GetContractReturn<T>(function));
        }

        public InProgress<ContractReturn<T>> CreateContractReturnInProgressFromTx<T>(string txhash, string function)
        {
            return new InProgress<ContractReturn<T>>(new WaitForHash(this), new GetContractReturn<T>(function));
        }

        public InProgress<ContractReturn> CreateContractReturnInProgressFromTx(string txhash, string function)
        {
            return new InProgress<ContractReturn>(new WaitForHash(this), new GetContractReturn(function));
        }

        private async Task<DryRunResult> MeasureDeployInternalAsync(BigInteger amount, BigInteger deposit, ulong gasPrice, string constructorFunction, object[] pars, CancellationToken token)
        {
            Account.ValidatePublicKey();
            string calldata = await EncodeCallDataAsync(constructorFunction, pars, true, token).ConfigureAwait(false);
            ContractCreateTransaction ts = Account.Client.CreateContractCreateTransaction(AbiVersion, amount, calldata, ByteCode, deposit, Constants.BaseConstants.CONTRACT_GAS, gasPrice, Account.Nonce + 1, Account.KeyPair.PublicKey, Account.Ttl, VmVersion);
            CreateContractUnsignedTx tx = await ts.CreateUnsignedTransactionAsync(token).ConfigureAwait(false);
            List<Dictionary<AccountParameter, object>> dict = new List<Dictionary<AccountParameter, object>>();
            dict.Add(new Dictionary<AccountParameter, object> {{AccountParameter.PUBLIC_KEY, Account.KeyPair.PublicKey}});
            DryRunResults res = await Account.Client.DryRunTransactionsAsync(dict, null, new List<UnsignedTx> {tx}, token).ConfigureAwait(false);
            DryRunResult r=res.Results.First();
            if (r.Reason!=null && r.Reason.Contains("account_nonce_too_low"))
            {
                await Account.RefreshAsync(token);
                res = await Account.Client.DryRunTransactionsAsync(dict, null, new List<UnsignedTx> {tx}, token).ConfigureAwait(false);
                r=res.Results.First();
            }
            return r;
        }

        private async Task<DryRunResult> MeasureCallInternalAsync(bool stateful, ulong gasPrice, string function, ulong amount, object[] pars, CancellationToken token)
        {
            Account.ValidatePublicKey();
            ValidateContract();
            string calldata = await EncodeCallDataAsync(function, pars, stateful, token).ConfigureAwait(false);
            ContractCallTransaction ts = Account.Client.CreateContractCallTransaction(AbiVersion, calldata, ContractId, 1000000, gasPrice, Account.Nonce + 1, Account.KeyPair.PublicKey, Account.Ttl);
            UnsignedTx tx = await ts.CreateUnsignedTransactionAsync(token).ConfigureAwait(false);
            List<Dictionary<AccountParameter, object>> dict = new List<Dictionary<AccountParameter, object>>();
            dict.Add(new Dictionary<AccountParameter, object> {{AccountParameter.PUBLIC_KEY, Account.KeyPair.PublicKey}});
            DryRunResults res = await Account.Client.DryRunTransactionsAsync(dict, null, new List<UnsignedTx> {tx}, token).ConfigureAwait(false);
            DryRunResult r=res.Results.First();
            if (r.Reason!=null && r.Reason.Contains("account_nonce_too_low"))
            {
                await Account.RefreshAsync(token);
                res = await Account.Client.DryRunTransactionsAsync(dict, null, new List<UnsignedTx> {tx}, token).ConfigureAwait(false);
                r=res.Results.First();
            }
            return r;
        }

        private void ValidateContract()
        {
            if (string.IsNullOrEmpty(ContractId))
                throw new ArgumentException("Cannot call this method from an Account without a Private Key");
        }


        public async Task<string> EncodeCallDataAsync(string function, object[] pars, bool stateful, CancellationToken token)
        {
            if (string.IsNullOrEmpty(function))
                return null;
            Function f = Functions.FirstOrDefault(a => a.Name == function);

            if (f == null)
                throw new ArgumentException($"Invalid function name '{function}' available functions are : ({string.Join(",", Functions.Select(c => "'" + c.Name + "'"))})");
            if (pars.Length != f.InputTypes.Count)
                throw new ArgumentException($"Invalid number of parameters for function name '{function}' provided: {pars.Length} expected: {f.InputTypes.Count}");
            if (f.StateFul != stateful)
            {
                if (f.StateFul)
                    throw new ArgumentException($"This function is not static should be called with a normal call");
                throw new ArgumentException($"This function is static should be called with a static call (NO GAS)");
            }

            List<string> sts = new List<string>();
            for (int x = 0; x < pars.Length; x++)
            {
                sts.Add(f.InputTypes[x].Serialize(pars[x], pars[x].GetType()));
            }

            Calldata cl = await Account.Client.EncodeCallDataAsync(SourceCode, function, sts, CompileOpts, token).ConfigureAwait(false);
            return cl.CallData;
        }

        private async Task DeployAsyncInternal(BigInteger amount, BigInteger deposit, ulong gasPrice, ulong gas, string constructorFunction, CancellationToken token = default(CancellationToken), params object[] pars)
        {
            Account.ValidatePrivateKey();
            Account.Nonce++;
            string calldata = await EncodeCallDataAsync(constructorFunction, pars, true, token).ConfigureAwait(false);
            ContractCreateTransaction ts = Account.Client.CreateContractCreateTransaction(AbiVersion, amount, calldata, ByteCode, deposit, (ulong) gas, gasPrice, Account.Nonce, Account.KeyPair.PublicKey, Account.Ttl, VmVersion);
            await SignAndSendAsync(ts, token).ConfigureAwait(false);
        }

        private async Task CallAsyncInternal(ulong gasPrice, ulong gas, string function, ulong amount, CancellationToken token, params object[] pars)
        {
            Account.ValidatePrivateKey();
            ValidateContract();
            Account.Nonce++;
            string calldata = await EncodeCallDataAsync(function, pars, true, token).ConfigureAwait(false);
            ContractCallTransaction ts = Account.Client.CreateContractCallTransaction(AbiVersion, calldata, ContractId, (ulong) gas, gasPrice, Account.Nonce, Account.KeyPair.PublicKey, Account.Ttl);
            ts.Model.Amount = amount;
            await SignAndSendAsync(ts, token).ConfigureAwait(false);
        }

        internal static async Task<Contract> CreateAsync(Account account, string sourcecode, string bytecode, string contractId, ushort vmVersion = Constants.BaseConstants.VM_VERSION, ushort abiVersion = Constants.BaseConstants.ABI_VERSION, CancellationToken token = default(CancellationToken))
        {
            Contract c = new Contract(account);
            c.SourceCode = sourcecode;
            c.AbiVersion = abiVersion;
            c.VmVersion = vmVersion;
            if (string.IsNullOrEmpty(bytecode))
            {
                ByteCode bc = await account.Client.CompileAsync(sourcecode, null, null,c.CompileOpts, token).ConfigureAwait(false);
                c.ByteCode = bc.Bytecode;
            }
            else
                c.ByteCode = bytecode;

            ACI acii = await account.Client.GenerateACIAsync(sourcecode, c.CompileOpts, token).ConfigureAwait(false);
            c.ACI = acii.EncodedAci.ToString();
            c.Interface = acii.Interface;
            c.ContractId = contractId;
            return c;
        }
    }
}