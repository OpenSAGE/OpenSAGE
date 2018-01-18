using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //this add can also concatenate strings. Pop values from stack
    public sealed class Add2 : IInstruction
    {
        public InstructionType Type => InstructionType.Add2;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //this equal can also compare strings
    public sealed class Equals2 : IInstruction
    {
        public InstructionType Type => InstructionType.Equals2;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //pop a value from stack, convert it to boolean and invert it
    public sealed class Not : IInstruction
    {
        public InstructionType Type => InstructionType.Not;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
