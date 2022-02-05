using System;

namespace OpenAS2.HostObjects
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AptCallbacksAttribute : Attribute
    {
        public SageGame[] Games { get; }

        public AptCallbacksAttribute(params SageGame[] games)
        {
            Games = games;
        }
    }
}
