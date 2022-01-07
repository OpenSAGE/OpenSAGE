using System;
using OpenAS2.Base;

namespace OpenAS2.Runtime.Opcodes
{


    /// <summary>
    /// Pop two values from stack and check them for equality. Does work with types. Result on stack
    /// </summary>
    public sealed class Equals2 : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromBoolean(Value.AbstractEquals(a, b));
        public override InstructionType Type => InstructionType.Equals2;
        public override int Precendence => 10;
        public override string ToString(string[] p)
        {
            return $"{p[1]} == {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack and compare the two values using the Strict Equality Comparison Algorithm.
    /// Push the resulting Boolean value onto the stack.
    /// </summary>
    public sealed class StrictEquals : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromBoolean(Value.StrictEquals(a, b));
        public override InstructionType Type => InstructionType.StrictEquals;
        public override int Precendence => 10;
        public override string ToString(string[] p)
        {
            return $"{p[1]} === {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack and check them for equality. Does work with strings. Result on stack
    /// </summary>
    public sealed class LessThan : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
                {
                    var arg1 = a.ToFloat();
                    var arg2 = b.ToFloat();

                    if (double.IsNaN(arg1)) arg1 = 0;
                    if (double.IsNaN(arg2)) arg2 = 0;

                    bool result = arg2 < arg1;
                    return Value.FromBoolean(result);

                };
        public override InstructionType Type => InstructionType.LessThan;
        public override int Precendence => 11;
        public override string ToString(string[] p)
        {
            return $"{p[1]} < {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack and check them for equality. Does work with strings. Result on stack
    /// </summary>
    public sealed class LessThan2 : InstructionDiOperator
    {
        //Should work according to ECMA-262 Section 11.8.5 https://262.ecma-international.org/5.1/#sec-11.8.5
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            {
                var arg1 = a.ToFloat();
                var arg2 = b.ToFloat();

                if (double.IsNaN(arg1) || double.IsNaN(arg2))
                    return Value.Undefined();
                else
                {
                    bool result = arg2 < arg1;
                    return Value.FromBoolean(result);
                }
            };
        public override InstructionType Type => InstructionType.LessThan2;
        public override int Precendence => 11;
        public override string ToString(string[] p)
        {
            return $"{p[1]} < {p[0]}";
        }
    }

    //Pop two values A,B from stack and check if B is greater A (reverse stack order)
    public sealed class Greater : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromBoolean(b.ToFloat() > a.ToFloat());
        public override InstructionType Type => InstructionType.Greater;
        public override int Precendence => 11;
        public override string ToString(string[] p)
        {
            return $"{p[1]} > {p[0]}";
        }
    }

    //Pop two values A,B from stack and calculate their bitwise and. Result on stack
    public sealed class BitwiseAnd : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromInteger(a.ToInteger() & b.ToInteger());
        public override InstructionType Type => InstructionType.BitwiseAnd;
        public override int Precendence => 9;
        public override string ToString(string[] p)
        {
            return $"{p[1]} & {p[0]}";
        }
    }

    //Pop two values A,B from stack and calculate their bitwise or. Result on stack
    public sealed class BitwiseOr : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromInteger(a.ToInteger() | b.ToInteger());
        public override InstructionType Type => InstructionType.BitwiseOr;
        public override int Precendence => 7;
        public override string ToString(string[] p)
        {
            return $"{p[1]} | {p[0]}";
        }
    }

    //Pop two values A,B from stack and calculate their bitwise xor. Result on stack
    public sealed class BitwiseXOr : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (a, b) => Value.FromInteger(a.ToInteger() ^ b.ToInteger());
        public override InstructionType Type => InstructionType.BitwiseXOr;
        public override int Precendence => 8;
        public override string ToString(string[] p)
        {
            return $"{p[1]} ^ {p[0]}";
        }
    }

    /// <summary>
    /// Pop a value from stack, convert it to boolean. Push the inverted value back to stack
    /// </summary>
    public sealed class LogicalNot : InstructionMonoOperator
    {
        public override Func<Value, Value> Operator => (a) => Value.FromBoolean(!a.ToBoolean());
        public override InstructionType Type => InstructionType.LogicalNot;
        public override int Precendence => 15;
        public override string ToString(string[] p)
        {
            return $"!{p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack and check their logical and value in float.
    /// </summary>
    public sealed class LogicalAnd : InstructionDiOperator
    {
        //Should work according to ECMA-262 Section 11.8.5
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            {
                var arg1 = a.ToFloat();
                var arg2 = b.ToFloat();
                return Value.FromBoolean((!double.IsNaN(arg1) && arg1 != 0) && (!double.IsNaN(arg2) && arg2 != 0));
            };
        public override InstructionType Type => InstructionType.LogicalAnd;
        public override int Precendence => 6;
        public override string ToString(string[] p)
        {
            return $"{p[1]} && {p[0]}";
        }
    }

    /// <summary>
    /// Pop two values from stack and check their logical and value in float.
    /// </summary>
    public sealed class LogicalOr : InstructionDiOperator
    {
        //Should work according to ECMA-262 Section 11.8.5
        public override Func<Value, Value, Value> Operator =>
            (a, b) =>
            {
                var arg1 = a.ToFloat();
                var arg2 = b.ToFloat();
                return Value.FromBoolean((!double.IsNaN(arg1) && arg1 != 0) || (!double.IsNaN(arg2) && arg2 != 0));
            };
        public override InstructionType Type => InstructionType.LogicalOr;
        public override int Precendence => 5;
        public override string ToString(string[] p)
        {
            return $"{p[1]} || {p[0]}";
        }
    }
}
