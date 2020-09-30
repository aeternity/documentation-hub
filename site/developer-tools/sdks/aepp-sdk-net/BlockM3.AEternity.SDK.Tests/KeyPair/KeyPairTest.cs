using System.Collections.Generic;
using System.IO;
using System.Linq;
using BlockM3.AEternity.SDK.Exceptions;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Tests.KeyPair
{
    [TestClass]
    public class KeyPairTest : BaseTest
    {
        [TestCategory("Keypair Test")]
        [TestMethod]
        [Description("Mnemonic keypair generation tests")]
        public void MnemonicTest()
        {
            MnemonicKeyPair generatedKeyPair = client.GenerateMasterMnemonicKeyPair(DefaultPassword);
            MnemonicKeyPair restoredKeyPairWithSamePWD = MnemonicKeyPair.RecoverMnemonicKeyPair(generatedKeyPair.MnemonicSeedWords, DefaultPassword);
            MnemonicKeyPair restoredKeyPairWithoutPWD = MnemonicKeyPair.RecoverMnemonicKeyPair(generatedKeyPair.MnemonicSeedWords, null);
            // mnemonic keypair recovered from word seed list is same
            Assert.AreEqual(Hex.ToHexString(generatedKeyPair.PrivateKey), Hex.ToHexString(restoredKeyPairWithSamePWD.PrivateKey));
            // mnemonic keypair recovered from word seed list without password is not same
            Assert.AreNotEqual(Hex.ToHexString(generatedKeyPair.PrivateKey), Hex.ToHexString(restoredKeyPairWithoutPWD.PrivateKey));
            // mnemonic keypair cannot be generated due to small entropy
            Configuration cfg = GetNewConfiguration();
            cfg.EntropySizeInByte = 2;
            FlatClient cl = new FlatClient(cfg);
            Assert.ThrowsException<AException>(() => cl.GenerateMasterMnemonicKeyPair(null));
            // default vector recover test
            List<string> mnemonic = new List<string>
            {
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "abandon",
                "about"
            };
            string privateKeyAsHex = "61ae3ed32d9c82749be2f4bf122ea01de434705de3662ed416394df9be045ea9d8607b3a21a3d35529c0f4f60c7f3ddc782ce928d73dae02b0aad92ba38bd94f";
            string publicKeyAsHex = "ak_2eJ4Jk8F9yc1Hn4icG2apyExwrXcxZZADYLGDiMkyfoSpPSEM3";
            MnemonicKeyPair restoredDefault = MnemonicKeyPair.RecoverMnemonicKeyPair(mnemonic, DefaultPassword);
            BaseKeyPair restoredBaseKeyPair = Encoding.CreateBaseKeyPair(restoredDefault.ToRawKeyPair());
            Assert.AreEqual(publicKeyAsHex, restoredBaseKeyPair.PublicKey);
            Assert.AreEqual(privateKeyAsHex, restoredBaseKeyPair.PrivateKey);
            // hd derivation keys restore test
            mnemonic = new List<string>
            {
                "legal",
                "winner",
                "thank",
                "year",
                "wave",
                "sausage",
                "worth",
                "useful",
                "legal",
                "winner",
                "thank",
                "yellow"
            };
            MnemonicKeyPair master = MnemonicKeyPair.RecoverMnemonicKeyPair(mnemonic, DefaultPassword);
            MnemonicKeyPair masterNoPWD = MnemonicKeyPair.RecoverMnemonicKeyPair(mnemonic, "");
            Dictionary<string, string> derivedKeys = File.ReadAllLines(Path.Combine(ResourcePath, "derivedKeys.properties")).Select(a => a.Split('=')).ToDictionary(a => a[0], a => a[1]);
            /**
            * make sure every that derived keys can be restored and that the hardened
            * keys differ
            */
            for (int i = 0; i < 20; i++)
            {
                // derive different keys
                BaseKeyPair generatedDerivedKey = Encoding.CreateBaseKeyPair(master.DerivedKey(true).ToRawKeyPair());
                BaseKeyPair notHardendedKey = Encoding.CreateBaseKeyPair(master.DerivedKey(false).ToRawKeyPair());
                BaseKeyPair generatedDerivedKeyNoPwd = Encoding.CreateBaseKeyPair(masterNoPWD.DerivedKey(true).ToRawKeyPair());
                BaseKeyPair generatedDerivedKeyWithCustomPath = Encoding.CreateBaseKeyPair(master.DerivedKey(true, "4711'/4712'").ToRawKeyPair());
                // assert that the generated keys are the same
                Assert.AreEqual(derivedKeys[generatedDerivedKey.PublicKey], generatedDerivedKey.PrivateKey);
                // make sure, not hardended keys differ
                Assert.ThrowsException<KeyNotFoundException>(() => derivedKeys[notHardendedKey.PublicKey]);
                // make sure, keys derived from master with same mnemonics but different pwd
                // are different
                Assert.ThrowsException<KeyNotFoundException>(() => derivedKeys[generatedDerivedKeyNoPwd.PublicKey]);
                // make sure, keys from other derivation path differ
                Assert.ThrowsException<KeyNotFoundException>(() => derivedKeys[generatedDerivedKeyWithCustomPath.PublicKey]);
            }

            // hd derivation keys not possible from derived key test"
            mnemonic = new List<string>
            {
                "letter",
                "advice",
                "cage",
                "absurd",
                "amount",
                "doctor",
                "acoustic",
                "avoid",
                "letter",
                "advice",
                "cage",
                "above"
            };
            master = MnemonicKeyPair.RecoverMnemonicKeyPair(mnemonic, DefaultPassword);
            MnemonicKeyPair generatedDerivedKey2 = master.DerivedKey(true);
            Assert.ThrowsException<AException>(() => generatedDerivedKey2.DerivedKey(true), "Given mnemonicKeyPair object does not contain the master key");
        }
    }
}