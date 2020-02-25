using System;
using System.Text.RegularExpressions;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class OptionType : SophiaType
    {
        public OptionType(string mapname) : base(mapname)
        {
        }

        public override string SophiaBaseName => "option('a)";
        public override string SophiaName => SophiaBaseName;

        public override string Serialize(object o, Type t)
        {
            if (o == null)
                return "None";
            if (t.IsEnum)
                return Enum.GetName(t, o) + "(" + Convert.ToInt32(o) + ")";
            Type enumType = GetUnderlyingEnum(t);
            if (enumType != null)
                return Enum.GetName(enumType, o) + "(" + Convert.ToInt32(o) + ")";
            return base.Serialize(o, t);
        }

        public override object Deserialize(string value, Type t)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            if (value.ToLowerInvariant() == "none")
            {
                Type enumType = GetUnderlyingEnum(t);
                if (enumType != null)
                    return null;
                throw new ArgumentException($"The object type should be a nullable enum, since the value provided is 'None'");
            }

            Match m = numReg.Match(value);
            if (m.Success)
            {
                object obj = Enum.ToObject(t, int.Parse(m.Groups[1].Value));
                if (t.IsEnum || GetUnderlyingEnum(t) != null)
                    return obj;
            }

            return base.Deserialize(value, t);
        }
    }
}