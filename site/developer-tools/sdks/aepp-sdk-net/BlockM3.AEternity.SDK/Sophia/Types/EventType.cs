using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BlockM3.AEternity.SDK.Sophia.Attributes;

namespace BlockM3.AEternity.SDK.Sophia.Types
{
    public class EventType : SophiaType
    {
        public EventType(string mapname, string name, List<SophiaType> types) : base(mapname)
        {
            Name = name;
            FieldTypes = types;
        }

        public override string SophiaBaseName => "event";
        public override string SophiaName => Name + " (" + string.Join(", ", FieldTypes.Select(a => a.SophiaName)) + ")";

        public List<SophiaType> FieldTypes { get; }
        public string Name { get; }

        public override string Serialize(object o, Type t)
        {
            SophiaEventAttribute rec = t.GetCustomAttribute<SophiaEventAttribute>();
            if (!t.IsClass)
                throw new ArgumentException("Events can be only classes");
            if (rec == null)
                throw new ArgumentException("Events classes should have the SophiaEvent Attribute");
            StringBuilder bld = new StringBuilder();
            bld.Append(Name);
            bld.Append("(");
            List<(string, PropertyInfo)> props = OrderProps(t.GetTypeInfo());
            bool add = false;
            if (props.Count < FieldTypes.Count)
            {
                throw new ArgumentException($"Unable to serialize event, the class has less properties than the definition");
            }

            for (int x = 0; x < FieldTypes.Count; x++)
            {
                add = true;
                PropertyInfo p = props[x].Item2;
                bld.Append(FieldTypes[x].Serialize(p.GetValue(o), p.PropertyType));
                bld.Append(", ");
            }

            if (add)
                bld.Remove(bld.Length - 2, 2);
            bld.Append(")");
            return bld.ToString();
        }

        public override object Deserialize(string value, Type t)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
                value = value.Substring(1, value.Length - 2);
            SophiaEventAttribute rec = t.GetCustomAttribute<SophiaEventAttribute>();
            if (!t.IsClass)
                throw new ArgumentException("Events can be only classes");
            if (rec == null)
                throw new ArgumentException("Events classes should have the SophiaEvent Attribute");
            value = value.Trim();
            int idx = value.IndexOf('(');
            if (idx > -1)
                value = value.Substring(idx);
            if (value.StartsWith("(") && value.EndsWith(")"))
                value = value.Substring(1, value.Length - 2).Trim();
            string[] dicsplits = SophiaMapper.SplitByComma(value);
            List<(string, PropertyInfo)> props = OrderProps(t.GetTypeInfo());
            object o = Activator.CreateInstance(t);
            if (props.Count < FieldTypes.Count || dicsplits.Length != FieldTypes.Count)
            {
                throw new ArgumentException($"Unable to deserialize event, the class has less properties than the definition");
            }

            for (int x = 0; x < FieldTypes.Count; x++)
            {
                PropertyInfo p = props[x].Item2;
                p.SetValue(o, FieldTypes[x].Deserialize(dicsplits[x], p.PropertyType));
            }

            return o;
        }
    }
}