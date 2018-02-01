using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            switch (name)
            {
                case "_root":
                    obj = Value.FromObject(Apt.Root.ScriptObject);
                    break;
                case "extern":
                    obj = Value.FromObject(Apt.ActionScriptVM.ExternObject);
                    break;
                case "_parent":
                    throw new NotImplementedException();
                    break;
                //string must be a variable of current scope
                default:
                    obj = Scope.GetMember(name);
                    break;
            }

            return obj;
        }
    }
}
