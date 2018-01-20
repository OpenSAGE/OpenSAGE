using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pop two strings from the stack and concatenate them
    /// </summary>
    public sealed class StringConcat : InstructionBase
    {
        public override InstructionType Type => InstructionType.StringConcat;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
