using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //simple add instruction
    public sealed class Add : InstructionBase
    {
        public override InstructionType Type => InstructionType.Add;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //simple subtract instruction
    public sealed class Subtract : InstructionBase
    {
        public override InstructionType Type => InstructionType.Subtract;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //this add can also concatenate strings. Pop values from stack
    public sealed class Add2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Add2;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //multiply two numbers that are popped from the stack (as float) and push the result
    public sealed class Multiply : InstructionBase
    {
        public override InstructionType Type => InstructionType.Multiply;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //divide two numbers that are popped from the stack (as float) and push the result
    public sealed class Divide : InstructionBase
    {
        public override InstructionType Type => InstructionType.Divide;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //this equal can also compare strings
    public sealed class Equals2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Equals2;

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
