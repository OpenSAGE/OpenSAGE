using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    public sealed class GetUrl : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetURL2;
        public override uint Size => 8;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //stack based get url
    public sealed class GetUrl2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetURL2;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
