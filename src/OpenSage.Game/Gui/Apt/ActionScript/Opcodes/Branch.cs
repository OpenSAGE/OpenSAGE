using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //branch to a relative offset if the value on stack is true
    public sealed class BranchIfTrue : InstructionBase
    {
        public override InstructionType Type => InstructionType.BranchIfTtrue;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //branch to a relative offset
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
