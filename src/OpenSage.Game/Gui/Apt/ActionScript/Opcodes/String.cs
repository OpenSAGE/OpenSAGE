using System;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pop two strings from the stack and concatenate them
    /// </summary>
    public sealed class StringConcat : InstructionMonoPush
    {
        public override bool PushStack => true;
        public override uint StackPop => 2;
        public override InstructionType Type => InstructionType.StringConcat;

        public override void Execute(ActionContext context)
        {
            var a = context.Pop();
            var b = context.Pop();

            if (a.Type != ValueType.String || b.Type != ValueType.String)
                throw new InvalidOperationException();

            context.Push(Value.FromString(b.ToString() + a.ToString()));
        }
    }

    /// <summary>
    /// Pop two strings from the stack and check if they are equal
    /// </summary>
    public sealed class StringEquals : InstructionMonoPush
    {
        public override bool PushStack => true;
        public override uint StackPop => 2;
        public override InstructionType Type => InstructionType.StringEquals;

        public override void Execute(ActionContext context)
        {
            var a = context.Pop();
            var b = context.Pop();

            context.Push(Value.FromBoolean(b.ToString() == a.ToString()));
        }
    }
}
