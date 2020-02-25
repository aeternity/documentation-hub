using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Sophia;
using Newtonsoft.Json.Linq;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class DryRunContractReturn : ContractReturn
    {
        internal DryRunContractReturn(DryRunResult res, Contract c) : base(res.CallObj, null, c)
        {
            Type = res.Type;
            Result = res.Result;
            Reason = res.Reason;
        }

        public string Type { get; set; }

        public string Result { get; set; }

        public string Reason { get; set; }

        
        public static async Task<DryRunContractReturn> DeserializeAsync(Account account, Contract c, string function, DryRunResult obj, CancellationToken token)
        {
            if (obj != null && obj.CallObj!=null && !string.IsNullOrEmpty(obj.CallObj.ReturnType))
                await DeserializeAsync(account, c, function, obj.CallObj, token).ConfigureAwait(false);
            return new DryRunContractReturn(obj, c);
        }
    }

    public class DryRunContractReturn<T> : DryRunContractReturn
    {
        internal DryRunContractReturn(DryRunResult res, T value, Contract c) : base(res, c)
        {
            ReturnValue = value;
        }

        public T ReturnValue { get; }

        internal new static async Task<DryRunContractReturn<T>> CreateAsync(Account account, Contract c, string function, DryRunResult obj, CancellationToken token)
        {

            if (obj != null && obj.CallObj!=null && !string.IsNullOrEmpty(obj.CallObj.ReturnType))
            {
                JToken ret = await DeserializeAsync(account, c, function, obj.CallObj, token).ConfigureAwait(false);
                Function f = c.Functions.First(a => a.Name == function);
                if (ret != null)
                    return new DryRunContractReturn<T>(obj, f.OutputType.Deserialize<T>(ret.ToString()), c);
            }
            return new DryRunContractReturn<T>(obj, default(T), c);
        }

    }
}