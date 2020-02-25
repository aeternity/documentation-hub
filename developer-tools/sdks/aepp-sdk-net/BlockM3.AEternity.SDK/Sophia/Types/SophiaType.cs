using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using BlockM3.AEternity.SDK.Sophia.Attributes;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public abstract class SophiaType
    {

        internal static Regex numReg = new Regex("\\((.*)\\)", RegexOptions.Compiled);

        public SophiaType(string mapname)
        {
            MapName = mapname;
        }

        public abstract string SophiaBaseName { get; }
        public string MapName { get; set; }
        public abstract string SophiaName { get; }

        public virtual string Serialize(object o, Type t)
        {
            throw new ArgumentException($"Expecting a '{SophiaBaseName}' sophia type for object of type {t.Name}");
        }

        public static List<(string, PropertyInfo)> OrderProps(TypeInfo tinfo)
        {
            List<(int, string, PropertyInfo)> order = new List<(int, string, PropertyInfo)>();
            foreach (PropertyInfo p in tinfo.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                SophiaOrderAttribute idx = p.GetCustomAttribute<SophiaOrderAttribute>();
                SophiaNameAttribute nm = p.GetCustomAttribute<SophiaNameAttribute>();
                string name = p.Name.ToLowerInvariant();
                if (nm != null)
                    name = nm.Name;
                order.Add(idx != null ? (idx.Index, name, p) : (int.MaxValue, name, p));
            }

            return order.OrderBy(a => a.Item1).Select(a => (a.Item2, a.Item3)).ToList();
        }

        public static Type GetUnderlyingEnum(Type t)
        {
            Type u = Nullable.GetUnderlyingType(t);
            if (u != null && u.IsEnum)
                return u;
            return null;
        }

        public virtual object Deserialize(string value, Type t)
        {
            throw new ArgumentException($"Expecting a '{SophiaBaseName}' sophia type for object of type {t.Name}");
        }

        public virtual object FromBigInteger(BigInteger b)
        {
            throw new NotSupportedException();
        }

        internal byte[] BigIntegerToByteArray(BigInteger b)
        {
            byte[] data = b.ToByteArray();
            if (data[data.Length - 1] == 0)
            {
                byte[] dta = new byte[data.Length - 1];
                Buffer.BlockCopy(data, 0, dta, 0, dta.Length);
                data = dta;
            }

            if (BitConverter.IsLittleEndian)
                data = data.Reverse().ToArray();
            return data;
        }

        public string Serialize<T>(T o) => Serialize(o, typeof(T));
        public T Deserialize<T>(string value) => (T) Deserialize(value, typeof(T));
    }
}