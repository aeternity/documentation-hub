using System;
using BlockM3.AEternity.SDK.Exceptions;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using Konscious.Security.Cryptography;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;
using Sodium;

namespace BlockM3.AEternity.SDK.Security.Store
{
    public class Keystore
    {
        public Keystore()
        {
        }

        public Keystore(string json)
        {
            JsonConvert.PopulateObject(json, this);
        }

        [JsonProperty("public_key")]
        public string PublicKey { get; set; }

        [JsonProperty("crypto")]
        public Crypto Crypto { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public int Version { get; set; }

        public byte[] RecoverPrivateKey(string argon2mode, string walletPassword)
        {
            try
            {
                byte[] salt = Hex.Decode(Crypto.KdfParams.Salt);

                Argon2 arg = Utils.Crypto.GetArgon2FromType(argon2mode, walletPassword);
                //mpiva get from wallet not from config
                arg.DegreeOfParallelism = Crypto.KdfParams.Parallelism; //_config.Parallelism;
                arg.Iterations = Crypto.KdfParams.OpsLimit; //_config.OpsLimit;
                arg.MemorySize = Crypto.KdfParams.MemLimitKib; //_config.MemLimitKIB;
                arg.Salt = salt;
                byte[] rawHash = arg.GetBytes(32);
                // extract nonce
                byte[] nonce = Hex.Decode(Crypto.CipherParams.Nonce);

                // extract cipertext
                byte[] ciphertext = Hex.Decode(Crypto.CipherText);

                // recover private key
                byte[] decrypted = null;
                try
                {
                    decrypted = SecretBox.Open(ciphertext, nonce, rawHash);
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {
                }

                if (decrypted == null)
                    throw new AException("Error recovering privateKey: wrong password.");
                return decrypted;
            }
            catch (Exception e)
            {
                throw new AException("Error recovering privateKey: wrong password.", e);
            }
        }


        public static Keystore Generate(Configuration cfg, RawKeyPair rawKeyPair, string walletPassword, string walletName)
        {
            // create derived key with Argon2

            byte[] salt = Utils.Crypto.GenerateSalt(cfg.DefaultSaltLength);

            Argon2 arg = Utils.Crypto.GetArgon2FromType(cfg.Argon2Mode, walletPassword);
            arg.DegreeOfParallelism = cfg.Parallelism;
            arg.Iterations = cfg.OpsLimit;
            arg.MemorySize = cfg.MemLimitKIB;
            arg.Salt = salt;
            byte[] rawHash = arg.GetBytes(32);

            // chain public and private key byte arrays
            byte[] privateAndPublicKey = rawKeyPair.ConcatenatedPrivateKey;
            // encrypt the key arrays with nonce and derived key
            byte[] nonce = SecretBox.GenerateNonce();
            byte[] ciphertext = SecretBox.Create(privateAndPublicKey, nonce, rawHash);

            // generate walletName if not given
            if (walletName == null || walletName.Trim().Length == 0)
            {
                walletName = "generated wallet file - " + DateTime.Now;
            }

            // generate the domain object for keystore
            Keystore wallet = new Keystore
            {
                PublicKey = rawKeyPair.GetPublicKey(),
                Id = Guid.NewGuid().ToString().Replace("-", ""),
                Name = walletName,
                Version = cfg.Version,
                Crypto = new Crypto
                {
                    SecretType = cfg.SecretType,
                    SymmetricAlgorithm = cfg.SymmetricAlgorithm,
                    CipherText = Hex.ToHexString(ciphertext),
                    Kdf = cfg.Argon2Mode,
                    CipherParams = new CipherParams {Nonce = Hex.ToHexString(nonce)},
                    KdfParams = new KdfParams {MemLimitKib = cfg.MemLimitKIB, OpsLimit = cfg.OpsLimit, Salt = Hex.ToHexString(salt), Parallelism = cfg.Parallelism}
                }
            };
            return wallet;
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}