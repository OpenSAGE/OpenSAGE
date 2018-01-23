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

        public ActionContext()
        {
            Stack = new Stack<Value>();
        }

        //check for special object names
        public Value GetObject(string name)
        {
            Value obj = null;

            switch(name)
            {
                case "_root":
                    obj = Value.FromObject(Apt.Root.ScriptObject);
                    break;
                case "_parent":
                case "extern":
                    throw new NotImplementedException();  
                //string must be a property
                default:
                    obj = Scope.Properties[name];
                    break;
            }

            return obj;
        }
    }
}
