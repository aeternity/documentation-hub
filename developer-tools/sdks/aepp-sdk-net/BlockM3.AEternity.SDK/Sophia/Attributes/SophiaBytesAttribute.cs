using System;

namespace BlockM3.AEternity.SDK.Sophia.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.GenericParameter)]
    public class SophiaBytesAttribute : Attribute, ISophiaAttribute
    {
        public SophiaBytesAttribute(int length)
        {
            Length = length;
        }

        public int Length { get; set; }
    }
}