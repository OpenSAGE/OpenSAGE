using System;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// End the execution of the current Action
    /// </summary>
    public sealed class End : InstructionBase
    {
        public override InstructionType Type => InstructionType.End;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Declare a pool of constants that will be used in the current scope. Mostly used at start.
    /// </summary>
    public sealed class ConstantPool : InstructionBase
    {
        public override InstructionType Type => InstructionType.ConstantPool;
        public override uint Size => 8;

        public override void Execute(ActionContext context)
        {
            //create a new constantpool
            var pool = context.Scope.Constants;
            pool.Clear();

            for (var i = 0; i < Parameters.Count; ++i)
            {
                pool.Add(Parameters[i].ResolveConstant(context));
            }
        }
    }

    /// <summary>
    /// Pop a string from stack and print it to console. Used for debug purposes.
    /// </summary>
    public sealed class Trace : InstructionBase
    {
        public override InstructionType Type => InstructionType.Trace;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void Execute(ActionContext context)
        {
            logger.Debug($"[TRACE] {context.Pop().ToString()}");
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
            context.Registers[reg] = val;
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
            Value[] args = new Value[nArgs];

            for (int i = 0; i < nArgs; ++i)
            {
                args[i] = context.Pop();
            }

            context.Push(Value.FromArray(args));
        }
    }

    /// <summary>
    /// Pops a property name and an object from the stack. Then deletes the property in that object
    /// </summary>
    public sealed class Delete : InstructionBase
    {
        public override InstructionType Type => InstructionType.Delete;

        public override void Execute(ActionContext context)
        {
            var value = context.Pop();
            var varName = context.Pop().ToString();
            context.Locals.Remove(varName);
        }
    }

    /// <summary>
    /// Pops a property name from the stack. Then deletes the property
    /// </summary>
    public sealed class Delete2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Delete2;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pops name & value. Then set the variable. If value is already defined, overwrite it
    /// </summary>
    public sealed class DefineLocal : InstructionBase
    {
        public override InstructionType Type => InstructionType.DefineLocal;

        public override void Execute(ActionContext context)
        {
            var value = context.Pop();
            var varName = context.Pop().ToString();

            context.Locals[varName] = value;
        }
    }

    /// <summary>
    /// Pops a value from the stack, converts it to integer and pushes it back
    /// </summary>
    public sealed class ToInteger : InstructionBase
    {
        public override InstructionType Type => InstructionType.ToInteger;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pops a value from the stack, converts it to integer and pushes it back
    /// </summary>
    public sealed class ToString : InstructionBase
    {
        public override InstructionType Type => InstructionType.ToString;

        public override void Execute(ActionContext context)
        {
            var val = context.Pop();
            context.Push(Value.FromString(val.ToString()));
        }
    }


    /// <summary>
    /// Pops an object from stack and enumerates it's slots
    /// </summary>
    public sealed class Enumerate2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.Enumerate2;

        public override void Execute(ActionContext context)
        {
            var obj = context.Pop().ToObject();
            context.Push(Value.FromObject(null));
            // Not sure if this is correct
            foreach (var slot in obj.Variables.Keys)
            {
                context.Push(Value.FromString(slot));
            }
        }
    }

    /// <summary>
    /// Pops an object from stack and enumerates it's slots
    /// </summary>
    public sealed class Var : InstructionBase
    {
        public override InstructionType Type => InstructionType.Var;

        public override void Execute(ActionContext context)
        {
            // TODO: fix this
            //throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pops an object from stack and enumerates it's slots
    /// </summary>
    public sealed class RandomNumber : InstructionBase
    {
        public override InstructionType Type => InstructionType.Random;

        public override void Execute(ActionContext context)
        {
            // TODO: fix this
            var max = context.Pop().ToInteger();

            var rnd = new Random();
            context.Push(Value.FromInteger(rnd.Next(0, max)));
        }
    }
}
