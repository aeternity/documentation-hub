using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BlockM3.AEternity.SDK.Exceptions;
using BlockM3.AEternity.SDK.Extensions;
using BlockM3.AEternity.SDK.Models;
using BlockM3.AEternity.SDK.Security.KeyPairs;
using NBitcoin.DataEncoders;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Base64Encoder = NBitcoin.DataEncoders.Base64Encoder;

namespace BlockM3.AEternity.SDK.Utils
{
/** this util class provides all encoding related methods */
    public static class Encoding
    {
        /**
         * encode input with encoding determined from given identifier String
         *
         * @param input
         * @param identifier see @{@link ApiIdentifiers}
         * @return
         * @throws EncodingNotSupportedException
         */
        public static string EncodeCheck(byte[] input, string identifier)
        {
            if (identifier != null && identifier.Trim().Length > 0)
            {
                string encoded;
                // determine encoding from given identifier
                if (Constants.ApiIdentifiers.IDENTIFIERS_B58_LIST.Contains(identifier))
                {
                    encoded = EncodeCheck(input, EncodingType.BASE58);
                }
                else if (Constants.ApiIdentifiers.IDENTIFIERS_B64_LIST.Contains(identifier))
                {
                    encoded = EncodeCheck(input, EncodingType.BASE64);
                }
                else
                {
                    throw new ArgumentException("unknown identifier");
                }

                return identifier + "_" + encoded;
            }

            // if we get to this point, we cannot determine the encoding
            throw new EncodingNotSupportedException($"Cannot determine encoding type from given identifier {identifier}");
        }

        /**
         * encode input with given encodingType
         *
         * @param input
         * @param encodingType
         * @return
         * @throws EncodingNotSupportedException
         */
        public static string EncodeCheck(byte[] input, EncodingType encodingType)
        {
            switch (encodingType)
            {
                case EncodingType.BASE58:
                    return EncodeBase58Check(input);
                case EncodingType.BASE64:
                    return EncodeBase64Check(input);
                default:
                    throw new EncodingNotSupportedException($"Encoding {encodingType} is currently not supported");
            }
        }

        /**
         * decode input which is combined of the identifier and the encoded string (e.g. ak_[encoded])
         *
         * @param input
         * @return
         */
        public static byte[] DecodeCheckWithIdentifier(string input)
        {
            string[] splitted = input.Split('_');
            if (splitted.Length != 2)
            {
                throw new ArgumentException("input has wrong format");
            }

            string identifier = splitted[0];
            string encoded = splitted[1];
            return DecodeCheck(encoded, identifier);
        }

        /**
         * decode input with encoding determined from given identifier String
         *
         * @param input
         * @param identifier see @{@link ApiIdentifiers}
         * @return
         * @throws EncodingNotSupportedException
         */
        private static byte[] DecodeCheck(string input, string identifier)
        {
            if (identifier != null && identifier.Trim().Length > 0)
            {
                // determine encoding from given identifier
                if (Constants.ApiIdentifiers.IDENTIFIERS_B58_LIST.Contains(identifier))
                {
                    return DecodeCheck(input, EncodingType.BASE58);
                }
                else if (Constants.ApiIdentifiers.IDENTIFIERS_B64_LIST.Contains(identifier))
                {
                    return DecodeCheck(input, EncodingType.BASE64);
                }
            }

            // if we get to this point, we cannot determine the encoding
            throw new EncodingNotSupportedException($"Cannot determine encoding type from given identifier {identifier}");
        }

        /**
         * decode input with given encodingType
         *
         * @param input
         * @param encodingType
         * @return
         * @throws EncodingNotSupportedException
         */
        public static byte[] DecodeCheck(string input, EncodingType encodingType)
        {
            switch (encodingType)
            {
                case EncodingType.BASE58:
                    return DecodeBase58Check(input);
                case EncodingType.BASE64:
                    return DecodeBase64Check(input);
                default:
                    throw new EncodingNotSupportedException($"Encoding {encodingType} is currently not supported");
            }
        }

        /**
         * @param input
         * @param serializationTag see {@link SerializationTags}
         * @return
         */
        public static byte[] DecodeCheckAndTag(string input, int serializationTag)
        {
            byte[] tag = new BigInteger(serializationTag).ToByteArray();
            byte[] decoded = DecodeCheckWithIdentifier(input);
            return tag.Concatenate(decoded);
        }

        private static string EncodeBase58Check(byte[] input)
        {
            SHA256Managed m = new SHA256Managed();
            byte[] re = m.ComputeHash(input);
            re = m.ComputeHash(re);
            byte[] checksum = new byte[4];
            Buffer.BlockCopy(re, 0, checksum, 0, 4);
            byte[] base58checksum = input.Concatenate(checksum);
            Base58Encoder enc = new Base58Encoder();
            return enc.EncodeData(base58checksum);
        }

        private static byte[] DecodeBase58Check(string base58encoded)
        {
            Base58CheckEncoder cj = new Base58CheckEncoder();
            return cj.DecodeData(base58encoded);
        }

        private static string EncodeBase64Check(byte[] input)
        {
            SHA256Managed m = new SHA256Managed();
            byte[] re = m.ComputeHash(input);
            re = m.ComputeHash(re);
            byte[] checksum = new byte[4];
            Buffer.BlockCopy(re, 0, checksum, 0, 4);
            byte[] base64checksum = input.Concatenate(checksum);
            Base64Encoder enc = new Base64Encoder();
            return enc.EncodeData(base64checksum);
        }

        private static byte[] DecodeBase64Check(string base64encoded)
        {
            // TODO logic copied from Base58.decodeChecked -> can this be reused
            // here?
            Base64Encoder enc = new Base64Encoder();
            byte[] decoded = enc.DecodeData(base64encoded);
            if (decoded.Length < 4) throw new FormatException("Input too short");
            byte[] data = new byte[decoded.Length - 4];
            byte[] checksum = new byte[4];
            Buffer.BlockCopy(decoded, 0, data, 0, data.Length);
            Buffer.BlockCopy(decoded, data.Length, checksum, 0, 4);
            SHA256Managed m = new SHA256Managed();
            byte[] re = m.ComputeHash(data);
            re = m.ComputeHash(re);
            byte[] actualChecksum = new byte[4];
            Buffer.BlockCopy(re, 0, actualChecksum, 0, 4);
            if (!Enumerable.SequenceEqual(checksum, actualChecksum))
                throw new FormatException("Checksum does not validate");
            return data;
        }

        /**
         * check if the given address has the correct length
         *
         * @param address
         * @return
         */
        public static bool IsAddressValid(string address)
        {
            bool isValid;
            try
            {
                isValid = DecodeBase58Check(AssertedType(address, Constants.ApiIdentifiers.ACCOUNT_PUBKEY)).Length == 32;
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }

        /**
         * @param base58CheckAddress
         * @return the readable public key as hex
         */
        public static string AddressToHex(string base58CheckAddress)
        {
            return Constants.BaseConstants.PREFIX_ZERO_X + Hex.ToHexString(DecodeBase58Check(AssertedType(base58CheckAddress, Constants.ApiIdentifiers.ACCOUNT_PUBKEY)));
        }

        private static string AssertedType(string data, string type)
        {
            if (Regex.IsMatch(data, "^" + type + "_.+$"))
            {
                return data.Split('_')[1];
            }

            throw new ArgumentException("Data doesn't match expected type " + type);
        }

        public static string HashEncode(byte[] input, string identifier)
        {
            byte[] hash = Hash(input);
            return EncodeCheck(hash, identifier);
        }

        public static byte[] Hash(byte[] input)
        {
            Blake2bDigest digest = new Blake2bDigest(256);
            digest.BlockUpdate(input, 0, input.Length);
            byte[] hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);
            return hash;
        }

        public static BaseKeyPair CreateBaseKeyPair(RawKeyPair rawKeyPair)
        {
            string privateKey = Hex.ToHexString(rawKeyPair.PrivateKey) + Hex.ToHexString(rawKeyPair.PublicKey);
            string publicKey = EncodeCheck(rawKeyPair.PublicKey, Constants.ApiIdentifiers.ACCOUNT_PUBKEY);
            return new BaseKeyPair(publicKey, privateKey);
        }

        public static string GenerateCommitmentHash(string name, BigInteger salt)
        {
            return EncodeCheck(Hash(NameId(name).Concatenate(BigIntegerToBytes(salt))), Constants.ApiIdentifiers.COMMITMENT);
        }

        /**
         * if salt is not 32 byte array, copy salt at the end of the buffer
         *
         * @param salt
         * @return
         */
        public static byte[] BigIntegerToBytes(BigInteger value)
        {
            byte[] array = value.ToByteArray();
            if (BitConverter.IsLittleEndian)
                array = value.ToByteArray().Reverse().ToArray();
            if (array.Length < 32)
            {
                byte[] buffer = new byte[32];
                for (int i = array.Length - 1; i > -1; i--)
                {
                    buffer[32 - (array.Length - i)] = array[i];
                }

                return buffer;
            }
            else
            {
                return array;
            }
        }

        public static string Normalize(string domainName)
        {
            IdnMapping map = new IdnMapping();
            map.UseStd3AsciiRules = true;
            return map.GetAscii(domainName);
        }

        public static byte[] NameId(string domainName)
        {
            string normalizedDomainName = Normalize(domainName);
            byte[] buffer = new byte[32];
            Arrays.Fill(buffer, 0);
            if (string.IsNullOrEmpty(domainName))
                return buffer;
            string[] labels = normalizedDomainName.Split('.');
            for (int i = 0; i < labels.Length; i++)
            {
                buffer = Hash(buffer.Concatenate(Hash(System.Text.Encoding.Default.GetBytes(labels[i])))); //mpiva Default encoding is allright? non deterministic way should be used?
            }

            return buffer;
        }

        public static string ComputeTxHash(string encodedSignedTx)
        {
            byte[] signed = DecodeCheckWithIdentifier(encodedSignedTx);
            return HashEncode(signed, Constants.ApiIdentifiers.TRANSACTION_HASH);
        }

        public static string EncodeSignedTransaction(byte[] sig, byte[] binaryTx)
        {
            RLPEncoder enc = new RLPEncoder();
            enc.AddInt(Constants.SerializationTags.OBJECT_TAG_SIGNED_TRANSACTION);
            enc.AddInt(Constants.SerializationTags.VSN);
            RLPEncoder sublist = new RLPEncoder();
            sublist.AddByteArray(sig);
            enc.AddList(sublist);
            enc.AddByteArray(binaryTx);
            return EncodeCheck(enc.Encode(), Constants.ApiIdentifiers.TRANSACTION);
        }


        public static string EncodeContractId(string publickey, ulong nonce)
        {
            return HashEncode(DecodeCheckWithIdentifier(publickey).Concatenate(RLPEncoder.CheckZeroAndWriteValue(nonce)), Constants.ApiIdentifiers.CONTRACT_PUBKEY);

        }
        public static string EncodeQueryId(string publickey, string oracleid, ulong nonce)
        {
            return HashEncode(DecodeCheckWithIdentifier(publickey).Concatenate(BigIntegerToBytes(nonce), DecodeCheckWithIdentifier(oracleid)), Constants.ApiIdentifiers.ORACLE_QUERY_ID);
        }
    }
}