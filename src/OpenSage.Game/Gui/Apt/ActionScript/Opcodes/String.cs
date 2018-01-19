using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //this Add can also concatenate strings. Pop values from stack
    public sealed class StringConcat : InstructionBase
    {
        public override InstructionType Type => InstructionType.StringConcat;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
