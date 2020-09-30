using Newtonsoft.Json;

namespace BlockM3.AEternity.SDK.Security.Store
{
    public class Crypto
    {
        [JsonProperty("secret_type")]
        public string SecretType { get; set; }

        [JsonProperty("symmetric_alg")]
        public string SymmetricAlgorithm { get; set; }

        [JsonProperty("ciphertext")]
        public string CipherText { get; set; }

        [JsonProperty("cipher_params")]
        public CipherParams CipherParams { get; set; }

        [JsonProperty("kdf")]
        public string Kdf { get; set; }

        [JsonProperty("kdf_params")]
        public KdfParams KdfParams { get; set; }
    }
}