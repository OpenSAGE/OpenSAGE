using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    /// <summary>
    /// This class is meant to access builtin variables and return the corresponding value
    /// </summary>
    public static class Builtin
    {
        private static readonly Dictionary<string, Func<Value[], Value>> BuiltinClasses;
        private static readonly Dictionary<string, Action<ActionContext, ObjectContext, Value[]>> BuiltinFunctions;
        private static readonly Dictionary<string, Func<ObjectContext, Value>> BuiltinVariablesGet;
        private static readonly Dictionary<string, Action<ObjectContext, Value>> BuiltinVariablesSet;

        static Builtin()
        {
            // list of builtin objects and their corresponding constructors
            BuiltinClasses = new Dictionary<string, Func<Value[], Value>>
            {
                ["Color"] = args => Value.FromObject(new Color())
            };

            // list of builtin variables
            BuiltinVariablesGet = new Dictionary<string, Func<ObjectContext, Value>>
            {
                ["_root"] = ctx => Value.FromObject(ctx.Item.Context.Root.ScriptObject),
                ["_global"] = ctx => Value.FromObject(ctx.Item.Context.AVM.GlobalObject),
                ["_parent"] = GetParent,
                ["extern"] = ctx => Value.FromObject(ctx.Item.Context.AVM.ExternObject)
            };

            // list of builtin variables - set
            BuiltinVariablesSet = new Dictionary<string, Action<ObjectContext, Value>>
            {
                ["_alpha"] = (ctx, v) =>
                {
                    Debug.WriteLine("Setting alpha to: " + v.ToInteger());

                    var transform = ctx.Item.Transform;
                    ctx.Item.Transform =
                        transform.WithColorTransform(transform.ColorTransform.WithA(v.ToInteger() / 100.0f));
                }
            };

            // list of builtin functions
            BuiltinFunctions = new Dictionary<string, Action<ActionContext, ObjectContext, Value[]>>
            {
                ["gotoAndPlay"] = (actx, ctx, args) => GotoAndPlay(ctx, args),
                ["stop"] = (actx, ctx, args) => Stop(ctx),
                ["clearInterval"] = ClearInterval,
                ["setInterval"] = SetInterval
            };
        }

        public static bool IsBuiltInClass(string name)
        {
            return BuiltinClasses.ContainsKey(name);
        }

        public static bool IsBuiltInFunction(string name)
        {
            return BuiltinFunctions.ContainsKey(name);
        }

        public static bool IsBuiltInVariable(string name)
        {
            return BuiltinVariablesGet.ContainsKey(name) || BuiltinVariablesSet.ContainsKey(name);
        }

        public static void CallBuiltInFunction(string name, ActionContext actx, ObjectContext ctx, Value[] args)
        {
            BuiltinFunctions[name](actx, ctx, args);
        }

        public static Value GetBuiltInVariable(string name, ObjectContext ctx)
        {
            return BuiltinVariablesGet[name](ctx);
        }

        public static void SetBuiltInVariable(string name, ObjectContext ctx, Value val)
        {
            BuiltinVariablesSet[name](ctx, val);
        }

        public static Value GetBuiltInClass(string name, Value[] args)
        {
            return BuiltinClasses[name](args);
        }

        private static Value GetParent(ObjectContext ctx)
        {
            var item = ctx.Item;

            // Parent of a render item is the parent of the containing sprite
            var parent = item is RenderItem ? item.Parent.Parent.ScriptObject : item.Parent.ScriptObject;

            return Value.FromObject(parent);
        }

        private static void GotoAndPlay(ObjectContext ctx, Value[] args)
        {
            if (ctx.Item is SpriteItem si)
            {
                if (args.First().Type == ValueType.String)
                {
                    si.Goto(args.First().ToString());
                }
                else if (args.First().Type == ValueType.Integer)
                {
                    si.GotoFrame(args.First().ToInteger() - 1);
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

        private static void Stop(ObjectContext ctx)
        {
            if (ctx.Item is SpriteItem si)
            {
                si.Stop();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private static void SetInterval(ActionContext actionContext, ObjectContext ctx, Value[] args)
        {
            var vm = actionContext.Apt.AVM;
            var name = actionContext.Stack.Pop().ToString();

            vm.CreateInterval(name, args[1].ToInteger(), args[0].ToFunction(), ctx, new Value[0]);

            ctx.Variables[name] = Value.FromString(name);
        }

        private static void ClearInterval(ActionContext actionContext, ObjectContext ctx, Value[] args)
        {
            var vm = actionContext.Apt.AVM;
            var name = args[0].ToString();

            vm.ClearInterval(name);
            ctx.Variables.Remove(name);
        }
    }
}
