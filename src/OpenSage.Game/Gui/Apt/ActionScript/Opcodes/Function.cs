using System;
using System.Collections.Generic;

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

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Return out of the current function back to the calling point
    /// </summary>
    public sealed class Return : InstructionBase
    {
        public override InstructionType Type => InstructionType.Return;

        public override void Execute()
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

        public override void Execute()
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

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
