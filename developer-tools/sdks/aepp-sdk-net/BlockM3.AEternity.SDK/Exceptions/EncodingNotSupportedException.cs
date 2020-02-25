using System;

namespace BlockM3.AEternity.SDK.Exceptions
{
    public class EncodingNotSupportedException : ArgumentException
    {
        public EncodingNotSupportedException()
        {
        }

        public EncodingNotSupportedException(string message) : base(message)
        {
        }
    }
}