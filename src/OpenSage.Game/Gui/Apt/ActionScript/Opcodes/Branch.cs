using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pops a bool from the stack. If the bool is true jump to the byte offset (parameter)
    /// </summary>
    public sealed class BranchIfTrue : InstructionBase
    {
        public override InstructionType Type => InstructionType.BranchIfTtrue;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    // <summary>
    /// Jump to the byte offset (parameter)
    /// </summary>
    public sealed class BranchAlways : InstructionBase
    {
        public override InstructionType Type => InstructionType.BranchAlways;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
