using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BlockM3.AEternity.SDK.Exceptions;
using BlockM3.AEternity.SDK.Generated.Models;
using BlockM3.AEternity.SDK.Sophia;
using Newtonsoft.Json.Linq;

namespace BlockM3.AEternity.SDK.ClientModels
{
    public class ContractReturn
    {
        internal ContractReturn(ContractCallObject obj, string txinfo, Contract c)
        {
            CallerId = obj?.CallerId;
            Height = (long?) obj?.Height ?? -1;
            ContractId = obj?.ContractId;
            GasPrice = obj?.GasPrice ?? 0;
            GasUsed = obj?.GasUsed ?? 0;
            RawLog = obj?.Log.ToList() ?? new List<Event>();
            Events = SophiaMapper.GetEvents(c, RawLog);
            TXInfo = txinfo;
        }

        public string CallerId { get; }

        public long Height { get; }

        public string ContractId { get; }

        public BigInteger GasPrice { get; }

        public ulong GasUsed { get; }

        public List<Event> RawLog { get; }

        public List<Models.Event> Events { get; }

        public string TXInfo { get; }

        internal class AbortCls
        {
            public string abort { get; set; }
        }

        public static async Task<JToken> DeserializeAsync(Account account, Contract c, string function, ContractCallObject obj, CancellationToken token)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ReturnType))
            {
                Function f = c.Functions.First(a => a.Name == function);
                JToken ret = null;
                if (obj.ReturnValue != "cb_Xfbg4g==")
                    ret = await account.Client.DecodeCallResultAsync(c.SourceCode, function, obj.ReturnType, obj.ReturnValue, c.CompileOpts, token).ConfigureAwait(false);
                switch (obj.ReturnType)
                {
                    case "revert":
                        if (ret != null && ret.Value<JArray>("abort") != null)
                            throw new CallRevertException(ret.Value<JArray>("abort")[0].ToString());
                        throw new CallRevertException("Unknown Error");
                    case "error":
                        if (ret != null && ret.Value<JArray>("error") != null)
                            throw new CallErrorException(ret.Value<JArray>("error")[0].ToString());
                        throw new CallErrorException("Unknown Error");
                    default:
                        return ret;
                }
            }
            return null;
        }

        internal static async Task<ContractReturn> CreateAsync(Account account, Contract c, string function, ContractCallObject obj, string txinfo, CancellationToken token)
        {
            if (obj != null && !string.IsNullOrEmpty(obj.ReturnType))
                await DeserializeAsync(account, c, function, obj, token).ConfigureAwait(false);
            return new ContractReturn(obj, txinfo, c);
        }

    }

    public class ContractReturn<T> : ContractReturn
    {
        private ContractReturn(ContractCallObject obj, string txinfo, T value, Contract c) : base(obj, txinfo, c)
        {
            ReturnValue = value;
        }

        public T ReturnValue { get; }

        internal new static async Task<ContractReturn<T>> CreateAsync(Account account, Contract c, string function, ContractCallObject obj, string txinfo, CancellationToken token)
        {

            if (obj != null && !string.IsNullOrEmpty(obj.ReturnType))
            {
                JToken ret = await DeserializeAsync(account, c, function, obj, token).ConfigureAwait(false);
                Function f = c.Functions.First(a => a.Name == function);
                if (ret != null)
                    return new ContractReturn<T>(obj, txinfo, f.OutputType.Deserialize<T>(ret.ToString()), c);
            }

            return new ContractReturn<T>(obj, txinfo, default(T), c);
        }
    }
}