using System;

namespace BlockM3.AEternity.SDK.Sophia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SophiaOrderAttribute : Attribute, ISophiaAttribute
    {
        public SophiaOrderAttribute(int index)
        {
            Index = index;
        }

        public int Index { get; set; }
    }
}