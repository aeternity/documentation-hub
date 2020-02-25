using System;
using System.Linq;

namespace BlockM3.AEternity.SDK.Extensions
{
    public static class Bytes
    {
        /**
         * concatenate multiple bytearrays
         *
         * @param bytes
         * @return
         */
        public static byte[] Concatenate(this byte[] start, params byte[][] bytes)
        {
            byte[] final = new byte[bytes.Sum(a => a.Length) + start.Length];
            int pos = 0;
            Buffer.BlockCopy(start, 0, final, pos, start.Length);
            pos += start.Length;
            foreach (byte[] b in bytes)
            {
                Buffer.BlockCopy(b, 0, final, pos, b.Length);
                pos += b.Length;
            }

            return final;
        }

        /**
         * add leading zeros to given byte array
         *
         * @param length
         * @param data
         * @return
         */
        public static byte[] LeftPad(this byte[] data, int length)
        {
            int fill = length - data.Length;
            if (fill > 0)
            {
                byte[] t = new byte[length];
                Buffer.BlockCopy(data, 0, t, fill, data.Length);
                return t;
            }

            return data;
        }

        /**
         * add trailing zeros to given byte array
         *
         * @param length
         * @param data
         * @return
         */
        public static byte[] RightPad(this byte[] data, int length)
        {
            int fill = length - data.Length;
            if (fill > 0)
            {
                byte[] t = new byte[length];
                Buffer.BlockCopy(data, 0, t, 0, data.Length);
                return t;
            }

            return data;
        }
    }
}