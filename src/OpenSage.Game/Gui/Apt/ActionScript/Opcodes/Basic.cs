using System;
using System.Collections.Generic;

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

            for(int i=0;i<Parameters.Count;++i)
            {
                Parameters[i].Resolve(context);
            }

            pool.InsertRange(0, Parameters);
        }
    }

    /// <summary>
    /// Pop a string from stack and print it to console. Used for debug purposes.
    /// </summary>
    public sealed class Trace : InstructionBase
    {
        public override InstructionType Type => InstructionType.Trace;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pop a value from stack and store it inside a register
    /// </summary>
    public sealed class SetRegister : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetRegister;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }   
}
