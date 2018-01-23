using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// Pop an object from stack and retrieve the member name from the pool. Push the member of
    /// the object to stack.
    /// </summary>
    public sealed class GetNamedMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_GetNamedMember;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            var member = context.Scope.Constants[id].ToString();

            //pop the object
            var objectVal = context.Stack.Pop();

            var valueVal =  objectVal.ToObject().GetMember(member);
            context.Stack.Push(valueVal);
        }
    }

    /// <summary>
    /// set the member of a specific object (everything on stack)
    /// </summary>
    public sealed class SetMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetMember;

        public override void Execute(ActionContext context)
        {
            //pop the value
            var valueVal = context.Stack.Pop();
            //pop the member name
            var memberVal = context.Stack.Pop();
            //pop the object
            var objectVal = context.Stack.Pop();

            var obj = objectVal.ToObject();
            obj.Variables[memberVal.ToString()] = valueVal;
        }
    }

    /// <summary>
    /// Get a variable from the current object and push it to the stack
    /// </summary>
    public sealed class GetStringVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_GetStringVar;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            var str = Parameters[0].ToString();

            //check if this a special object, like _root, _parent etc.
            Value result = context.GetObject(str);

            if (result == null)
                throw new InvalidOperationException();

            context.Stack.Push(result);
        }
    }

    /// <summary>
    /// Pops variable name and value from the stack. Then set the variable to that value.
    /// </summary>
    public sealed class SetVariable : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetVariable;

        public override void Execute(ActionContext context)
        {
            //pop the value
            var valueVal = context.Stack.Pop();
            //pop the member name
            var memberVal = context.Stack.Pop();

            context.Scope.Variables[memberVal.ToString()] = valueVal;
        }
    }

    /// <summary>
    /// get the member of a specific object. Result will be pushed to stack
    /// </summary>
    public sealed class GetMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetMember;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class GetProperty : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetProperty;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// just pushes a string to stack it seems like
    /// </summary>
    public sealed class GetStringMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_GetStringMember;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            //pop the member name
            var memberVal = Parameters[0];

            //pop the object
            var objectVal = context.Stack.Pop();

            var valueVal = objectVal.ToObject().Properties[memberVal.ToString()];

            context.Stack.Push(memberVal);
        }
    }

    public sealed class SetStringMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_SetStringMember;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
