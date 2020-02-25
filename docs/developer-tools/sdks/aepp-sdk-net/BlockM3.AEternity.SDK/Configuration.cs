using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BlockM3.AEternity.SDK
{
    public class Configuration
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerfactory;

        public Configuration() : this(null, null)
        {
        }

        public Configuration(IConfiguration cfg) : this(null, cfg)
        {
        }

        public Configuration(ILoggerFactory loggerFactory) : this(loggerFactory, null)
        {
        }

        public Configuration(ILoggerFactory loggerFactory, IConfiguration cfg)
        {
            _loggerfactory = loggerFactory ?? NullLoggerFactory.Instance;
            _logger = _loggerfactory?.CreateLogger<Configuration>();
            if (_logger == null)
                throw new ArgumentException("Unable to Create logger");
            if (cfg != null)
            {
                BaseUrl = cfg.GetValue("baseUrl", Constants.BaseConstants.DEFAULT_TESTNET_URL);
                ContractBaseUrl = cfg.GetValue("contractBaseUrl", Constants.BaseConstants.DEFAULT_TESTNET_CONTRACT_URL);
            }
            else
            {
                BaseUrl = Constants.BaseConstants.DEFAULT_TESTNET_URL;
                ContractBaseUrl = Constants.BaseConstants.DEFAULT_TESTNET_CONTRACT_URL;
            }

            IConfigurationSection sec = cfg?.GetSection("wallet");
            if (sec != null)
            {
                SecretType = sec.GetValue("secretType", "ed25519");
                SymmetricAlgorithm = sec.GetValue("symmetricAlgorithm", "xsalsa20-poly1305");
                MemLimitKIB = sec.GetValue("memlimitKIB", 65536);
                OpsLimit = sec.GetValue("opsLimit", 2);
                Parallelism = sec.GetValue("parallelism", 1);
                Version = sec.GetValue("version", 1);
                DefaultSaltLength = sec.GetValue("defaultSaltLength", 16);
                Argon2Mode = sec.GetValue("argon2Mode", "argon2id");
            }
            else
            {
                SecretType = "ed25519";
                SymmetricAlgorithm = "xsalsa20-poly1305";
                MemLimitKIB = 65536;
                OpsLimit = 2;
                Parallelism = 1;
                Version = 1;
                DefaultSaltLength = 16;
                Argon2Mode = "argon2id";
            }

            sec = cfg?.GetSection("transaction");
            if (sec != null)
            {
                NativeMode = sec.GetValue("nativeMode", true);
                Network = sec.GetValue("network", Constants.Network.TESTNET);
                MinimalGasPrice = sec.GetValue("minimalGasPrice", Constants.BaseConstants.MINIMAL_GAS_PRICE);
            }
            else
            {
                NativeMode = true;
                Network = Constants.Network.TESTNET;
                MinimalGasPrice = Constants.BaseConstants.MINIMAL_GAS_PRICE;
            }

            sec = cfg?.GetSection("keyPair");
            if (sec != null)
            {
                CipherAlgorithm = sec.GetValue("cipherAlgorithm", "AES/ECB/NoPadding");
                SecretKeySpec = sec.GetValue("secretKeySpec", "AES");
                EntropySizeInByte = sec.GetValue("entropySizeInByte", 256 / 8);
            }
            else
            {
                CipherAlgorithm = "AES/ECB/NoPadding";
                SecretKeySpec = "AES";
                EntropySizeInByte = 256 / 8;
            }
        }

        //Endpoints
        public string BaseUrl { get; set; }
        public string ContractBaseUrl { get; set; }

        //Wallet

        public string SecretType { get; set; }

        public string SymmetricAlgorithm { get; set; }

        public int MemLimitKIB { get; set; }

        public int OpsLimit { get; set; }

        public int Parallelism { get; set; }

        public int Version { get; set; }

        public int DefaultSaltLength { get; set; }

        public string Argon2Mode { get; set; }

        //Transaction

        public bool NativeMode { get; set; }

        public string Network { get; set; }

        public long MinimalGasPrice { get; set; }

        //Keypair
        public string CipherAlgorithm { get; set; }

        public string SecretKeySpec { get; set; }

        /**
         * this param has direct influence to the number of mnemonic seed words for correlation of entropy
         * bit size and number of words see spec {@linkplain
         * https://github.com/bitcoin/bips/blob/master/bip-0039.mediawiki#Generating_the_mnemonic}
         */
        public int EntropySizeInByte { get; set; }


        public virtual HttpClient GetHttpClient()
        {
            return new HttpClient();
        }

        public Generated.Api.Client GetApiClient()
        {
            if (string.IsNullOrEmpty(BaseUrl))
                throw new ArgumentException("Cannot instantiate ApiClient due to missing BaseUrl");
            _logger.LogDebug("Initializing ApiClient using BaseUrl {0}", BaseUrl);
            return new Generated.Api.Client(BaseUrl, GetHttpClient());
        }

        public ILoggerFactory GetLoggerFactory()
        {
            return _loggerfactory;
        }

        public Generated.Compiler.Client GetCompilerApiClient()
        {
            if (string.IsNullOrEmpty(ContractBaseUrl))
                throw new ArgumentException("Cannot instantiate CompilerApiClient due to missing ContractBaseUrl");
            _logger.LogDebug("Initializing CompApiClient using ContractBaseUrl {0}", ContractBaseUrl);
            return new Generated.Compiler.Client(ContractBaseUrl, GetHttpClient());
        }
    }
}