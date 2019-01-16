using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    /// <summary>
    /// This class is meant to access builtin variables and return the corresponding value
    /// </summary>
    public sealed class Builtin
    {
        private static readonly Dictionary<string, Func<Value[], Value>> _builtinClasses;
        private static readonly Dictionary<string, Action<ActionContext, ObjectContext, Value[]>> _builtinFunctions;
        private static readonly Dictionary<string, Func<ObjectContext, Value>> _builtinVariables;

        static Builtin()
        {
            //list of builtin objects and their corresponding constructors
            _builtinClasses = new Dictionary<string, Func<Value[], Value>>();
            _builtinClasses["Color"] = (Value[] args) => { return Value.FromObject(new Color()); };

            //list of builtin variables
            _builtinVariables = new Dictionary<string, Func<ObjectContext, Value>>();
            _builtinVariables["_root"] = (ObjectContext ctx) => { return Value.FromObject(ctx.Item.Context.Root.ScriptObject); };
            _builtinVariables["_global"] = (ObjectContext ctx) => { return Value.FromObject(ctx.Item.Context.AVM.GlobalObject); };
            _builtinVariables["_parent"] = (ObjectContext ctx) => { return GetParent(ctx); };
            _builtinVariables["extern"] = (ObjectContext ctx) => { return Value.FromObject(ctx.Item.Context.AVM.ExternObject); };

            //list of builtin functions
            _builtinFunctions = new Dictionary<string, Action<ActionContext, ObjectContext, Value[]>>();
            _builtinFunctions["gotoAndPlay"] = (ActionContext actx, ObjectContext ctx, Value[] args) => { GotoAndPlay(ctx, args); };
            _builtinFunctions["stop"] = (ActionContext actx, ObjectContext ctx, Value[] args) => { Stop(ctx); };
            _builtinFunctions["clearInterval"] = (ActionContext actx, ObjectContext ctx, Value[] args) => { ClearInterval(actx, ctx, args); };
            _builtinFunctions["setInterval"] = (ActionContext actx, ObjectContext ctx, Value[] args) => { SetInterval(actx, ctx, args); };
        }


        public static bool IsBuiltInClass(string name)
        {
            if (_builtinClasses.ContainsKey(name))
            {
                return true;
            }

            return false;
        }

        public static bool IsBuiltInFunction(string name)
        {
            if (_builtinFunctions.ContainsKey(name))
            {
                return true;
            }

            return false;
        }

        public static bool IsBuiltInVariable(string name)
        {
            if (_builtinVariables.ContainsKey(name))
            {
                return true;
            }

            return false;
        }

        public static void CallBuiltInFunction(string name, ActionContext actx, ObjectContext ctx, Value[] args)
        {
            _builtinFunctions[name](actx, ctx, args);
        }

        public static Value GetBuiltInVariable(string name, ObjectContext ctx)
        {
            return _builtinVariables[name](ctx);
        }


        public static Value GetBuiltInClass(string name, Value[] args)
        {
            return _builtinClasses[name](args);
        }

        private static Value GetParent(ObjectContext ctx)
        {
            var item = ctx.Item;
            ObjectContext parent = null;

            //parent of a renderitem is the parent of the containing sprite
            if (item is RenderItem)
            {
                parent = item.Parent.Parent.ScriptObject;
            }
            else
            {
                parent = item.Parent.ScriptObject;
            }

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
                    si.GotoFrame(args.First().ToInteger());
                }
                else
                {
                    throw new InvalidOperationException("Can only jump to labels or framenumbers");
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
