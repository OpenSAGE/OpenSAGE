using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Used to get variables from the engine (?)
    /// </summary>
    public sealed class GetUrl : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetURL;
        public override uint Size => 8;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Used to get variables from the engine (?) (stack based)
    /// </summary>
    public sealed class GetUrl2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetURL2;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
