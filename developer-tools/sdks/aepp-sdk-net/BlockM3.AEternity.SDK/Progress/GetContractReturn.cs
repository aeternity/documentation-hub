using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.ClientModels;
using BlockM3.AEternity.SDK.Generated.Models;
using Contract = BlockM3.AEternity.SDK.ClientModels.Contract;

namespace BlockM3.AEternity.SDK.Progress
{
    public class GetContractReturn : ITransactionProgress
    {
        private readonly string _function;

        public GetContractReturn(string function)
        {
            _function = function;
        }
        public string TXHash => null;

        public async Task<(object result, bool done)> CheckForFinishAsync(object input, CancellationToken token = default(CancellationToken))
        {
            Contract co = (Contract) input;
            TxInfoObject res = await co.Account.Client.GetTransactionInfoByHashAsync(co.TxHash, token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(co.ContractId) && !string.IsNullOrEmpty(res.CallInfo?.ContractId))
                co.ContractId = res.CallInfo?.ContractId;
            return (await ContractReturn.CreateAsync(co.Account, co, _function, res.CallInfo, res.TXInfo, token).ConfigureAwait(false), true);
        }
    }

    public class GetContractReturn<T> : ITransactionProgress
    {
        private readonly string _function;

        public GetContractReturn(string function)
        {
            _function = function;
        }

        public string TXHash => null;

        public async Task<(object result, bool done)> CheckForFinishAsync(object input, CancellationToken token = default(CancellationToken))
        {
            Contract co = (Contract) input;
            TxInfoObject res = await co.Account.Client.GetTransactionInfoByHashAsync(co.TxHash, token).ConfigureAwait(false);
            if (string.IsNullOrEmpty(co.ContractId) && !string.IsNullOrEmpty(res.CallInfo?.ContractId))
                co.ContractId = res.CallInfo?.ContractId;
            return (await ContractReturn<T>.CreateAsync(co.Account, co, _function, res.CallInfo, res.TXInfo, token).ConfigureAwait(false), true);
        }
    }
}