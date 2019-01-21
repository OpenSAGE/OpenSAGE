using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Gui.Apt.ActionScript.Library;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class ActionContext
    {
        public ObjectContext Scope { get; set; }
        public ObjectContext Global { get; set; }
        public AptContext Apt { get; set; }
        public InstructionStream Stream { get; set; }
        public Stack<Value> Stack { get; set; }
        public Value[] Registers { get; set; }
        public Dictionary<string, Value> Params { get; set; }
        public Dictionary<string, Value> Locals { get; set; }
        public bool Return { get; set; }

        public ActionContext(int numRegisters = 0)
        {
            Stack = new Stack<Value>();
            Registers = new Value[numRegisters];
            Params = new Dictionary<string, Value>();
            Locals = new Dictionary<string, Value>();
            Return = false;
        }

        /// <summary>
        /// Check if a specific string is a parameter in the current params
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns></returns>
        public bool CheckParameter(string name)
        {
            return Params.ContainsKey(name);
        }

        /// <summary>
        /// Returns the value of a parameter. Must be used with CheckParameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns></returns>
        public Value GetParameter(string name)
        {
            return Params[name];
        }

        /// <summary>
        /// Check if a specific string is a local value to this context
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public bool CheckLocal(string name)
        {
            return Locals.ContainsKey(name);
        }

        /// <summary>
        /// Returns the value of a local variable. Must be used with CheckLocal
        /// </summary>
        /// <param name="name">local name</param>
        /// <returns></returns>
        public Value GetLocal(string name)
        {
            return Locals[name];
        }


        /// <summary>
        /// Checks for special handled/global objects. After that checks for child objects of the
        /// current object
        /// </summary>
        /// <param name="name">the object name</param>
        /// <returns></returns>
        public Value GetObject(string name)
        {
            Value obj = null;

            if(Builtin.IsBuiltInVariable(name))
            {
                obj = Builtin.GetBuiltInVariable(name, Scope);
            }
            else
            {
                obj = Scope.GetMember(name);
            }

            return obj;
        }

        /// <summary>
        /// Get the current target path
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Value GetTarget(string target)
        {
            //empty target means the current scope
            if (target.Length == 0)
                return Value.FromObject(Scope);

            //depending on wether or not this is a relative path or not
            ObjectContext obj = target.First()=='/' ? Apt.Root.ScriptObject : Scope;

            foreach(var part in target.Split('/'))
            {
                if (part == "..")
                {
                    obj = obj.GetParent();
                }
                else
                {
                    obj = obj.Variables[part].ToObject();
                }
            }

            return Value.FromObject(obj);
        }

        /// <summary>
        /// Call an object constructor. Either builtin or defined earlier
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public Value ConstructObject(string name,Value[] args)
        {
            Value result = null;
            if(Builtin.IsBuiltInClass(name))
            {
                result = Builtin.GetBuiltInClass(name,args);
            }
            else
            {
                throw new NotImplementedException();
            }

            return result;
        }

        /// <summary>
        /// Preload specified variables
        /// </summary>
        /// <param name="flags">the flags</param>
        public void Preload(FunctionPreloadFlags flags)
        {
            //preloaded variables start at register 1
            int reg = 1;

            //order is important
            if (flags.HasFlag(FunctionPreloadFlags.PreloadThis))
            {
                Registers[reg] = Value.FromObject(Scope);
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadArguments))
            {
                throw new NotImplementedException();
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadSuper))
            {
                throw new NotImplementedException();
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadRoot))
            {
                Registers[reg] = Value.FromObject(Apt.Root.ScriptObject);
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadParent))
            {
                Registers[reg] = Value.FromObject(Scope.GetParent());
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadGlobal))
            {
                Registers[reg] = Value.FromObject(Apt.Avm.GlobalObject);
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadExtern))
            {
                Registers[reg] = Value.FromObject(Apt.Avm.ExternObject);
                ++reg;
            }
            if (!flags.HasFlag(FunctionPreloadFlags.SupressSuper))
            {
                throw new NotImplementedException();
            }

            if (!flags.HasFlag(FunctionPreloadFlags.SupressArguments))
            {
                throw new NotImplementedException();
            }

            if (!flags.HasFlag(FunctionPreloadFlags.SupressThis))
            {
                Locals["this"] = Value.FromObject(Scope);
            }
        }
    }
}
