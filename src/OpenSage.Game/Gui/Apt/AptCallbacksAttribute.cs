using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class AptCallbacksAttribute : Attribute
    {
        public SageGame[] Games { get; private set; }

        public AptCallbacksAttribute(params SageGame[] games)
        {
            Games = games;
        }
    }
}
