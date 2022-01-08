using System;
using OpenAS2.Runtime.Library;
using System.Linq;
using OpenAS2.Base;

namespace OpenAS2.Runtime.Opcodes
{
    /// <summary>
    /// Pop an object from stack and retrieve the member name from the pool. Push the member of
    /// the object to stack.
    /// </summary>
    public sealed class GetNamedMember : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.EA_GetNamedMember;
        public override uint Size => 1;
        public override bool PushStack => true;
        public override bool PopStack => true;

        public override void Execute(ExecutionContext context)
        {
            var id = Parameters[0].ToInteger();
            var member = context.Constants[id].ToString();

            //pop the object
            var objectVal = context.Pop();
            var obj = objectVal.ToObject();

            if (obj != null)
            {
                context.Push(obj.IGet(member));
            }
            else
            {
                context.Push(Value.Undefined());
            }
        }
        public override int Precendence => 18;
        public override string ToString(string[] p)
        {
            return $"{p[1]}.{p[0]}";
        }
    }

    /// <summary>
    /// set the member of a specific object (everything on stack)
    /// </summary>
    public sealed class SetMember : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.SetMember;
        public override uint StackPop => 3;

        public override void Execute(ExecutionContext context)
        {
            //pop the value
            var valueVal = context.Pop();
            //pop the member name
            var memberName = context.Pop().ToString();
            //pop the object
            var obj = context.Pop().ToObject();
            if (obj is null)
                throw new NotImplementedException("Do not know what to do in this situation");
            else
                obj.IPut(memberName, valueVal);

        }
        public override int Precendence => 3;
        public override string ToString(string[] p)
        {
            return $"{p[2]}.{p[1]} = {p[0]}";
        }
    }

    /// <summary>
    /// Get a variable from the current object and push it to the stack
    /// </summary>
    public sealed class GetStringVar : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.EA_GetStringVar;
        public override uint Size => 4;
        public override bool PushStack => true;

        public override void Execute(ExecutionContext context)
        {
            var str = Parameters[0].ToString();

            // check if this a special object, like _root, _parent etc.
            // this is automatically done by the built-in variables in the global object.
            var result = context.GetValueOnChain(str);

            if (result == null)
                throw new InvalidOperationException();

            context.Push(result);
        }
        public override string ToString(string[] p)
        {
            return Parameters[0].ToString();
        }
    }

    /// <summary>
    /// Set a string variable in the current scope
    /// </summary>
    public sealed class SetStringVar : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.EA_SetStringVar;
        public override uint Size => 4;
        public override bool PopStack => true;

        public override void Execute(ExecutionContext context)
        {
            var name = context.Pop().ToString();
            context.This.IPut(name, Parameters[0]);
        }
        public override int Precendence => 3;
        public override string ToString(string[] p)
        {
            return $"this.{p[0]} = {Parameters[0]}";
        }
    }

    /// <summary>
    /// Pops variable name and pushes the corresponding variable back to stack
    /// </summary>
    public sealed class GetVariable : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.GetVariable;
        public override bool PopStack => true;
        public override bool PushStack => true;

        public override void Execute(ExecutionContext context)
        {
            //pop the value
            var variableName = context.Pop().ToString();
            Value variable = context.This.IGet(variableName);
            context.Push(variable);
        }
        public override int Precendence => 18;
        public override string ToString(string[] p)
        {
            return $"this.{p[0]}";
        }
    }

    /// <summary>
    /// Pops variable name and value from the stack. Then set the variable to that value.
    /// </summary>
    public sealed class SetVariable : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.SetVariable;
        public override uint StackPop => 2;

        public override void Execute(ExecutionContext context)
        {
            var valueVal = context.Pop();
            var memberName = context.Pop().ToString();
            context.This.IPut(memberName, valueVal);
        }
        public override int Precendence => 3;
        public override string ToString(string[] p)
        {
            return $"this.{p[1]} = {p[0]}";
        }
    }

    /// <summary>
    /// get the member of a specific object. Result will be pushed to stack
    /// </summary>
    public sealed class GetMember : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.GetMember;
        public override uint StackPop => 2;
        public override bool PushStack => true;

        public override void Execute(ExecutionContext context)
        {
            var member = context.Pop();
            var obj = context.Pop().ToObject();

            // TODO What about arrays?
            context.Push(obj.IGet(member.ToString()));
        }
        public override int Precendence => 18;
        public override string ToString(string[] p)
        {
            return $"{p[1]}.{p[0]}";
        }
    }

    /// <summary>
    /// get a property and push it to the stack
    /// </summary>
    public sealed class GetProperty : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.GetProperty;
        public override uint StackPop => 2;
        public override bool PushStack => true;

        public override void Execute(ExecutionContext context)
        {
            var property = context.Pop().ToEnum<PropertyType>();
            var target = context.GetTarget(context.Pop().ToString());

            var prop = ((StageObject) target.ToObject()).GetProperty(property);
            context.Push(prop);
        }
        public override int Precendence => 18;
        public override string ToString(string[] p)
        {
            return $"(target)\"{p[1]}\".{p[0]}";
        }
    }

    /// <summary>
    /// set a property. Get value, name and object from stack
    /// </summary>
    public sealed class SetProperty : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.SetProperty;
        public override uint StackPop => 3;

        public override void Execute(ExecutionContext context)
        {
            var value = context.Pop();
            var property = context.Pop().ToEnum<PropertyType>();
            var target = context.GetTarget(context.Pop().ToString());

             target.ToObject<StageObject>().SetProperty(property, value);
        }
        public override int Precendence => 3;
        public override string ToString(string[] p)
        {
            return $"(target)\"{p[2]}\".{p[1]} = {p[0]}";
        }
    }

    /// <summary>
    /// clones a sprite to assigned depth and target.
    /// </summary>
    public sealed class CloneSprite : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.CloneSprite;
        public override uint StackPop => 3;

        public override void Execute(ExecutionContext context)
        {
            var depth = context.Pop();
            var target = context.Pop();
            var source = context.Pop();
            throw new NotImplementedException();            
        }
    }

    /// <summary>
    /// removes a sprite.
    /// </summary>
    public sealed class RemoveSprite : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.RemoveSprite;
        public override bool PopStack => true;

        public override void Execute(ExecutionContext context)
        {
            var target = context.Pop();
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Pops an object from the stack and retrieves a member variable which is pushed to stack
    /// </summary>
    public sealed class GetStringMember : InstructionMonoPushPop
    {
        public override InstructionType Type => InstructionType.EA_GetStringMember;
        public override uint Size => 4;
        public override bool PopStack => true;
        public override bool PushStack => true;

        public override void Execute(ExecutionContext context)
        {
            //pop the member name
            var memberVal = Parameters[0];

            //pop the object
            var objectVal = context.Pop();

            var valueVal = objectVal.ToObject().IGet(memberVal.ToString());

            context.Push(valueVal);
        }
        public override int Precendence => 18;
        public override string ToString(string[] p)
        {
            return $"{p[0]}.{Parameters[0]}";
        }
    }

    public sealed class SetStringMember : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.EA_SetStringMember;
        public override uint Size => 4;
        public override uint StackPop => 2;

        public override void Execute(ExecutionContext context)
        {
            var memberVal = context.Pop().ToString();
            var objectVal = context.Pop().ToObject();

            objectVal.IPut(memberVal, Parameters[0]);
        }
        public override int Precendence => 3;
        public override string ToString(string[] p)
        {
            return $"{p[1]}.{p[0]} = {Parameters[0]}";
        }
    }

    /// <summary>
    /// Create a new object by calling it's constructor
    /// </summary>
    public sealed class NewObject : InstructionBase
    {
        public override InstructionType Type => InstructionType.NewObject;
        public override bool IsStatement => false;

        public override void Execute(ExecutionContext context)
        {
            var name = context.Pop().ToString();
            var args = FunctionCommon.GetArgumentsFromStack(context);

            context.ConstructObjectAndPush(name, args);
        }
        public override int Precendence => 18;
        public override string ToString(string[] p)
        {
            return $"new {p[0]}({string.Join(", ", p.Skip(1))})";
        }
    }

    /// <summary>
    /// From its name, looks like it corresponds to the method definition of ECMAScript.
    /// Since it doesn't have any parameters, it should be using the stack.
    /// </summary>
    public sealed class NewMethod : InstructionBase
    {
        public override InstructionType Type => InstructionType.NewMethod;
        public override bool IsStatement => false;

        public override void Execute(ExecutionContext context)
        {
            var nameVal = context.Pop();
            var name = nameVal.ToString();
            var obj = context.Pop();
            var args = FunctionCommon.GetArgumentsFromStack(context);
            
            if (nameVal.Type != ValueType.Undefined && name.Length != 0) obj = obj.ToObject().IGet(name);

            context.ConstructObjectAndPush(obj, args);
            
        }
        public override int Precendence => 18;
        public override string ToString(string[] p)
        {
            return $"new {p[1]}{(p[0] == "undefined" || p[0] == "" ? "" : ".")}{p[0]}({string.Join(", ", p.Skip(2))})";
        }
    }

    /// <summary>
    /// Initializes an object from the stack
    /// </summary>
    public sealed class InitObject : InstructionBase
    {
        public override InstructionType Type => InstructionType.InitObject;
        public override bool IsStatement => false;

        public override void Execute(ExecutionContext context)
        {
            var nArgs = context.Pop().ToInteger();
            var obj = new ESObject(context.Avm);
            for (int i = 0; i < nArgs; ++i)
            {
                var vi = context.Pop();
                var ni = context.Pop().ToString();
                obj.IPut(ni, vi);
            }

            context.Push(Value.FromObject(obj));
        }
        public override string ToString(string[] p)
        {
            return $"{{{string.Join(", ", p)}}}";
        }
    }

    /// <summary>
    /// Pops a value from the stack pushes it's type as a string to stack
    /// </summary>
    public sealed class TypeOf : InstructionMonoOperator
    {
        public override Func<Value, Value> Operator => (v) => Value.FromString(v.GetStringType());
        public override InstructionType Type => InstructionType.TypeOf;
        public override int Precendence => 15;
        public override string ToString(string[] p)
        {
            return $"typeof {p[0]}";
        }
    }

    /// <summary>
    /// Set the inheritance structure of a class.
    /// See https://www.adobe.com/content/dam/acom/en/devnet/pdf/swf-file-format-spec.pdf p.114 "ActionExtends"
    /// </summary>
    public sealed class Extends : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.Extends;
        public override uint StackPop => 2;

        public override void Execute(ExecutionContext context)
        {
            // TODO This will override functions in the old prototype of cls.
            // I followed the document, but don't know if will cause issues.
            var sup = context.Pop().ToFunction();
            var cls = context.Pop().ToFunction();
            var obj = new ESObject(context.Avm);
            obj.__proto__ = sup.prototype;
            obj.constructor = sup;
            cls.prototype = obj;
        }
        public override int Precendence => 3;
        public override string ToString(string[] p)
        {
            return $"{p[1]}.prototype.__proto__ = {p[0]}.prototype";
        }
    }

    /// <summary>
    /// Corresponds to the `instanceof` keyword of ECMAScript.
    /// </summary>
    public sealed class InstanceOf : InstructionMonoPush
    {
        public override InstructionType Type => InstructionType.InstanceOf;
        public override uint StackPop => 2;
        public override bool PushStack => true;

        public override void Execute(ExecutionContext context)
        {
            var constr = context.Pop().ToFunction();
            var obj = context.Pop().ToObject();
            var val = obj.InstanceOf(constr);
            context.Push(Value.FromBoolean(val));
        }
        public override int Precendence => 11;
        public override string ToString(string[] p)
        {
            return $"{p[1]} instanceof {p[0]}";
        }
    }

}
