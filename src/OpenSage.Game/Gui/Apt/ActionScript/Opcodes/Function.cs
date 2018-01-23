using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Declare a new named or anonymous function (depending on function name) that will either be
    /// pushed to stack or set as a variable. 
    /// </summary>
    public sealed class DefineFunction : InstructionBase
    {
        public override InstructionType Type => InstructionType.DefineFunction;
        public override uint Size => 20;

        public override void Execute(ActionContext context)
        {
            var name = Parameters[0].String;
            var nParams = Parameters[1].Number;
            var size = Parameters[2 + nParams];

            //create a list of parameters
            var paramList = Parameters
                .Skip(2)
                .Take(nParams)
                .ToList();

            //get all the instructions
            var code = context.Stream.GetInstructions(size.Number);
                
            var func = new Function() { Parameters = paramList, Instructions = code };

            context.Scope.Functions[name] = func; 
        }
    }

    /// <summary>
    /// Return out of the current function back to the calling point
    /// </summary>
    public sealed class Return : InstructionBase
    {
        public override InstructionType Type => InstructionType.Return;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call an anonymous method that is on the stack. Function arguments are also popped from the stack
    /// </summary>
    public sealed class CallMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallMethodPop;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Function arguments are popped from the stack
    /// </summary>
    public sealed class CallNamedMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedMethodPop;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Has no arguments
    /// </summary>
    public sealed class CallNamedFunc : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedFunc;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Function arguments are popped from the stack
    /// </summary>
    public sealed class CallNamedFuncPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedFuncPop;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a function that is defined in the current scope
    /// </summary>
    public sealed class CallFunction : InstructionBase
    {
        public override InstructionType Type => InstructionType.CallFunction;
        public override uint Size => 0;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a function that is defined in the current scope
    /// </summary>
    public sealed class CallFunc : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallFunc;
        public override uint Size => 0;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
