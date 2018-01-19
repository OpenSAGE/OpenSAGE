using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //start the current object playback
    public sealed class Play : InstructionBase
    {
        public override InstructionType Type => InstructionType.Play;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //stop the current object playback
    public sealed class Stop : InstructionBase
    {
        public override InstructionType Type => InstructionType.Stop;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //jump to a specifc frame number
    public sealed class GotoFrame : InstructionBase
    {
        public override InstructionType Type => InstructionType.GotoFrame;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //jump to a specifc frame label
    public sealed class GotoLabel : InstructionBase
    {
        public override InstructionType Type => InstructionType.GotoLabel;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
