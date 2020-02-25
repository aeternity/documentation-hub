using System;
using System.Collections.Generic;

namespace BlockM3.AEternity.SDK.Sophia.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.GenericParameter)]
    public class SophiaGenericAttribute : Attribute, ISophiaAttribute
    {
        public SophiaGenericAttribute(Dictionary<int, ISophiaAttribute> attributes)
        {
            Attributes = attributes;
        }

        public Dictionary<int, ISophiaAttribute> Attributes { get; set; }
    }
}