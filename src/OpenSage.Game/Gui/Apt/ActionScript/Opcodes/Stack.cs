using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //push a string to stack
    public sealed class PushString : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushString;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a float to stack
    public sealed class PushFloat : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushFloat;
        public override uint Size => 4;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //read a constant entry and push it to stack
    public sealed class PushConstantByte : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushConstantByte;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //read a byte and push it to the stack
    public sealed class PushByte : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushByte;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //read a constant entry and push the variable with that name to the stack
    public sealed class PushValueOfVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushValueOfVar;
        public override uint Size => 1;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push an undefined value to the stack
    public sealed class PushUndefined : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushUndefined;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a false boolean to the stack
    public sealed class PushFalse : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushFalse;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a zero integer to the stack
    public sealed class PushZero : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushZero;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push the current object to the stack
    public sealed class PushThis : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushThis;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push the current object to the stack
    public sealed class PushThisVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushThisVar;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a one integer to the stack
    public sealed class PushOne : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushOne;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push a true boolean to the stack
    public sealed class PushTrue : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushTrue;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //get multiple variables and push them to the stack
    public sealed class PushData : InstructionBase
    {
        public override InstructionType Type => InstructionType.PushData;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push the zero variable to the stack
    public sealed class ZeroVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_ZeroVar;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push the global variable to the stack
    public sealed class PushGlobalVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_PushGlobalVar;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //push the variable currently on top once more
    public sealed class PushDuplicate : InstructionBase
    {
        public override InstructionType Type => InstructionType.PushDuplicate;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //pop a value from the stack
    public sealed class Pop : InstructionBase
    {
        public override InstructionType Type => InstructionType.Pop;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
