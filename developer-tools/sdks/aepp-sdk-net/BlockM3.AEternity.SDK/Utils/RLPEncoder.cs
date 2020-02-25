using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Nethereum.RLP;

namespace BlockM3.AEternity.SDK.Utils
{
    public class RLPEncoder
    {
        private readonly List<byte[]> acc = new List<byte[]>();

        public void AddInt(int value)
        {
            acc.Add(RLP.EncodeElement(value.ToBytesForRLPEncoding()));
        }

        public byte[] Encode()
        {
            return RLP.EncodeList(acc.ToArray());
        }

        public void AddNumber(BigInteger? value)
        {
            acc.Add(RLP.EncodeElement(CheckZeroAndWriteValue(value)));
        }

        public void AddString(string str)
        {
            acc.Add(RLP.EncodeElement(str.ToBytesForRLPEncoding()));
        }

        public void AddByteArray(byte[] array)
        {
            acc.Add(RLP.EncodeElement(array));
        }

        public void AddList(RLPEncoder list)
        {
            acc.Add(RLP.EncodeList(list.acc.ToArray()));
        }

        public static byte[] CheckZeroAndWriteValue(BigInteger? value)
        {
            if (!value.HasValue || value == 0)
                return new byte[] {0};
            return ToBytesFromNumber(value.Value.ToByteArray());
        }

        private static byte[] ToBytesFromNumber(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                bytes = bytes.Reverse().ToArray();

            var trimmed = new List<byte>();
            var previousByteWasZero = true;

            for (var i = 0; i < bytes.Length; i++)
            {
                if (previousByteWasZero && bytes[i] == 0)
                    continue;

                previousByteWasZero = false;
                trimmed.Add(bytes[i]);
            }

            return trimmed.ToArray();
        }
    }
}