using System;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// An instruction that pops two values and adds them. Result on stack
    /// </summary>
    public sealed class Add : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.Add;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            return Value.FromFloat(
                poppedVals[1].ToFloat() + poppedVals[0].ToFloat()
                );
        }
    }

    /// <summary>
    /// An instruction that pops two values and subtracts them. Result on stack
    /// </summary>
    public sealed class Subtract : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.Subtract;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            return Value.FromFloat(
                poppedVals[1].ToFloat() - poppedVals[0].ToFloat()
                );
        }
    }

    /// <summary>
    /// Pop two values from stack and add them. Can concatenate strings. Result on stack
    /// The additive operator follows https://262.ecma-international.org/5.1/#sec-11.6.1
    /// </summary>
    public sealed class Add2 : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.Add2;
        public override bool PushStack => true;
        public override uint StackPop => 2;
        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            //Pop two values
            var a = poppedVals[0];
            var b = poppedVals[1];

            Value ans = null;
            if (a.IsNumericType() && b.IsNumericType())
                ans = Value.FromFloat(b.ToFloat() + a.ToFloat());
            else
                ans = Value.FromString(b.ToString() + a.ToString());
            return ans;
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then multiply them. Result on stack
    /// </summary>
    public sealed class Multiply : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.Multiply;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            return Value.FromFloat(
                poppedVals[1].ToFloat() * poppedVals[0].ToFloat()
                );
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then divide them. Result on stack
    /// </summary>
    public sealed class Divide : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.Divide;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            var af = poppedVals[0].ToFloat();
            var bf = poppedVals[1].ToFloat();

            var val_to_push = Value.FromFloat(float.NaN);

            if (af != 0) { val_to_push = Value.FromFloat(bf / af); }

            return val_to_push;
        }
    }

    /// <summary>
    /// Pop two values from stack, convert them to float and then divide them. Result on stack
    /// </summary>
    public sealed class Modulo : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.Modulo;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            var af = poppedVals[0].ToFloat();
            var bf = poppedVals[1].ToFloat();

            var  val_to_push = Value.FromFloat(af % bf);

            return val_to_push;
        }
    }

    /// <summary>
    /// Pop a value from stack, increments it and pushes it back
    /// </summary>
    public sealed class Increment : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.Increment;
        public override bool PushStack => true;
        public override bool PopStack => true;

        public override Value ExecuteWithArgs2(Value poppedVal)
        {
            var num = poppedVal.ToInteger();
            return Value.FromInteger(++num);
        }
    }

    /// <summary>
    /// Pop a value from stack, increments it and pushes it back
    /// </summary>
    public sealed class Decrement : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.Decrement;
        public override bool PushStack => true;
        public override bool PopStack => true;

        public override Value ExecuteWithArgs2(Value poppedVal)
        {
            var num = poppedVal.ToInteger();
            return Value.FromInteger(--num);
        }
    }

    public sealed class ShiftLeft : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.ShiftLeft;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVal)
        {
            var count = poppedVal[0].ToInteger() & 0b11111;
            var val = poppedVal[1].ToInteger();
            return Value.FromInteger(val << count);
        }
    }

    public sealed class ShiftRight : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.ShiftRight;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVal)
        {
            var count = poppedVal[0].ToInteger() & 0b11111;
            var val = poppedVal[1].ToInteger();
            return Value.FromInteger(val >> count);
        }
    }

    // shift right as uint
    public sealed class ShiftRight2 : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.ShiftRight2;
        public override bool PushStack => true;
        public override uint StackPop => 2;

        public override Value ExecuteWithArgs2(Value[] poppedVal)
        {
            var count = poppedVal[0].ToInteger() & 0b11111;
            var val = (uint) poppedVal[1].ToInteger();
            return Value.FromInteger((int) (val >> count));
        }
    }
}
