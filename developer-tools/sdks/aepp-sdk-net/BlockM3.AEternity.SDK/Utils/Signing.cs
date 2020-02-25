using System;
using BlockM3.AEternity.SDK.Extensions;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Utilities.Encoders;

namespace BlockM3.AEternity.SDK.Utils
{
    public static class Signing
    {
        public static byte[] Sign(string data, string privateKey)
        {
            return Sign(Hex.Decode(data), privateKey);
        }

        public static byte[] Sign(byte[] data, string privateKey)
        {
            Ed25519Signer signer = new Ed25519Signer();
            signer.Init(true, privateKey.ToPrivateKeyCipherParamsFromHex());
            signer.BlockUpdate(data, 0, data.Length);
            return signer.GenerateSignature();
        }

        public static byte[] SignMessage(string message, string privateKey)
        {
            return Sign(MessageToBinary(message), privateKey);
        }

        public static bool Verify(string data, byte[] signature, string publicKey)
        {
            byte[] dataBinary = Hex.Decode(data);
            return Verify(dataBinary, signature, publicKey);
        }

        public static bool Verify(byte[] data, byte[] signature, string publicKey)
        {
            Ed25519Signer verifier = new Ed25519Signer();
            verifier.Init(false, publicKey.ToPublicKeyCipherParamsFromHex());
            verifier.BlockUpdate(data, 0, data.Length);
            return verifier.VerifySignature(signature);
        }

        public static bool VerifyMessage(string message, byte[] signature, string publicKey)
        {
            return Verify(MessageToBinary(message), signature, publicKey);
        }

        private static byte[] MessageToBinary(string message)
        {
            byte[] p = System.Text.Encoding.UTF8.GetBytes(Constants.BaseConstants.AETERNITY_MESSAGE_PREFIX);
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(message);
            if (msg.Length > Constants.BaseConstants.MAX_MESSAGE_LENGTH)
            {
                throw new ArgumentException($"Message exceeds allow maximum size {Constants.BaseConstants.MAX_MESSAGE_LENGTH}");
            }

            byte[] pLength = {(byte) p.Length};
            byte[] msgLength = {(byte) msg.Length};
            return pLength.Concatenate(p, msgLength, msg);
        }
    }
}