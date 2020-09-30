using System;

namespace BlockM3.AEternity.SDK.Sophia.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.GenericParameter)]
    public class SophiaOracleQueryAttribute : Attribute, ISophiaAttribute
    {
    }
}