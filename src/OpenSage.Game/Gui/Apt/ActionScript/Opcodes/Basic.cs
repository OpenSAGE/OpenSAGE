using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Declare a pool of constants that will be used in the current scope. Mostly used at start.
    /// </summary>
    public sealed class ConstantPool : InstructionBase
    {
        public override InstructionType Type => InstructionType.ConstantPool;
        public override uint Size => 8;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop a string from stack and print it to console. Used for debug purposes.
    /// </summary>
    public sealed class Trace : InstructionBase
    {
        public override InstructionType Type => InstructionType.Trace;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop a value from stack and store it inside a register
    /// </summary>
    public sealed class SetRegister : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetRegister;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Get a variable from the current object and push it to the stack
    /// </summary>
    public sealed class GetStringVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_GetStringVar;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pops variable name and value from the stack. Then set the variable to that value.
    /// </summary>
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
