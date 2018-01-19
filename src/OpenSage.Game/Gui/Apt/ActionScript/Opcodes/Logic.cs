using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //pop a value from stack, convert it to boolean and invert it
    public sealed class Not : InstructionBase
    {
        public override InstructionType Type => InstructionType.Not;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //pop two values from stack and do a greater comparison
    public sealed class Greater : InstructionBase
    {
        public override InstructionType Type => InstructionType.Greater;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
