using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Utils;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Security.KeyPairs
{
    public class RawKeyPair : IKeyPair<byte[]>
    {
        public RawKeyPair(byte[] publicKey, byte[] privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public RawKeyPair(string privateKey)
        {
            string privateKey32 = privateKey.Length == 128 ? privateKey.Substring(0, 64) : privateKey;
            Ed25519PrivateKeyParameters privateKeyParams = new Ed25519PrivateKeyParameters(Hex.Decode(privateKey32), 0);
            Ed25519PublicKeyParameters publicKeyParams = privateKeyParams.GeneratePublicKey();
            PublicKey = publicKeyParams.GetEncoded();
            PrivateKey = privateKeyParams.GetEncoded();
        }

        public byte[] ConcatenatedPrivateKey => PrivateKey.Concatenate(PublicKey);

        public byte[] PublicKey { get; }

        public byte[] PrivateKey { get; }

        public string GetPublicKey()
        {
            return Encoding.EncodeCheck(PublicKey, Constants.ApiIdentifiers.ACCOUNT_PUBKEY);
        }

        public static RawKeyPair Generate()
        {
            Ed25519KeyPairGenerator keyPairGenerator = new Ed25519KeyPairGenerator();
            keyPairGenerator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
            AsymmetricCipherKeyPair asymmetricCipherKeyPair = keyPairGenerator.GenerateKeyPair();
            Ed25519PublicKeyParameters publicKeyParams = (Ed25519PublicKeyParameters) asymmetricCipherKeyPair.Public;
            Ed25519PrivateKeyParameters privateKeyParams = (Ed25519PrivateKeyParameters) asymmetricCipherKeyPair.Private;
            byte[] publicKey = publicKeyParams.GetEncoded();
            byte[] privateKey = privateKeyParams.GetEncoded();
            return new RawKeyPair(publicKey, privateKey);
        }
    }
}