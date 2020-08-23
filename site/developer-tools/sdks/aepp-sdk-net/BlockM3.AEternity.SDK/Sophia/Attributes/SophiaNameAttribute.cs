using System;

namespace BlockM3.AEternity.SDK.Sophia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SophiaNameAttribute : Attribute, ISophiaAttribute
    {
        public SophiaNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
    }
}