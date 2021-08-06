using System;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pop two strings from the stack and concatenate them
    /// </summary>
    public sealed class StringConcat : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromString(b.ToString() + a.ToString());
        public override InstructionType Type => InstructionType.StringConcat;
    }

    /// <summary>
    /// Pop two strings from the stack and check if they are equal
    /// </summary>
    public sealed class StringEquals : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromBoolean(b.ToString() == a.ToString());
        public override InstructionType Type => InstructionType.StringEquals;
    }
}
