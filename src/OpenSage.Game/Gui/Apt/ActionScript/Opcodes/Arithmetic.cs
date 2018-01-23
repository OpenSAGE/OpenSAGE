using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// An instruction that pops two values and adds them. Result on stack
    /// </summary>
    public sealed class Add : InstructionBase
    {
        public override InstructionType Type => InstructionType.Add;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// An instruction that pops two values and subtracts them. Result on stack
    /// </summary>
    public sealed class Subtract : InstructionBase
    {
        public override InstructionType Type => InstructionType.Subtract;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop two values from stack and add them. Can concatenate strings. Result on stack
    /// </summary>
    public sealed class Add2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Add2;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then multiply them. Result on stack
    /// </summary>
    public sealed class Multiply : InstructionBase
    {
        public override InstructionType Type => InstructionType.Multiply;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then divide them. Result on stack
    /// </summary>
    public sealed class Divide : InstructionBase
    {
        public override InstructionType Type => InstructionType.Divide;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop two values from stack and check them for equality. Does work with strings. Result on stack
    /// </summary>
    public sealed class Equals2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Equals2;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop two values from stack and check them for equality. Does work with strings. Result on stack
    /// </summary>
    public sealed class LessThan2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.LessThan2;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
