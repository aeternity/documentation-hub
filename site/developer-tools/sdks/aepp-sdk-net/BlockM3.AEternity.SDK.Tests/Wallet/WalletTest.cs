using System.IO;
using System.Text;
using BlockM3.AEternity.SDK.Exceptions;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Security.Store;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Tests.Wallet
{
    [TestClass]
    public class WalletTest : BaseTest
    {
        [TestCategory("Wallet Tests")]
        [TestMethod]
        [Description("keyPair can be recovered from a generated wallet")]
        public void Recover()
        {
            string walletFileSecret = "my_super_safe_password";

            Configuration cfg = ServiceProvider.GetService<Configuration>();
            // generate Keypair
            RawKeyPair keypair = RawKeyPair.Generate();
            Keystore stor = Keystore.Generate(cfg, keypair, walletFileSecret, null);
            Assert.IsNotNull(stor);

            // recover Keypair
            byte[] recoveredPrivateKey = stor.RecoverPrivateKey(cfg.Argon2Mode, walletFileSecret);
            RawKeyPair recoveredRawKeypair = new RawKeyPair(Hex.ToHexString(recoveredPrivateKey));
            Assert.IsNotNull(recoveredRawKeypair);

            // compare generated and recovered keypair
            CollectionAssert.AreEqual(keypair.ConcatenatedPrivateKey, recoveredRawKeypair.ConcatenatedPrivateKey);
        }

        [TestCategory("Wallet Tests")]
        [TestMethod]
        [Description("recovery of a valid keystore.json works")]
        public void RecoverFromFile()
        {
            Configuration cfg = ServiceProvider.GetService<Configuration>();
            string walletFileSecret = "aeternity";
            string expectedPubKey = "ak_2hSFmdK98bhUw4ar7MUdTRzNQuMJfBFQYxdhN9kaiopDGqj3Cr";
            string keystore = File.ReadAllText(Path.Combine(ResourcePath, "keystore.json"), Encoding.UTF8);
            Keystore store = new Keystore(keystore);
            byte[] privateKey = store.RecoverPrivateKey(cfg.Argon2Mode, walletFileSecret);
            BaseKeyPair keyPair = new BaseKeyPair(Hex.ToHexString(privateKey));
            Assert.AreEqual(expectedPubKey, keyPair.PublicKey);
        }

        [TestCategory("Wallet Tests")]
        [TestMethod]
        [Description("recovery of a valid keystore.json fails with wrong password")]
        public void RecoverFromFileBadPass()
        {
            Configuration cfg = ServiceProvider.GetService<Configuration>();
            string walletFileSecret = "abc";
            string keystore = File.ReadAllText(Path.Combine(ResourcePath, "keystore.json"), Encoding.UTF8);
            Keystore store = new Keystore(keystore);
            try
            {
                store.RecoverPrivateKey(cfg.Argon2Mode, walletFileSecret);
                Assert.Fail();
            }
            catch (AException e)
            {
                Assert.AreEqual("Error recovering privateKey: wrong password.", e.Message);
            }
        }
    }
}