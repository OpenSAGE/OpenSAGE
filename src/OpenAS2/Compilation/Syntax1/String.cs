using System;
using OpenAS2.Base;

namespace OpenAS2.Compilation.Syntax
{
    /// <summary>
    /// Pop two strings from the stack and concatenate them
    /// </summary>
    public sealed class StringConcat : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromString(b.ToString() + a.ToString());
        public override InstructionType Type => InstructionType.StringConcat;
        public override int LowestPrecendence => 13;
        public override string ToString(string[] p)
        {
            return $"{p[1]} + {p[0]}";
        }
    }

    /// <summary>
    /// Pop two strings from the stack and check if they are equal
    /// </summary>
    public sealed class StringEquals : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromBoolean(b.ToString() == a.ToString());
        public override InstructionType Type => InstructionType.StringEquals;
        public override int LowestPrecendence => 18;
        public override string ToString(string[] p)
        {
            return $"{p[1]}.toString() == {p[0]}.toString()";
        }
    }
}
