using System.IO;
using System.Numerics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlockM3.AEternity.SDK.Tests
{
    [TestClass]
    public class BaseTest
    {
        public static ServiceProvider ServiceProvider;

        public static string DefaultPassword = "kryptokrauts";

        public static string DOMAIN = "kryptokrauts";

        public static string NS = "test";

        public static string KK_NAMESPACE = DOMAIN + "." + NS;

        public static BigInteger TEST_SALT = 2654988072698203;

        public static string ResourcePath = string.Empty;

        protected FlatClient client = new FlatClient(ServiceProvider.GetService<Configuration>());

        public Configuration GetNewConfiguration()
        {
            return new Configuration(ServiceProvider.GetService<ILoggerFactory>(), ServiceProvider.GetService<IConfiguration>());
        }

        [AssemblyInitialize]
        public static void Init(TestContext context)
        {
            IConfiguration cfg = new ConfigurationBuilder().AddJsonFile("appsettings.json", true).AddEnvironmentVariables().Build();
            ServiceCollection services = new ServiceCollection();
            ILoggerFactory fact = new LoggerFactory(new ILoggerProvider[] {new DebugLoggerProvider()});
            services.AddSingleton(fact);
            services.AddSingleton(cfg);
            services.AddSingleton(new Configuration(fact, cfg));
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
        }
    }
}