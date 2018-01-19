using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Push a string to the stack
    /// </summary>
    public sealed class PushString : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushString;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push a float to the stack
    /// </summary>
    public sealed class PushFloat : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushFloat;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Read a constant from the pool and push it to stack
    /// </summary>
    public sealed class PushConstantByte : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushConstantByte;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Read a byte and push it to the stack
    /// </summary>
    public sealed class PushByte : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushByte;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Read the variable name from the pool and push that variable's value to the stack
    /// </summary>
    public sealed class PushValueOfVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushValueOfVar;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push an undefined value to the stack
    /// </summary>
    public sealed class PushUndefined : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushUndefined;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push a boolean with value false to the stack
    /// </summary>
    public sealed class PushFalse : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushFalse;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push an integer with value zero to the stack
    /// </summary>
    public sealed class PushZero : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushZero;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push the current object to the stack
    /// </summary>
    public sealed class PushThis : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushThis;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push the current object to the stack
    /// </summary>
    public sealed class PushThisVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushThisVar;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push an integer with value one to the stack
    /// </summary>
    public sealed class PushOne : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushOne;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push a boolean with value true to the stack
    /// </summary>
    public sealed class PushTrue : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushTrue;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Get multiple variables from the pool and push them to the stack
    /// </summary>
    public sealed class PushData : InstructionBase
    {
        public override InstructionType Type => InstructionType.PushData;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push a zero variable to the stack
    /// </summary>
    public sealed class ZeroVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_ZeroVar;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Push the global object to the stack
    /// </summary>
    public sealed class PushGlobalVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushGlobalVar;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop a value from the stack and push it twice
    /// </summary>
    public sealed class PushDuplicate : InstructionBase
    {
        public override InstructionType Type => InstructionType.PushDuplicate;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop a value from the stack
    /// </summary>
    public sealed class Pop : InstructionBase
    {
        public override InstructionType Type => InstructionType.Pop;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
