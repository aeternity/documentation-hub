using System;

namespace BlockM3.AEternity.SDK.Exceptions
{
    public class AException : Exception
    {
        public AException()
        {
        }

        public AException(string message) : base(message)
        {
        }

        public AException(Exception e) : base(e.ToString(), e)
        {
        }

        public AException(string message, Exception e) : base(message, e)
        {
        }
    }
}