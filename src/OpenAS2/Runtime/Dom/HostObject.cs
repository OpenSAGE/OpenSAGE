using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace OpenAS2.Runtime.Dom
{
    public abstract class HostObject : ESObject
    {
        protected HostObject(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces) : base(classIndicator, extensible, prototype, interfaces)
        {
        }
        protected HostObject(VirtualMachine vm, string? classIndicator, string? protoIndicator = null, bool extensible = true) : base(vm, classIndicator, protoIndicator, extensible)
        {
        }

        public abstract HostObject? GetParent();

    }

}
