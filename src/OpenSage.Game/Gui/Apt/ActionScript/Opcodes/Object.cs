using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //pop one object name from the stack and member name is retrieved from constantpool
    public sealed class GetNamedMember : IInstruction
    {
        public InstructionType Type => InstructionType.EA_GetNamedMember;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 1;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class SetMember : IInstruction
    {
        public InstructionType Type => InstructionType.SetMember;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetMember : IInstruction
    {
        public InstructionType Type => InstructionType.SetMember;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetStringVar : IInstruction
    {
        public InstructionType Type => InstructionType.EA_GetStringVar;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 4;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetStringMember : IInstruction
    {
        public InstructionType Type => InstructionType.EA_GetStringMember;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 4;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
