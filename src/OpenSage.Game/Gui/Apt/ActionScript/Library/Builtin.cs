using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    public sealed class Builtin
    {
        private static readonly Dictionary<string, Func<Value[], Value>> _builtinClasses;
        private static readonly Dictionary<string, Action<ObjectContext,Value[]>> _builtinFunctions;
        private static readonly Dictionary<string, Func<ObjectContext, Value>> _builtinVariables;

        static Builtin()
        {
            //list of builtin objects and their corresponding constructors
            _builtinClasses = new Dictionary<string, Func<Value[],Value>>();
            _builtinClasses["Color"] = (Value[] args) => { return Value.FromObject(new Color()); };

            //list of builtin variables
            _builtinVariables = new Dictionary<string, Func<ObjectContext, Value>>();
            _builtinVariables["_root"] = (ObjectContext ctx) => { return Value.FromObject(ctx.Item.Context.Root.ScriptObject); };
            _builtinVariables["_global"] = (ObjectContext ctx) => { return Value.FromObject(ctx.Item.Context.AVM.GlobalObject); };
            _builtinVariables["_parent"] = (ObjectContext ctx) => { return GetParent(ctx); };
            _builtinVariables["extern"] = (ObjectContext ctx) => { return Value.FromObject(ctx.Item.Context.AVM.ExternObject); };

            //list of builtin functions
            _builtinFunctions = new Dictionary<string, Action<ObjectContext,Value[]>>();
            _builtinFunctions["gotoAndPlay"] = (ObjectContext ctx,Value[] args) => { GotoAndPlay(ctx, args); };
            _builtinFunctions["stop"] = (ObjectContext ctx, Value[] args) => { Stop(ctx); };
            _builtinFunctions["setInterval"] = (ObjectContext ctx, Value[] args) => { SetInterval(ctx,args); };
        }


        public static bool IsBuiltInClass(string name)
        {
            if(_builtinClasses.ContainsKey(name))
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

        public static void CallBuiltInFunction(string name, ObjectContext ctx,Value[] args)
        {
            _builtinFunctions[name](ctx,args);
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
            if(item is RenderItem)
            {
                parent = item.Parent.Parent.ScriptObject; ;
            }
            else
            {
                parent = item.Parent.ScriptObject; ;
            }

            return Value.FromObject(parent);
        }

        private static void GotoAndPlay(ObjectContext ctx,Value[] args)
        {
            if(ctx.Item is SpriteItem si)
            {
                si.Goto(args.First().ToString());
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

        private static void SetInterval(ObjectContext ctx,Value[] args)
        {

        }
    }
}
