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
            var objectVal = context.Pop();
            var obj = objectVal.ToObject();

            if (obj != null)
            {
                context.Push(obj.GetMember(member));
            }
            else
            {
                context.Push(Value.Undefined());
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
            var valueVal = context.Pop();
            //pop the member name
            var memberName = context.Pop().ToString();
            //pop the object
            var p = context.Pop();
            var obj = p.ToObject();

            if (obj.IsBuiltInVariable(memberName))
            {
                obj.SetBuiltInVariable(memberName, valueVal);
            }
            else
            {
                obj.SetMember(memberName, valueVal);
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

            context.Push(result);
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
            var name = context.Pop().ToString();
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
            var variableName = context.Pop();
            Value variable = Value.Undefined();
            if (context.Scope.Variables.ContainsKey(variableName.ToString()))
            {
                variable = context.Scope.Variables[variableName.ToString()];
            }

            context.Push(variable);
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
            var valueVal = context.Pop();
            //pop the member name
            var memberName = context.Pop().ToString();

            if (context.CheckLocal(memberName))
            {
                context.Locals[memberName] = valueVal;
            }
            else
            {
                context.Scope.Variables[memberName] = valueVal;
            }
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
            var member = context.Pop();
            var obj = context.Pop().ToObject();

            context.Push(obj.GetMember(member.ToString()));
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
            var property = context.Pop().ToEnum<PropertyType>();
            var target = context.GetTarget(context.Pop().ToString());

            var prop = target.ToObject().GetProperty(property);
            context.Push(prop);
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
            var value = context.Pop();
            var property = context.Pop().ToEnum<PropertyType>();
            var target = context.GetTarget(context.Pop().ToString());

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
            var objectVal = context.Pop();

            var valueVal = objectVal.ToObject().GetMember(memberVal.ToString());

            context.Push(valueVal);
        }
    }

    public sealed class SetStringMember : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_SetStringMember;
        public override uint Size => 4;

        public override void Execute(ActionContext context)
        {
            var memberVal = context.Pop().ToString();
            var objectVal = context.Pop().ToObject();

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
            var name = context.Pop().ToString();
            var args = FunctionCommon.GetArgumentsFromStack(context);

            var obj = context.ConstructObject(name, args);
            context.Push(obj);
        }
    }

    /// <summary>
    /// From its name, looks like it corresponds to the method definition of ECMAScript.
    /// Since it doesn't have any parameters, it should be using the stack.
    /// </summary>
    public sealed class NewMethod : InstructionBase
    {
        public override InstructionType Type => InstructionType.NewMethod;

        public override void Execute(ActionContext context)
        {
            var name = context.Pop().ToString();
            var obj = context.Pop();
            var args = FunctionCommon.GetArgumentsFromStack(context);

            if (name.Length == 0)
                FunctionCommon.ExecuteFunction(obj, args, context.Scope, context);
            // throw new NotImplementedException("what the hell is a construction function?");
            else
                FunctionCommon.ExecuteFunction(name, args, obj.ToObject(), context);

            //WRONG!!!!
            // pop it if the value is undefined
            //var ret = context.Peek();
            //if (ret.Type.Equals(ValueType.Undefined))
            //    context.Pop();
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
            var nArgs = context.Pop().ToInteger();
            var obj = new ObjectContext();
            for (int i = 0; i < nArgs; ++i)
            {
                var vi = context.Pop();
                var ni = context.Pop().ToEnum<PropertyType>();
                obj.SetProperty(ni, vi);
            }

            context.Push(Value.FromObject(obj));
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
            var val = context.Pop();
            Value result = null;

            switch (val.Type)
            {
                case ValueType.String:
                    result = Value.FromString("string");
                    break;
                case ValueType.Boolean:
                    result = Value.FromString("boolean");
                    break;
                case ValueType.UInteger:
                case ValueType.Integer:
                case ValueType.Short:
                case ValueType.Float:
                    result = Value.FromString("number");
                    break;
                case ValueType.Object:
                    if (val.ToObject().Item.Character is Movie)
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

            context.Push(result);
        }
    }

    /// <summary>
    /// From its name, looks like it extends an object.
    /// Since it doesn't have any parameters, it might extend objects from the stack.
    /// </summary>
    public sealed class Extends : InstructionBase
    {
        public override InstructionType Type => InstructionType.Extends;

        public override void Execute(ActionContext context)
        {
            // throw new NotImplementedException(context.DumpStack());
            var sup = context.Pop().ToFunction();
            var cls = context.Pop().ToFunction();
            var obj = new ObjectContext();
            obj.__proto__ = sup.prototype;
            obj.constructor = sup;
            cls.prototype = obj;
            // Do not push it back
            // context.Push(Value.FromObject(obj));
        }
    }

    /// <summary>
    /// From its name, looks like it corresponds to the `instanceof` keyword of ECMAScript.
    /// Since it doesn't have any parameters, it should be using the stack.
    /// </summary>
    public sealed class InstanceOf : InstructionBase
    {
        public override InstructionType Type => InstructionType.InstanceOf;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException(context.DumpStack());

        }
    }

}
