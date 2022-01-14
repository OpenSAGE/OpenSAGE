using System;
using System.Collections.Generic;

namespace OpenAS2.Runtime.Dom
{
    public abstract class ExternObject : HostObject
    {
        protected ExternObject(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces) : base(classIndicator, extensible, prototype, interfaces)
        {
        }
    }
}
