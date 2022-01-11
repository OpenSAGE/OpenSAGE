using System;
using OpenAS2.Base;

namespace OpenAS2.Compilation.Syntax
{
    /// <summary>
    /// An instruction that pops two values and adds them. Result on stack
    /// </summary>
    public sealed class Add : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromFloat(b.ToFloat() + a.ToFloat());
        public override InstructionType Type => InstructionType.Add;
        public override int LowestPrecendence => 13;
        public override string ToString(string[] p)
        {
            return $"{p[1]} + {p[0]}";
        }
    }

    /// <summary>
    /// An instruction that pops two values and subtracts them. Result on stack
    /// </summary>
    public sealed class Subtract : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromFloat(b.ToFloat() - a.ToFloat());
        public override InstructionType Type => InstructionType.Subtract;
        public override int LowestPrecendence => 13;
        public override string ToString(string[] p)
        {
            return $"{p[1]} - {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack and add them. Can concatenate strings. Result on stack
    /// The additive operator follows https://262.ecma-international.org/5.1/#sec-11.6.1
    /// </summary>
    public sealed class Add2 : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            (a.IsNumber() && b.IsNumber()) ?
            Value.FromFloat(b.ToFloat() + a.ToFloat()) :
            Value.FromString(b.ToString() + a.ToString());
        public override InstructionType Type => InstructionType.Add2;
        public override int LowestPrecendence => 13;
        public override string ToString(string[] p)
        {
            return $"{p[1]} + {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then multiply them. Result on stack
    /// </summary>
    public sealed class Multiply : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromFloat(b.ToFloat() * a.ToFloat());
        public override InstructionType Type => InstructionType.Multiply;
        public override int LowestPrecendence => 14;
        public override string ToString(string[] p)
        {
            return $"{p[1]} * {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then divide them. Result on stack
    /// </summary>
    public sealed class Divide : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            {
                var af = a.ToFloat();
                var bf = b.ToFloat();

                var val_to_push = Value.FromFloat(float.NaN);

                if (af != 0) { val_to_push = Value.FromFloat(bf / af); }

                return val_to_push;
            };
        public override InstructionType Type => InstructionType.Divide;
        public override int LowestPrecendence => 14;
        public override string ToString(string[] p)
        {
            return $"{p[1]} / {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then divide them. Result on stack
    /// </summary>
    public sealed class Modulo : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromFloat(a.ToFloat() % b.ToFloat());
        public override InstructionType Type => InstructionType.Modulo;
        public override int LowestPrecendence => 14;
        public override string ToString(string[] p)
        {
            return $"{p[1]} % {p[0]}";
        }
    }

    /// <summary>
    /// Pop a value from stack, increments it and pushes it back
    /// </summary>
    public sealed class Increment : InstructionCalculableUnary
    {
        public override Func<Value, Value> Operator =>
            (a) => Value.FromInteger(a.ToInteger() + 1);
        public override InstructionType Type => InstructionType.Increment;
        public override int LowestPrecendence => 15;
        public override string ToString(string[] p)
        {
            return $"{p[0]} + 1";
        }
    }

    /// <summary>
    /// Pop a value from stack, increments it and pushes it back
    /// </summary>
    public sealed class Decrement : InstructionCalculableUnary
    {
        public override Func<Value, Value> Operator =>
            (a) => Value.FromInteger(a.ToInteger() - 1);
        public override InstructionType Type => InstructionType.Decrement;
        public override int LowestPrecendence => 15;
        public override string ToString(string[] p)
        {
            return $"{p[0]} - 1";
        }
    }

    public sealed class ShiftLeft : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            {
                var count = a.ToInteger() & 0b11111;
                var val = b.ToInteger();
                return Value.FromInteger(val << count);
            };
        public override InstructionType Type => InstructionType.ShiftLeft;
        public override int LowestPrecendence => 12;
        public override string ToString(string[] p)
        {
            return $"{p[1]} << {p[0]}";
        }
    }

    public sealed class ShiftRight : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            {
                var count = a.ToInteger() & 0b11111;
                var val = b.ToInteger();
                return Value.FromInteger(val >> count);
            };
        public override InstructionType Type => InstructionType.ShiftRight;
        public override int LowestPrecendence => 12;
        public override string ToString(string[] p)
        {
            return $"{p[1]} >> {p[0]}";
        }
    }

    // shift right as uint
    public sealed class ShiftRight2 : InstructionCalculableBinary
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            {
                var count = a.ToInteger() & 0b11111;
                var val = (uint) b.ToInteger();
                return Value.FromInteger((int) (val >> count));
            };
        public override InstructionType Type => InstructionType.ShiftRight2;
        public override int LowestPrecendence => 12;
        public override string ToString(string[] p)
        {
            return $"{p[1]} >>> {p[0]}";
        }
    }
}
