using System;
using System.Numerics;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Utils
{
    public static class Crypto
    {
        private static readonly RNGCryptoServiceProvider secureRandom = new RNGCryptoServiceProvider();

        /**
         * generate a securely randomed salt of given size
         *
         * @param size
         * @return
         */
        public static byte[] GenerateSalt(int size)
        {
            byte[] salt = new byte[size];
            secureRandom.GetBytes(salt);
            return salt;
        }

        /** @return positive salt */
        public static BigInteger GenerateNamespaceSalt()
        {
            byte[] salt = new byte[9];
            secureRandom.GetBytes(salt, 0, 8);
            return new BigInteger(salt);
        }

        /**
         * returns a initialized SecureRandom object
         *
         * @return
         */
        public static RNGCryptoServiceProvider GetSecureRandom()
        {
            return secureRandom;
        }

        /**
         * Extract CipherParameters from given privateKey
         *
         * @param privateKey
         * @return
         */
        public static AsymmetricKeyParameter ToPrivateKeyCipherParamsFromHex(this string privateKey)
        {
            return new Ed25519PrivateKeyParameters(Hex.Decode(privateKey.Length == 128 ? privateKey.Substring(0, 64) : privateKey), 0);
        }

        /**
         * extract CipherParameters from given publicKey
         *
         * @param publicKey
         * @return
         */
        public static AsymmetricKeyParameter ToPublicKeyCipherParamsFromHex(this string publicKey)
        {
            return new Ed25519PublicKeyParameters(Hex.Decode(publicKey), 0);
        }

        public static Argon2 GetArgon2FromType(string type, string password)
        {
            if (type.Equals("argon2id", StringComparison.InvariantCultureIgnoreCase))
                return new Argon2id(System.Text.Encoding.UTF8.GetBytes(password));
            if (type.Equals("argon2d", StringComparison.InvariantCultureIgnoreCase))
                return new Argon2d(System.Text.Encoding.UTF8.GetBytes(password));
            return new Argon2i(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }
}