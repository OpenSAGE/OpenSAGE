using System;

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

            var valueVal = objectVal.ResolveRegister(context).ToObject().GetMember(member);
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

            //make sure that potential register values are resolved:
            var obj = objectVal.ResolveRegister(context).ToObject();

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
    /// Pops variable name and pushes the corresponding variable back to stack
    /// </summary>
    public sealed class GetVariable : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetVariable;

        public override void Execute(ActionContext context)
        {
            //pop the value
            var variableName = context.Stack.Pop();
            var variable = context.Scope.Variables[variableName.ToString()];
            context.Stack.Push(variable);
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

    /// <summary>
    /// get a property and push it to the stack
    /// </summary>
    public sealed class GetProperty : InstructionBase
    {
        public override InstructionType Type => InstructionType.GetProperty;

        public override void Execute(ActionContext context)
        {
            var property = context.Stack.Pop();
            var target = context.GetTarget(context.Stack.Pop().ToString());

            var prop = target.ToObject().GetProperty(property.ToInteger());
            context.Stack.Push(prop);
        }
    }

    /// <summary>
    /// Pops an object from the stack and retrieves a member variable which is pushed to stack
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

            var valueVal = objectVal.ToObject().GetMember(memberVal.ToString());

            context.Stack.Push(valueVal);
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

    /// <summary>
    /// Create a new object by calling it's constructor
    /// </summary>
    public sealed class NewObject : InstructionBase
    {
        public override InstructionType Type => InstructionType.NewObject;

        public override void Execute(ActionContext context)
        {
            var name = context.Stack.Pop().ToString();
            var nArgs = context.Stack.Pop().ToInteger();

            Value[] args = new Value[nArgs];

            for (int i = 0; i < nArgs; ++i)
            {
                args[i] = context.Stack.Pop();
            }

            var obj = context.ConstructObject(name,args);
            context.Stack.Push(obj);
        }
    }

    /// <summary>
    /// Initializes an object from the stack
    /// </summary>
    public sealed class InitObject : InstructionBase
    {
        public override InstructionType Type => InstructionType.InitObject;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
