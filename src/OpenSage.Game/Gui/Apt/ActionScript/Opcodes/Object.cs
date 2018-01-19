using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //pop one object name from the stack and member name is retrieved from constantpool
    public sealed class GetNamedMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_GetNamedMember;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class SetMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetMember;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetMember;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetProperty : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetProperty;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetStringMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_GetStringMember;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class SetStringMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_SetStringMember;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
