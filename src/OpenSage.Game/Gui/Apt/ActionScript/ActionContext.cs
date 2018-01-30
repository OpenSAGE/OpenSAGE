using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


        public ActionContext(int numRegisters = 0)
        {
            Stack = new Stack<Value>();
            Registers = new Value[numRegisters];
            Params = new Dictionary<string, Value>();
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
                    obj = obj.Item.Parent.ScriptObject;
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
    }
}
