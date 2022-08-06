using System;
using System.Linq;

namespace OpenSage.Data.Ini
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class IniEnumAttribute : Attribute
    {
        public string[] Names { get; }

        public IniEnumAttribute(string name)
        {
            Names = new[] { name };
        }

        public IniEnumAttribute(string name, params string[] otherNames)
        {
            Names = new[] { name }.Union(otherNames).ToArray();
        }
    }
}
