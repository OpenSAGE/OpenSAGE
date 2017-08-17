using System;

namespace OpenSage.Data.Ini
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class IniEnumAttribute : Attribute
    {
        public string Name { get; }

        public IniEnumAttribute(string name)
        {
            Name = name;
        }
    }
}
