using System.Collections.Generic;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class Function
    {
        public List<InstructionBase> Instructions { get; set; }
        public List<Value> Parameters { get; set; }


    }
}
