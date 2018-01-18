using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //declare a pool of constants that will be used in the following code
    public sealed class ConstantPool : IInstruction
    {
        public InstructionType Type => InstructionType.ConstantPool;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 8;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //print a string (pop from stack) to console. Used for debugging
    public sealed class Trace : IInstruction
    {
        public InstructionType Type => InstructionType.Trace;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //set a register to a stack value
    public sealed class SetRegister : IInstruction
    {
        public InstructionType Type => InstructionType.SetRegister;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 4;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
