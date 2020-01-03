using System;

namespace OpenSage.Gui.Apt
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
