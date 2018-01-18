using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //start the current object playback
    public sealed class Play : IInstruction
    {
        public InstructionType Type => InstructionType.Play;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //stop the current object playback
    public sealed class Stop : IInstruction
    {
        public InstructionType Type => InstructionType.Stop;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 0;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //jump to a specifc frame label
    public sealed class GotoLabel : IInstruction
    {
        public InstructionType Type => InstructionType.Stop;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;
        public uint Size => 4;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
