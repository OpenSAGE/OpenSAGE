using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    public sealed class GetUrl : IInstruction
    {
        public InstructionType Type => InstructionType.GetURL2;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 8;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //stack based get url
    public sealed class GetUrl2 : IInstruction
    {
        public InstructionType Type => InstructionType.GetURL2;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
