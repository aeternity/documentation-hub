using System;

namespace BlockM3.AEternity.SDK.Sophia.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.GenericParameter)]
    public class SophiaRecordAttribute : Attribute, ISophiaAttribute
    {
        public SophiaRecordAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}