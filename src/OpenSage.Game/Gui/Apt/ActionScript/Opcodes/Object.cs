using System;
using OpenSage.Data.Apt.Characters;

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
            var objectVal = context.Stack.Pop().ResolveRegister(context);
            var obj = objectVal.ToObject();

            if (obj != null)
            {
                context.Stack.Push(obj.GetMember(member));
            }
            else
            {
                context.Stack.Push(Value.Undefined());
            }
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
            var valueVal = context.Stack.Pop().ResolveRegister(context);
            //pop the member name
            var memberName = context.Stack.Pop().ResolveRegister(context).ToString();
            //pop the object
            var p = context.Stack.Pop();
            var obj = p.ResolveRegister(context).ToObject();

            if (obj.IsBuiltInVariable(memberName))
            {
                obj.SetBuiltInVariable(memberName, valueVal);
            }
            else
            {
                obj.Variables[memberName] = valueVal;
            }
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
            var result = context.GetObject(str);

            if (result == null)
                throw new InvalidOperationException();

            context.Stack.Push(result);
        }
    }

    /// <summary>
    /// Set a string variable in the current scope
    /// </summary>
    public sealed class SetStringVar : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_SetStringVar;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            var name = context.Stack.Pop().ToString();
            context.Scope.Variables[name] = Parameters[0];
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
            Value variable = Value.Undefined();
            if(context.Scope.Variables.ContainsKey(variableName.ToString()))
            {
                variable = context.Scope.Variables[variableName.ToString()];
            }

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
        public override InstructionType Type => InstructionType.GetMember;

        public override void Execute(ActionContext context)
        {
            var member = context.Stack.Pop();
            var obj = context.Stack.Pop().ResolveRegister(context).ToObject();

            context.Stack.Push(obj.GetMember(member.ResolveRegister(context).ToString()));
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
            var property = context.Stack.Pop().ToEnum<PropertyType>();
            var target = context.GetTarget(context.Stack.Pop().ToString());

            var prop = target.ToObject().GetProperty(property);
            context.Stack.Push(prop);
        }
    }

    /// <summary>
    /// set a property. Get value, name and object from stack
    /// </summary>
    public sealed class SetProperty : InstructionBase
    {
        public override InstructionType Type => InstructionType.SetProperty;

        public override void Execute(ActionContext context)
        {
            var value = context.Stack.Pop();
            var property = context.Stack.Pop().ToEnum<PropertyType>();
            var target = context.GetTarget(context.Stack.Pop().ToString());

            target.ToObject().SetProperty(property, value);
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
            var memberVal = context.Stack.Pop().ToString();
            var objectVal = context.Stack.Pop().ToObject();

            objectVal.Variables[memberVal] = Parameters[0];        
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

    /// <summary>
    /// Pops a value from the stack pushes it's type as a string to stack
    /// </summary>
    public sealed class TypeOf : InstructionBase
    {
        public override InstructionType Type => InstructionType.TypeOf;

        public override void Execute(ActionContext context)
        {
            var val = context.Stack.Pop();
            Value result = null;

            switch (val.Type)
            {
                case ValueType.String:
                    result = Value.FromString("string");
                    break;
                case ValueType.Boolean:
                    result = Value.FromString("boolean");
                    break;
                case ValueType.Integer:
                case ValueType.Short:
                case ValueType.Float:
                    result = Value.FromString("number");
                    break;
                case ValueType.Object:
                    if(val.ToObject().Item.Character is Movie)
                        result = Value.FromString("movieclip");
                    else
                        result = Value.FromString("object");
                    break;
                case ValueType.Function:
                    result = Value.FromString("function");
                    break;
                case ValueType.Undefined:
                    result = Value.FromString("undefined");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            context.Stack.Push(result);
        }
    }
}
