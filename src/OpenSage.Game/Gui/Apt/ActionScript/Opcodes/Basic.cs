using System;
using OpenSage.Gui.Apt.ActionScript.Library;
using System.Linq;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// End the execution of the current Action
    /// </summary>
    public sealed class End : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.End;

        public override void Execute(ActionContext context)
        {
            context.Halt = true;
        }
    }

    /// <summary>
    /// Declare a pool of constants that will be used in the current scope. Mostly used at start.
    /// </summary>
    public sealed class ConstantPool : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.ConstantPool;
        public override uint Size => 8;

        public override void Execute(ActionContext context)
        {
            context.ReformConstantPool(Parameters);
        }

        public override string GetParameterDesc(ActionContext context)
        {
            if (Parameters.Count <= 5)
                return $"{Parameters.Count} Constants: {string.Join(", ", Parameters.Select((x)=>x.ToString()).ToArray())}";
            return $"{Parameters.Count} Constants: {Parameters[0]}, {Parameters[1]}, {Parameters[2]}, ..., {Parameters[Parameters.Count - 1]}";
        }
    }

    /// <summary>
    /// Pop a string from stack and print it to console. Used for debug purposes.
    /// </summary>
    public sealed class Trace : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.Trace;
        public override bool PopStack => true;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override Value ExecuteWithArgs2(Value poppedVal)
        {
            logger.Debug($"[TRACE] {poppedVal.ToString()}");
            return null;
        }
        public override string ToString(string[] p)
        {
            return $"trace({p[0]})";
        }

    }

    /// <summary>
    /// Get a value from stack and store it inside a register
    /// </summary>
    public sealed class SetRegister : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetRegister;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            //get the value from the stack
            var val = context.Peek();

            //store the value inside the specified register
            var reg = Parameters[0].ToInteger();
            context.SetRegister(reg, val);
        }
        public override string ToString(string[] p)
        {
            return $"reg[{Parameters[0]}] = {p[0]}";
        }
    }

    /// <summary>
    /// Initializes an array from the stack
    /// </summary>
    public sealed class InitArray : InstructionBase
    {
        public override InstructionType Type => InstructionType.InitArray;

        public override void Execute(ActionContext context)
        {
            var nArgs = context.Pop().ToInteger();
            var args = context.Pop((uint) nArgs);

            context.Push(Value.FromArray(args, context.Apt.Avm));
        }

        public override string ToString(string[] p)
        {
            return $"[{p[0]}]";
        }
    }

    /// <summary>
    /// Pops a property name and an object from the stack. Then deletes the property in that object
    /// The description file says "property", but one can't access a property from a string,
    /// so I take the "property" as "member".
    /// </summary>
    public sealed class Delete : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.Delete;
        public override bool PopStack => true;

        public override void Execute(ActionContext context)
        {
            var property = context.Pop().ToString();
            var target = context.GetTarget(context.Pop().ToString());
            target.ToObject().DeleteMember(property);
        }
    }

    /// <summary>
    /// Pops a property name from the stack. Then deletes the property
    /// </summary>
    public sealed class Delete2 : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.Delete2;
        public override bool PopStack => true;

        public override void Execute(ActionContext context)
        {
            var property = context.Pop().ToString();
            context.DeleteValueOnChain(property);
        }
    }

    /// <summary>
    /// Pops name & value. Then set the variable. If value is already defined, overwrite it
    /// </summary>
    public sealed class DefineLocal : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.DefineLocal;
        public override uint StackPop => 2;

        public override void Execute(ActionContext context)
        {
            var value = context.Pop();
            var varName = context.Pop().ToString();
            context.SetValueOnLocal(varName, value);
        }
    }

    public sealed class DefineLocal2 : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.Var;
        public override bool PopStack => true;

        public override void Execute(ActionContext context)
        {
            var varName = context.Pop().ToString();
            if (context.HasValueOnLocal(varName))
                return;
            else
                context.SetValueOnLocal(varName, Value.Undefined()); 
        }
    }

    /// <summary>
    /// Pops a value from the stack, converts it to integer and pushes it back
    /// </summary>
    public sealed class ToInteger : InstructionMonoOperator
    {
        public override Func<Value, Value> Operator =>
            (a) => Value.FromInteger(a.ToInteger());
        public override InstructionType Type => InstructionType.ToInteger;
    }

    /// <summary>
    /// Pops a value from the stack, converts it to integer and pushes it back
    /// </summary>
    public sealed class ToString : InstructionMonoOperator
    {
        public override Func<Value, Value> Operator =>
            (a) => Value.FromString(a.ToString());
        public override InstructionType Type => InstructionType.ToString;
    }


    /// <summary>
    /// Pops an object from stack and enumerates its slots
    /// </summary>
    public sealed class Enumerate2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Enumerate2;
        public override bool IsStatement => false;

        public override void Execute(ActionContext context)
        {
            var obj = context.Pop().ToObject();
            context.Push(Value.FromObject(null));
            // Not sure if this is correct
            foreach (var slot in obj.GetAllProperties())
            {
                context.Push(Value.FromString(slot));
            }
        }
    }

    /// <summary>
    /// Pops an object from stack and enumerates it's slots
    /// </summary>
    public sealed class RandomNumber : InstructionMonoPushPop
    {
        public override bool PushStack => true;
        public override bool PopStack => true;
        public override void Execute(ActionContext context)
        {
            context.Push(Operator(context.Pop()));
        }
        private Func<Value, Value> Operator =>
            (max) => Value.FromInteger(new Random().Next(0, max.ToInteger()));
        public override InstructionType Type => InstructionType.Random;
        public override string ToString(string[] p)
        {
            return $"random({p[0]})";
        }
    }

    /// <summary>
    /// Unknown yet
    /// </summary>
    public sealed class ImplementsOp: InstructionBase
    {
        public override InstructionType Type => InstructionType.ImplementsOp;

        public override void Execute(ActionContext context)
        {
            // throw new NotImplementedException(context.DumpStack());
            var cst = context.Pop().ToFunction();
            Value[] args = FunctionCommon.GetArgumentsFromStack(context);
        }
    }

    /// <summary>
    /// Unknown yet
    /// </summary>
    public sealed class CastOp : InstructionDiOperator
    {
        public override Func<Value, Value, Value> Operator =>
            (objv, cstv) =>
            {
                var obj = objv.ToObject();
                var cst = cstv.ToFunction();
                ObjectContext val = obj.InstanceOf(cst) ? obj : null;
                return Value.FromObject(val);
            };
        public override InstructionType Type => InstructionType.CastOp;
    }

    /// <summary>
    /// Shall be the same as getTime in ActionSctipt.
    /// Need to be certained: the description file says getting the millseconds since Flash Player started.
    /// So this action will get the millseconds since the program started.
    /// The return value shall be put in stack.
    /// </summary>
    public sealed class GetTime: InstructionMonoPushPop
    {
        public override bool PushStack => true;
        public override void Execute(ActionContext context)
        {
            context.Push(Builtin.GetTimer());
        }
        public override InstructionType Type => InstructionType.GetTime;
        public override string ToString(string[] p)
        {
            return "getTimer()";
        }
    }
}
