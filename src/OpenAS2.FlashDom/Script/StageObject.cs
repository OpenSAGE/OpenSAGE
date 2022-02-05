using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using OpenAS2.Runtime.Dom;
using OpenAS2.Runtime.Library;
using OpenAS2.Runtime;
using OpenAS2.FlashDom.HostObjects;

namespace OpenAS2.FlashDom.Script
{
    public class StageObject: HostObject
    {
        public static new Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
            ["_parent"] = () => PropertyDescriptor.A(
                (_, tv, _) => ESCallable.Return(((StageObject)tv).Item == null ? Value.Undefined() :
                        ((StageObject)tv).AnotherGetParent()),
                (_, tv, val) => { throw new NotImplementedException(); },
                false, false),
            ["_x"] = () => PropertyDescriptor.A(
                (_, tv, _) => ESCallable.Return(((StageObject)tv).Item == null ? Value.Undefined() :
                        Value.FromFloat(((StageObject)tv).Item.Transform.GeometryTranslation.X)),
                (_, tv, val) => { throw new NotImplementedException(); },
                false, false),
            ["_y"] = () => PropertyDescriptor.A(
                (_, tv, _) => ESCallable.Return(((StageObject)tv).Item == null ? Value.Undefined() :
                        Value.FromFloat(((StageObject)tv).Item.Transform.GeometryTranslation.Y)),
                (_, tv, val) => { throw new NotImplementedException(); },
                false, false),
            ["_name"] = () => PropertyDescriptor.A(
                (_, tv, _) => ESCallable.Return(((StageObject)tv).Item == null ? Value.Undefined() :
                        Value.FromString(((StageObject)tv).Item.Name)),
                (_, tv, val) => { throw new NotImplementedException(); },
                false, false),
            ["_alpha"] = () => PropertyDescriptor.A(
                (_, tv, _) => ESCallable.Return(((StageObject)tv).Item == null ? Value.Undefined() :
                        Value.FromFloat(((StageObject)tv).Item.Transform.ColorTransform.A * 100)),
                (ec, tv, args) =>
                {
                    if (HasArgs(args))
                    {
                        var val = args![0].ToFloat();
                        var ctx = (StageObject)tv;
                        if (ctx is null)
                            return ESCallable.Throw(ec.ConstrutError("TypeError"));
                        var transform = ctx.Item.Transform;
                        ctx.Item.Transform =
                            transform.WithColorTransform(transform.ColorTransform.WithA((float)(val / 100.0)));
                    }
                    return ESCallable.Normal(Value.Undefined());
                    
                },
                false, false),
        };

        public static new readonly Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {
            
        };

        public static new readonly Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {

        };

        public static new readonly Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };


        /// <summary>
        /// The item that this context is connected to
        /// </summary>
        public DisplayItem Item { get; private set; }

        /// <summary>
        /// this ActionScript object is bound to an item
        /// </summary>
        /// <param name="item"></param>
        /// the item that this context is bound to
        public StageObject(DisplayItem item, VirtualMachine? vm = null, string classIndicator = "Object") : base(vm == null ? (item == null ? null : item.Context.Avm) : vm, classIndicator)
        {
            Item = item;
        }

        public override string ToString()
        {
            return Item == null ? "StageObject" : Item.Name;
        }

        /// <summary>
        /// used by text
        /// </summary>
        /// <param name="value">value name</param>
        /// <returns></returns>
        public Value ResolveValue(string value, StageObject ctx)
        {
            var path = value.Split('.');
            var obj = ctx.GetParent();
            var member = path.Last();

            for (var i = 0; i < path.Length - 1; i++)
            {
                var fragment = path[i];

                if (obj.HasMember(fragment))
                {
                    obj = (StageObject) obj.GetMember(fragment).ToObject();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return obj.GetMember(member);
        }

        public override StageObject? GetParent(ExecutionContext ec)
        {
            StageObject? result = null;
            if (Item.Parent != null)
            {
                result = Item.Parent.ScriptObject;
            }
            return result;
        }

        private Value AnotherGetParent()
        {
            // Parent of a render item is the parent of the containing sprite
            // TODO: By doing some search on the web,
            // it seems like in Flash / ActionScript 3, when trying to access
            // the `parent` of root object, null or undefined will be returned.
            var parent = Item is RenderItem ? Item.Parent?.Parent?.ScriptObject : Item.Parent?.ScriptObject;
            return Value.FromObject(parent);
        }

        public Value GetProperty(PropertyType property)
        {
            Value result = null;

            switch (property)
            {
                case PropertyType.Target:
                    result = Value.FromString(GetTargetPath());
                    break;
                case PropertyType.Name:
                    result = Value.FromString(Item.Name);
                    break;
                case PropertyType.X:
                    result = Value.FromFloat(Item.Transform.GeometryTranslation.X);
                    break;
                case PropertyType.Y:
                    result = Value.FromFloat(Item.Transform.GeometryTranslation.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public void SetProperty(PropertyType property, Value val)
        {
            switch (property)
            {
                case PropertyType.Visible:
                    Item.Visible = val.ToBoolean();
                    break;
                case PropertyType.XScale:
                    Item.Transform.Scale((float) val.ToFloat(), 0.0f);
                    break;
                case PropertyType.YScale:
                    Item.Transform.Scale(0.0f, (float) val.ToFloat());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Calculates the absolute target path
        /// </summary>
        /// <returns>the target</returns>
        private string GetTargetPath()
        {
            string path;

            if (GetParent() == null)
                path = "/";
            else
            {
                path = GetParent().GetTargetPath();
                path += Item.Name;
            }

            return path;
        }

    }

}
