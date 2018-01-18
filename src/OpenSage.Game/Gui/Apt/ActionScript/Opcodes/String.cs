using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //this Add can also concatenate strings. Pop values from stack
    public sealed class StringConcat : IInstruction
    {
        public InstructionType Type => InstructionType.StringConcat;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
