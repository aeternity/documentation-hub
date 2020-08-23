using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using BlockM3.AEternity.SDK.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Tests
{
    [TestClass]
    public class GenerationAndSigningTest : BaseTest
    {
        private static readonly string privateKeyAsHex = "4d881dd1917036cc231f9881a0db978c8899dd76a817252418606b02bf6ab9d22378f892b7cc82c2d2739e994ec9953aa36461f1eb5a4a49a5b0de17b3d23ae8";
        private static readonly string publicKeyWithPrefix = "ak_Gd6iMVsoonGuTF8LeswwDDN2NF5wYHAoTRtzwdEcfS32LWoxm";
        private static readonly byte[] publicKey = Encoding.DecodeCheckWithIdentifier(publicKeyWithPrefix);
        private static readonly byte[] txBinaryAsArray = {248, 76, 12, 1, 160, 35, 120, 248, 146, 183, 204, 130, 194, 210, 115, 158, 153, 78, 201, 149, 58, 163, 100, 97, 241, 235, 90, 74, 73, 165, 176, 222, 23, 179, 210, 58, 232, 160, 63, 40, 35, 12, 40, 65, 38, 215, 218, 236, 136, 133, 42, 120, 160, 179, 18, 191, 241, 162, 198, 203, 209, 173, 89, 136, 202, 211, 158, 59, 12, 122, 1, 1, 1, 132, 84, 101, 115, 116};
        private static readonly byte[] signatureAsArray = {95, 146, 31, 37, 95, 194, 36, 76, 58, 49, 167, 156, 127, 131, 142, 248, 25, 121, 139, 109, 59, 243, 203, 205, 16, 172, 115, 143, 254, 236, 33, 4, 43, 46, 16, 190, 46, 46, 140, 166, 76, 39, 249, 54, 38, 27, 93, 159, 58, 148, 67, 198, 81, 206, 106, 237, 91, 131, 27, 14, 143, 178, 130, 2};

        [TestCategory("Crypto")]
        [TestMethod]
        [Description("generates an account key pair")]
        public void GenerateKeyPair()
        {
            BaseKeyPair keyPair = BaseKeyPair.Generate();
            Assert.IsNotNull(keyPair);
            Assert.IsTrue(Encoding.IsAddressValid(keyPair.PublicKey));
            Assert.IsTrue(keyPair.PublicKey.StartsWith("ak_"));
            int length = keyPair.PublicKey.Length;
            Assert.IsTrue(length <= 53 && length >= 51);
        }

        [TestCategory("Crypto")]
        [TestMethod]
        [Description("generate a password encrypted key pair, verify for private key, verify for public key")]
        public void EncryptPassword()
        {
            RawKeyPair keyPair = RawKeyPair.Generate();
            string password = "verysecret";
            byte[] privateBinary = keyPair.ConcatenatedPrivateKey;
            byte[] encryptedBinary = client.EncryptPrivateKey(password, privateBinary);
            byte[] decryptedBinary = client.DecryptPrivateKey(password, encryptedBinary);
            CollectionAssert.AreEqual(privateBinary, decryptedBinary);
            byte[] publicBinary = keyPair.PublicKey;
            encryptedBinary = client.EncryptPublicKey(password, publicBinary);
            decryptedBinary = client.DecryptPublicKey(password, encryptedBinary);
            CollectionAssert.AreEqual(publicBinary, decryptedBinary);
        }

        [TestCategory("Crypto")]
        [TestMethod]
        [Description("can be encoded and decoded")]
        public void EncodeBase()
        {
            string input = "helloword010101023";
            byte[] inputBinary = System.Text.Encoding.UTF8.GetBytes(input);
            string encoded = Encoding.EncodeCheck(inputBinary, EncodingType.BASE58);
            byte[] decodedBinary = Encoding.DecodeCheck(encoded, EncodingType.BASE58);
            string decoded = System.Text.Encoding.UTF8.GetString(decodedBinary);
            Assert.AreEqual(input, decoded);
        }

        [TestCategory("Crypto")]
        [TestMethod]
        [Description("check for the correct private key for the beneficiary")]
        public void Recover()
        {
            string beneficiaryPub = "ak_twR4h7dEcUtc2iSEDv8kB7UFJJDGiEDQCXr85C3fYF8FdVdyo";
            BaseKeyPair keyPair = new BaseKeyPair("79816BBF860B95600DDFABF9D81FEE81BDB30BE823B17D80B9E48BE0A7015ADF");
            Assert.AreEqual(beneficiaryPub, keyPair.PublicKey);
        }

        [TestCategory("Crypto")]
        [TestMethod]
        [Description("should produce correct signature")]
        public void Sign()
        {
            byte[] txSignature = Signing.Sign(txBinaryAsArray, privateKeyAsHex);
            CollectionAssert.AreEqual(txSignature, signatureAsArray);
        }

        [TestCategory("Crypto")]
        [TestMethod]
        [Description("should verify tx with correct signature")]
        public void Verify()
        {
            bool verified = Signing.Verify(txBinaryAsArray, signatureAsArray, Hex.ToHexString(publicKey));
            Assert.IsTrue(verified);
        }

        [TestCategory("Crypto")]
        [TestMethod]
        [Description("Misc Tests")]
        public void PersionalMessages()
        {
            string message = "test";
            string messageSignatureAsHex = "20f779383f3ce0ab7781b7c8ff848e6d80f7f22d5cdc266763cd74d89c5ee0716758e75f56391711957f506d4993ae7dea62bec0f2806e6de66227f52836160a";
            byte[] messageSignature = Hex.Decode(messageSignatureAsHex);
            string messageNonASCII = "tæst";
            string messageNonASCIISignatureAsHex = "68d1344c46d9b2ef642490ffade3155c714471dd6d097fc393edc1004031a5492270de77ef8918df7857ea348d8ba4444ed1ff8e84f19e8e685e31174356fa06";
            byte[] messageNonASCIISignature = Hex.Decode(messageNonASCIISignatureAsHex);
            //sign - should produce correct signature of message
            byte[] msgSignature = Signing.SignMessage(message, privateKeyAsHex);
            CollectionAssert.AreEqual(msgSignature, messageSignature);
            //sign - should produce correct signature of message with non-ASCII chars
            msgSignature = Signing.SignMessage(messageNonASCII, privateKeyAsHex);
            CollectionAssert.AreEqual(msgSignature, messageNonASCIISignature);
            //verify - should verify message
            bool verified = Signing.VerifyMessage(message, messageSignature, Hex.ToHexString(publicKey));
            Assert.IsTrue(verified);
            //verify - should verify message with non-ASCII chars
            verified = Signing.VerifyMessage(messageNonASCII, messageNonASCIISignature, Hex.ToHexString(publicKey));
            Assert.IsTrue(verified);
            // hashing produces 256 bit blake2b byte buffers
            string foobar = "foobar";
            string foobarHashedHex = "93a0e84a8cdd4166267dbe1263e937f08087723ac24e7dcc35b3d5941775ef47";
            byte[] hash = Encoding.Hash(System.Text.Encoding.UTF8.GetBytes(foobar));
            Assert.AreEqual(foobarHashedHex, Hex.ToHexString(hash));
            // convert base58Check address to hex
            string address = "ak_Gd6iMVsoonGuTF8LeswwDDN2NF5wYHAoTRtzwdEcfS32LWoxm";
            string hex = Encoding.AddressToHex(address);
            string fromHexAddress = Encoding.EncodeCheck(Hex.Decode(hex.Substring(2)), Constants.ApiIdentifiers.ACCOUNT_PUBKEY);
            Assert.AreEqual(fromHexAddress, address);
            // commitmentHash generation test
            string kkNamespaceCommitmentId = "cm_aJBGWWjT65JqviLSexkofr5oEAEuEySNkmghsqxNjGt2WqPW8";
            string generatedCommitmentId = Encoding.GenerateCommitmentHash(KK_NAMESPACE, TEST_SALT);
            Assert.AreEqual(kkNamespaceCommitmentId, generatedCommitmentId);
            // formatSalt generation test
            byte[] oneSalt = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 110, 178, 148, 244, 193, 91};
            byte[] formattedSalt = Encoding.BigIntegerToBytes(TEST_SALT);
            CollectionAssert.AreEqual(oneSalt, formattedSalt);
            // nameId generation test
            byte[] nameId = {182, 98, 49, 33, 170, 167, 215, 180, 62, 190, 1, 241, 67, 76, 100, 93, 243, 101, 162, 45, 120, 34, 190, 119, 255, 230, 114, 199, 72, 36, 190, 173};
            byte[] generatedNameId = Encoding.NameId(KK_NAMESPACE);
            CollectionAssert.AreEqual(nameId, generatedNameId);
            // test concat nameId and salt
            byte[] nameIdAndSalt = {182, 98, 49, 33, 170, 167, 215, 180, 62, 190, 1, 241, 67, 76, 100, 93, 243, 101, 162, 45, 120, 34, 190, 119, 255, 230, 114, 199, 72, 36, 190, 173, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 9, 110, 178, 148, 244, 193, 91};
            byte[] generatedNameIdAndSalt = Encoding.NameId(KK_NAMESPACE).Concatenate(Encoding.BigIntegerToBytes(TEST_SALT));
            CollectionAssert.AreEqual(nameIdAndSalt, generatedNameIdAndSalt);
            // test concat nameIdSalt and hash
            nameIdAndSalt = new byte[] {75, 154, 93, 216, 161, 119, 45, 33, 221, 255, 130, 163, 136, 227, 230, 90, 156, 162, 245, 187, 57, 193, 21, 224, 40, 39, 150, 225, 117, 59, 167, 243};
            generatedNameIdAndSalt = Encoding.Hash(Encoding.NameId(KK_NAMESPACE).Concatenate(Encoding.BigIntegerToBytes(TEST_SALT)));
            CollectionAssert.AreEqual(nameIdAndSalt, generatedNameIdAndSalt);
            // hash domain and namespace
            byte[] generatedHash = Encoding.Hash(System.Text.Encoding.Default.GetBytes(DOMAIN));
            byte[] kkHash = {226, 34, 173, 200, 83, 245, 155, 227, 178, 61, 137, 129, 46, 107, 56, 219, 48, 231, 61, 232, 212, 25, 240, 132, 173, 147, 145, 146, 118, 88, 125, 26};
            CollectionAssert.AreEqual(kkHash, generatedHash);
            byte[] generatedNS = Encoding.Hash(System.Text.Encoding.Default.GetBytes(NS));
            byte[] nsHash = {146, 139, 32, 54, 105, 67, 226, 175, 209, 30, 188, 14, 174, 46, 83, 169, 59, 241, 119, 164, 252, 243, 91, 204, 100, 213, 3, 112, 78, 101, 226, 2};
            CollectionAssert.AreEqual(nsHash, generatedNS);
        }
    }
}