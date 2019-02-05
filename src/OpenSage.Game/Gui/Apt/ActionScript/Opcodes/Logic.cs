using System;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pop a value from stack, convert it to boolean. Push the inverted value back to stack
    /// </summary>
    public sealed class Not : InstructionBase
    {
        public override InstructionType Type => InstructionType.Not;

        public override void Execute(ActionContext context)
        {
            var val = context.Stack.Pop();
            var boolVal = val.ResolveRegister(context).ToBoolean();
            context.Stack.Push(Value.FromBoolean(!boolVal));
        }
    }

    /// <summary>
    /// Pop two values from stack and check them for equality. Does work with types. Result on stack
    /// </summary>
    public sealed class Equals2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Equals2;

        public override void Execute(ActionContext context)
        {
            var a = context.Stack.Pop();
            var b = context.Stack.Pop();
            bool eq = a.Equals(b);
            context.Stack.Push(Value.FromBoolean(eq));

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

    //Pop two values A,B from stack and check if B is greater A (reverse stack order)
    public sealed class Greater : InstructionBase
    {
        public override InstructionType Type => InstructionType.Greater;

        public override void Execute(ActionContext context)
        {
            var a = context.Stack.Pop().ToInteger();
            var b = context.Stack.Pop().ToInteger();

            context.Stack.Push(Value.FromBoolean(b > a));
        }
    }

    //Pop two values A,B from stack and calculate their bitwise xor. Result on stack
    public sealed class BitwiseXOr : InstructionBase
    {
        public override InstructionType Type => InstructionType.BitwiseXOr;

        public override void Execute(ActionContext context)
        {
            var a = context.Stack.Pop().ToInteger();
            var b = context.Stack.Pop().ToInteger();
            var result = Value.FromInteger(a ^ b);

            context.Stack.Push(result);
        }
    }

    /// <summary>
    /// Pop two values from stack and compare the two values using the Strict Equality Comparison Algorithm.
    /// Push the resulting Boolean value onto the stack.
    /// </summary>
    public sealed class StrictEquals : InstructionBase
    {
        public override InstructionType Type => InstructionType.Equals2;

        public override void Execute(ActionContext context)
        {
            var a = context.Stack.Pop();
            var b = context.Stack.Pop();

            context.Stack.Push(Value.FromBoolean(a.StrictEquals(b)));
        }
    }
}
