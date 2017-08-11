using System;

namespace OpenZH.Data.Ini
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class IniEnumAttribute : Attribute
    {
        public string Name { get; }

        public IniEnumAttribute(string name)
        {
            Name = name;
        }
    }
}
