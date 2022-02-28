using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Dom;

namespace OpenSage.Gui.Apt.Script
{
    public class ASObject : HostObject
    {
        public ASObject(VirtualMachine vm, string? classIndicator, string? protoIndicator = null, bool extensible = true) : base(vm, classIndicator, protoIndicator, extensible)
        {
        }

        public override HostObject? GetParent(ExecutionContext ec)
        {
            return null;
        }
    }
}
