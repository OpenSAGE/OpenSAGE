using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //declare a function that will be declared in the current context
    public sealed class DefineFunction : InstructionBase
    {
        public override InstructionType Type => InstructionType.DefineFunction;
        public override uint Size => 20;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //return out of the current function
    public sealed class Return : InstructionBase
    {
        public override InstructionType Type => InstructionType.Return;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //call a method and pass arguments to it. Everything is popped from stack
    public sealed class CallMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallMethodPop;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //call a method and pass arguments to it. Everything is popped from stack
    public sealed class CallNamedMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedMethodPop;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
