using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //push a string to stack
    public sealed class PushString : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushString;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 4;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //read a constant entry and push it to stack
    public sealed class PushConstantByte : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushConstantByte;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 1;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //read a constant entry and push the variable with that name to the stack
    public sealed class PushValueOfVar : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushValueOfVar;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 1;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push an undefined value to the stack
    public sealed class PushUndefined : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushUndefined;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a false boolean to the stack
    public sealed class PushFalse : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushFalse;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a zero integer to the stack
    public sealed class PushZero : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushZero;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a one integer to the stack
    public sealed class PushOne : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushOne;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a true boolean to the stack
    public sealed class PushTrue : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushTrue;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //get multiple variables and push them to the stack
    public sealed class PushData : IInstruction
    {
        public InstructionType Type => InstructionType.PushData;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push the global variable to the stack
    public sealed class PushGlobalVar : IInstruction
    {
        public InstructionType Type => InstructionType.EA_PushGlobalVar;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push the variable currently on top once more
    public sealed class PushDuplicate : IInstruction
    {
        public InstructionType Type => InstructionType.PushDuplicate;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //pop a value from the stack
    public sealed class Pop : IInstruction
    {
        public InstructionType Type => InstructionType.Pop;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
