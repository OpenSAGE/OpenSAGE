using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Start playback of the current object (must be sprite)
    /// </summary>
    public sealed class Play : InstructionBase
    {
        public override InstructionType Type => InstructionType.Play;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Stop playback of the current object (must be sprite)
    /// </summary>
    public sealed class Stop : InstructionBase
    {
        public override InstructionType Type => InstructionType.Stop;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Jump to a specific frame number (must be sprite)
    /// </summary>
    public sealed class GotoFrame : InstructionBase
    {
        public override InstructionType Type => InstructionType.GotoFrame;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Jump to a labeled frame (must be sprite)
    /// </summary>
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
