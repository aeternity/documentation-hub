using Newtonsoft.Json;

namespace BlockM3.AEternity.SDK.Security.Store
{
    public class KdfParams
    {
        [JsonProperty("memlimit_kib")]
        public int MemLimitKib { get; set; }

        [JsonProperty("opslimit")]
        public int OpsLimit { get; set; }

        [JsonProperty("salt")]
        public string Salt { get; set; }

        [JsonProperty("parallelism")]
        public int Parallelism { get; set; }
    }
}