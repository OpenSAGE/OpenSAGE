using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //declare a function that will be declared in the current context
    public sealed class DefineFunction : IInstruction
    {
        public InstructionType Type => InstructionType.DefineFunction;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 20;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //return out of the current function
    public sealed class Return : IInstruction
    {
        public InstructionType Type => InstructionType.Return;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //call a method and pass arguments to it. Everything is popped from stack
    public sealed class CallMethodPop : IInstruction
    {
        public InstructionType Type => InstructionType.EA_CallMethodPop;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //call a method and pass arguments to it. Everything is popped from stack
    public sealed class CallNamedMethodPop : IInstruction
    {
        public InstructionType Type => InstructionType.EA_CallNamedMethodPop;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
