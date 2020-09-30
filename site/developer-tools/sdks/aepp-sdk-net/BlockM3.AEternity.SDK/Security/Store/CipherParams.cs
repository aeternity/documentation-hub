using Newtonsoft.Json;

namespace BlockM3.AEternity.SDK.Security.Store
{
    public class CipherParams
    {
        [JsonProperty("nonce")]
        public string Nonce { get; set; }
    }
}