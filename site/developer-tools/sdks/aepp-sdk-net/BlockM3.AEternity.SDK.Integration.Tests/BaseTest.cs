using System.IO;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockM3.AEternity.SDK.Integration.Tests
{
    [TestClass]
    [TestCategory("Integration Tests")]
    public class BaseTest
    {
        public static ServiceProvider ServiceProvider;
        protected static BaseKeyPair baseKeyPair;

        protected static FlatClient debugClient;
        protected static ILogger logger;
        protected static FlatClient nativeClient;
        public static string ResourcePath = string.Empty;
        protected static Client fluentClient;

        public static Configuration GetNewConfiguration()
        {
            return new Configuration(ServiceProvider.GetService<ILoggerFactory>(), ServiceProvider.GetService<IConfiguration>());
        }

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            IConfiguration cfg = new ConfigurationBuilder().AddJsonFile("appsettings.json", true).AddEnvironmentVariables().Build();
            ServiceCollection services = new ServiceCollection();
            ILoggerFactory fact = new LoggerFactory(new ILoggerProvider[] {new DebugLoggerProvider()});
            Configuration ncfg = new Configuration(fact, cfg);
            services.AddSingleton(fact);
            services.AddSingleton(cfg);
            services.AddSingleton(ncfg);
            ServiceProvider = services.BuildServiceProvider();
            string path = Directory.GetCurrentDirectory();
            do
            {
                if (!File.Exists(Path.Combine(path, "resources", "keystore.json")))
                {
                    int l = path.LastIndexOf(Path.DirectorySeparatorChar);
                    path = l == -1 ? null : path.Substring(0, l);
                }
                else
                {
                    ResourcePath = Path.Combine(path, "resources");
                    break;
                }
            } while (path != null);

            logger = fact.CreateLogger<BaseTest>();
            logger.LogInformation("--------------------------- Using following environment ---------------------------");
            logger.LogInformation($"{"baseUrl"}:{ncfg.BaseUrl}");
            logger.LogInformation($"{"contractBaseUrl"}:{ncfg.ContractBaseUrl}");
            Configuration cf = GetNewConfiguration();
            cf.NativeMode = false;
            cf.Network = Constants.Network.DEVNET;
            debugClient = new FlatClient(cf);

            cf = GetNewConfiguration();
            cf.NativeMode = true;
            cf.Network = Constants.Network.DEVNET;
            nativeClient = new FlatClient(cf);
            baseKeyPair = new BaseKeyPair(TestConstants.BENEFICIARY_PRIVATE_KEY);
            fluentClient = new Client(cf);
        }
    }
}