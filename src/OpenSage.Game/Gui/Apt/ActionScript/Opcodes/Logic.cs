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
            var boolVal = val.ToBoolean();
            context.Stack.Push(Value.FromBoolean(!boolVal));
        }
    }

    //Pop two values A,B from stack and check if B is greater A (reverse stack order)
    public sealed class Greater : InstructionBase
    {
        public override InstructionType Type => InstructionType.Greater;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    //Pop two values A,B from stack and check if B is greater A (reverse stack order)
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
}
