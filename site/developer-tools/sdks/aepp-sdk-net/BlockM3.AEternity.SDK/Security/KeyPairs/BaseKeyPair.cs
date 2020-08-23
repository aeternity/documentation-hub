using BlockM3.AEternity.SDK.Utils;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Security.KeyPairs
{
    public class BaseKeyPair : IKeyPair<string>
    {
        public BaseKeyPair(string publicKey, string privateKey)
        {
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }


        public BaseKeyPair(string privateKey)
        {
            string privateKey32 = privateKey.Length == 128 ? privateKey.Substring(0, 64) : privateKey;
            Ed25519PrivateKeyParameters privateKeyParams = new Ed25519PrivateKeyParameters(Hex.Decode(privateKey32), 0);
            Ed25519PublicKeyParameters publicKeyParams = privateKeyParams.GeneratePublicKey();
            byte[] publicBinary = publicKeyParams.GetEncoded();
            byte[] privateBinary = privateKeyParams.GetEncoded();
            PublicKey = Encoding.EncodeCheck(publicBinary, Constants.ApiIdentifiers.ACCOUNT_PUBKEY);
            PrivateKey = Hex.ToHexString(privateBinary) + Hex.ToHexString(publicBinary);
        }

        public string PublicKey { get; }

        public string PrivateKey { get; }


        public static BaseKeyPair Generate()
        {
            RawKeyPair rawKeyPair = RawKeyPair.Generate();
            byte[] publicKey = rawKeyPair.PublicKey;
            byte[] privateKey = rawKeyPair.PrivateKey;
            string aePublicKey = Encoding.EncodeCheck(publicKey, Constants.ApiIdentifiers.ACCOUNT_PUBKEY);
            string privateKeyHex = Hex.ToHexString(privateKey) + Hex.ToHexString(publicKey);
            return new BaseKeyPair(aePublicKey, privateKeyHex);
        }
    }
}