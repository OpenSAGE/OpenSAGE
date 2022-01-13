using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace OpenAS2.Runtime.Library
{
    public class HostObject: ESObject
    {
        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            // properties
            ["_parent"] = (avm) => PropertyDescriptor.A(
                (tv) => ((HostObject) tv).Item == null ? Value.Undefined() :
                        ((HostObject) tv).AnotherGetParent(),
                (tv, val) => { throw new NotImplementedException(); }, 
                false, false),
            ["_x"] = (avm) => PropertyDescriptor.A(
                (tv) => ((HostObject) tv).Item == null ? Value.Undefined() :
                        Value.FromFloat(((HostObject) tv).Item.Transform.GeometryTranslation.X),
                (tv, val) => { throw new NotImplementedException(); },
                false, false),
            ["_y"] = (avm) => PropertyDescriptor.A(
                (tv) => ((HostObject) tv).Item == null ? Value.Undefined() :
                        Value.FromFloat(((HostObject) tv).Item.Transform.GeometryTranslation.Y),
                (tv, val) => { throw new NotImplementedException(); },
                false, false),
            ["_name"] = (avm) => PropertyDescriptor.A(
                (tv) => ((HostObject) tv).Item == null ? Value.Undefined() :
                        Value.FromString(((HostObject) tv).Item.Name),
                (tv, val) => { throw new NotImplementedException(); },
                false, false),
            ["_alpha"] = (avm) => PropertyDescriptor.A(
                (tv) => ((HostObject) tv).Item == null ? Value.Undefined() :
                        Value.FromFloat(((HostObject) tv).Item.Transform.ColorTransform.A * 100),
                (tv, val) =>
                {
                    var ctx = (HostObject) tv;
                    if (ctx is null)
                        return;
                    var transform = ctx.Item.Transform;
                    ctx.Item.Transform =
                        transform.WithColorTransform(transform.ColorTransform.WithA((float) (val.ToFloat() / 100.0)));
                },
                false, false),
            // methods
            // nothing
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
        public HostObject(DisplayItem item, VirtualMachine vm = null, string prototype_indicator = "Object") : base(vm == null ? (item == null ? null : item.Context.Avm) : vm, prototype_indicator)
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
        public Value ResolveValue(string value, HostObject ctx)
        {
            var path = value.Split('.');
            var obj = ctx.GetParent();
            var member = path.Last();

            for (var i = 0; i < path.Length - 1; i++)
            {
                var fragment = path[i];

                if (obj.IHasProperty(fragment))
                {
                    obj = (HostObject) obj.IGet(fragment).ToObject();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return obj.IGet(member);
        }

        public HostObject GetParent()
        {
            HostObject result = null;

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

    public class TextField: HostObject
    {
        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>(HostObject.PropertiesDefined)
        {
            ["textColor"] = (avm) => PropertyDescriptor.A(
                (tv) => Value.FromString(((Text) ((HostObject)tv).Item.Character).Color.ToHex()),
                (tv, val) =>
                {
                    var ctx = (HostObject) tv;
                    var hexStr = val.ToString();
                    var hexColor = Convert.ToInt32(hexStr, 16);

                    var b = (hexColor & 0xFF) / 255.0f;
                    var g = ((hexColor & 0xFF00) >> 8) / 255.0f;
                    var r = ((hexColor & 0xFF0000) >> 16) / 255.0f;

                    var transform = ctx.Item.Transform;
                    ctx.Item.Transform =
                        transform.WithColorTransform(transform.ColorTransform.WithRGB(r, g, b));
                },
                true, false),
        };

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>(HostObject.StaticPropertiesDefined)
        {
            
        };

        public TextField(VirtualMachine vm) : this(null, vm) { }
        public TextField(RenderItem item, VirtualMachine vm = null) : base(item, vm, "TextField") { }
    }

    public class MovieClip : HostObject
    {
        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>(HostObject.PropertiesDefined)
        {
            // properties
            ["_currentframe"] = (avm) => PropertyDescriptor.A(
                (tv) => Value.FromInteger(((SpriteItem) ((HostObject) tv).Item).CurrentFrame),
                (tv, val) =>
                {
                    throw new NotImplementedException();
                },
                false, false),

            // methods
            ["gotoAndPlay"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     ((MovieClip) tv).GotoAndPlay(actx, args);
                     return null;
                 }
                 , avm)), true, false, false),
            ["gotoAndStop"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     ((MovieClip) tv).GotoAndStop(args);
                     return null;
                 }
                 , avm)), true, false, false),
            ["stop"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     ((MovieClip) tv).Stop();
                     return null;
                 }
                 , avm)), true, false, false),
            ["loadMovie"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     ((MovieClip) tv).LoadMovie(actx, args);
                     return null;
                 }
                 , avm)), true, false, false),
            ["attachMovie"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     var m = ((MovieClip) tv).AttachMovie(actx, args);
                     return Value.FromObject(m);
                 }
                 , avm)), true, false, false),
        };

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>(HostObject.StaticPropertiesDefined)
        {
            
        };

        public MovieClip(VirtualMachine vm) : this(null, vm) { }
        public MovieClip(SpriteItem item, VirtualMachine vm = null) : base(item, vm, "MovieClip") { }

        public void GotoAndPlay(ExecutionContext actx, Value[] args)
        {
            if (Item is SpriteItem si)
            {
                var dest = args.First().ResolveRegister(actx);

                if (dest.Type == ValueType.String)
                {
                    si.Goto(dest.ToString());
                }
                else if (dest.Type == ValueType.Integer)
                {
                    si.GotoFrame(dest.ToInteger() - 1);
                }
                else
                {
                    throw new InvalidOperationException("Can only jump to labels or frame numbers");
                }

                si.Play();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void GotoAndStop(Value[] args)
        {
            if (Item is SpriteItem si)
            {
                var dest = args.First();

                if (dest.Type == ValueType.String)
                {
                    si.Goto(dest.ToString());
                }
                else if (dest.Type == ValueType.Integer)
                {
                    si.GotoFrame(dest.ToInteger() - 1);
                }
                else
                {
                    throw new InvalidOperationException("Can only jump to labels or frame numbers");
                }

                si.Stop(true);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void Stop()
        {
            if (Item is SpriteItem si)
            {
                si.Stop();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void LoadMovie(ExecutionContext context, Value[] args)
        {
            var url = Path.ChangeExtension(args[0].ToString(), ".apt");
            // TODO Change to MovieHandler
            var window = context.Dom.LoadAptWindowAndQueryPush(url);
        }

        public MovieClip AttachMovie(ExecutionContext context, Value[] args)
        {
            var url = Path.ChangeExtension(args[0].ToString(), ".apt");
            var name = args[1].ToString();
            var depth = args[2].ToInteger();

            throw new NotImplementedException();
        }

    }
}
