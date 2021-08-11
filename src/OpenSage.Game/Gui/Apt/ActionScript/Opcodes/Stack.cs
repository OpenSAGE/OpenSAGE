using System;
using System.Linq;
using System.Collections.Generic;
using OpenSage.FileFormats.Apt.ActionScript;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{

    /// <summary>
    /// Pop a value from the stack
    /// </summary>
    public sealed class Pop : InstructionMonoPushPop
    {
        public override bool PopStack => true;
        public override InstructionType Type => InstructionType.Pop;

        public override void Execute(ActionContext context)
        {
            context.Pop();
        }
    }

    /// <summary>
    /// Pop a value from the stack and convert it to number push it back
    /// </summary>
    public sealed class ToNumber : InstructionMonoOperator
    {
        public override InstructionType Type => InstructionType.ToNumber;

        public override Func<Value, Value> Operator =>
            (a) =>
            {
                var strVal = a.ToString();
                return Value.FromFloat(float.Parse(strVal));
            };
    }

    /// <summary>
    /// Push the current object to the stack
    /// </summary>
    public sealed class PushThis : PushThisVar
    {
        public override InstructionType Type => InstructionType.EA_PushThis;
    }

    /// <summary>
    /// Push the current object to the stack
    /// </summary>
    public class PushThisVar : InstructionMonoPushPop
    {
        public override bool PushStack => true;
        public override InstructionType Type => InstructionType.EA_PushThisVar;

        public override void Execute(ActionContext context)
        {
            context.Push(Value.FromObject(context.This));
        }
        public override string ToString(string[] p)
        {
            return "this";
        }
    }


    /// <summary>
    /// Read the variable name from the pool and push that variable's value to the stack
    /// </summary>
    public sealed class PushValueOfVar : InstructionMonoPushPop
    {
        public override bool PushStack => true;
        public override InstructionType Type => InstructionType.EA_PushValueOfVar;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            var str = context.Constants[id].ToString();

            Value result;

            if (context.CheckParameter(str))
            {
                result = context.GetParameter(str);
            }
            else
            {
                result = context.GetValueOnChain(str);
            }
            context.Push(result);
        }
        public override string ToString(string[] p)
        {
            return $"{p[0]}";
        }
    }
    /// <summary>
    /// Get multiple variables from the pool and push them to the stack
    /// </summary>
    public sealed class PushData : InstructionBase
    {
        public override InstructionType Type => InstructionType.PushData;
        public override uint Size => 8;

        public override void Execute(ActionContext context)
        {
            // The first parameter is constant count, omit it
            foreach (var constant in Parameters.Skip(1))
            {
                context.Push(constant.ResolveConstant(context));
            }
        }
    }

    /// <summary>
    /// Push a zero variable to the stack
    /// </summary>
    public sealed class ZeroVar : InstructionMonoPushPop
    {
        public override bool PopStack => true;
        public override InstructionType Type => InstructionType.EA_ZeroVar;

        public override void Execute(ActionContext context)
        {
            // TODO: check if this is correct
            var name = context.Pop();
            context.This.SetMember(name.ToString(), Value.FromInteger(0));
        }
        public override int Precendence => 3;
        public override string ToString(string[] p)
        {
            return $"{p[0]} = 0";
        }
    }

    /// <summary>
    /// Push the global object to the stack
    /// </summary>
    public sealed class PushGlobalVar : InstructionMonoPushPop
    {
        public override bool PushStack => true;
        public override InstructionType Type => InstructionType.EA_PushGlobalVar;

        public override void Execute(ActionContext context)
        {
            context.Push(Value.FromObject(context.Apt.Avm.GlobalObject));
        }
        public override string ToString(string[] p)
        {
            return "_global";
        }
    }

    /// <summary>
    /// Pop a value from the stack and push it twice
    /// </summary>
    public sealed class PushDuplicate : InstructionBase
    {
        public override InstructionType Type => InstructionType.PushDuplicate;
        public override bool IsStatement => false;

        public override void Execute(ActionContext context)
        {
            var val = context.Peek();
            context.Push(val);
        }
    }

    /// <summary>
    /// Push a string to the stack
    /// </summary>
    public sealed class PushString : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushString;
        public override uint Size => 4;
    }

    /// <summary>
    /// Push a float to the stack
    /// </summary>
    public sealed class PushFloat : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushFloat;
        public override uint Size => 4;
    }

    /// <summary>
    /// Read a constant from the pool and push it to stack
    /// </summary>
    public sealed class PushConstantByte : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushConstantByte;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            context.Push(context.Constants[id]);
        }
        public override string ToString(string[] p)
        {
            return $"{p[0]}";
        }
    }

    /// <summary>
    /// Read a byte and push it to the stack
    /// </summary>
    public sealed class PushByte : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushByte;
        public override uint Size => 1;
    }

    /// <summary>
    /// Read a short and push it to the stack
    /// </summary>
    public sealed class PushShort : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushShort;
        public override uint Size => 2;
    }

    /// <summary>
    /// Read an int32 and push it to the stack (although claimed to be long)
    /// </summary>
    public sealed class PushLong : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushLong;
        public override uint Size => 4;
    }


    /// <summary>
    /// Push an undefined value to the stack
    /// </summary>
    public sealed class PushUndefined : InstructionPushValue
    {
        public override Value ValueToPush => Value.Undefined();
        public override InstructionType Type => InstructionType.EA_PushUndefined;
    }

    /// <summary>
    /// Push a boolean with value false to the stack
    /// </summary>
    public sealed class PushFalse : InstructionPushValue
    {
        public override Value ValueToPush => Value.FromBoolean(false);
        public override InstructionType Type => InstructionType.EA_PushFalse;
    }

    /// <summary>
    /// Push a null value false to the stack
    /// </summary>
    public sealed class PushNull : InstructionPushValue
    {
        public override Value ValueToPush => Value.FromObject(null);
        public override InstructionType Type => InstructionType.EA_PushNull;
    }

    /// <summary>
    /// Push an integer with value zero to the stack
    /// </summary>
    public sealed class PushZero : InstructionPushValue
    {
        public override Value ValueToPush => Value.FromInteger(0);
        public override InstructionType Type => InstructionType.EA_PushZero;
    }

    /// <summary>
    /// Push an integer with value one to the stack
    /// </summary>
    public sealed class PushOne : InstructionPushValue
    {
        public override Value ValueToPush => Value.FromInteger(1);
        public override InstructionType Type => InstructionType.EA_PushOne;
    }

    /// <summary>
    /// Push a boolean with value true to the stack
    /// </summary>
    public sealed class PushTrue : InstructionPushValue
    {
        public override Value ValueToPush => Value.FromBoolean(true);
        public override InstructionType Type => InstructionType.EA_PushTrue;
    }

    /// <summary>
    /// Push a register's value to the stack
    /// </summary>
    public sealed class PushRegister : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushRegister;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            context.Push(Parameters[0].ResolveRegister(context));
        }
        public override string ToString(string[] p)
        {
            return $"{p[0]}";
        }
    }

    /// <summary>
    /// (Just guessing) Similiar to PushConstantByte,
    /// read a constant from the pool and push it to stack,
    /// but it reads a Int16 instead of byte as constant index
    /// </summary>
    public sealed class PushConstantWord : InstructionPushValue
    {
        public override InstructionType Type => InstructionType.EA_PushConstantWord;
        public override uint Size => 2;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            context.Push(context.Constants[id]);
        }
        public override string ToString(string[] p)
        {
            return $"{p[0]}";
        }
    }

}
