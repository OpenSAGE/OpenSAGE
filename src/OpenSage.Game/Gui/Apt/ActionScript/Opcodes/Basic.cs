using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //declare a pool of constants that will be used in the following code
    public sealed class ConstantPool : InstructionBase
    {
        public override InstructionType Type => InstructionType.ConstantPool;
        public override uint Size => 8;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //print a string (pop from stack) to console. Used for debugging
    public sealed class Trace : InstructionBase
    {
        public override InstructionType Type => InstructionType.Trace;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //set a register to a stack value
    public sealed class SetRegister : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetRegister;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetStringVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_GetStringVar;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //pop variable name and value from the stack and set it
    public sealed class SetVariable : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetVariable;
        public override uint Size => 0;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
