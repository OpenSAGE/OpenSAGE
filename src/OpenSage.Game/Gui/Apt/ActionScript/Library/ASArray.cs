using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    class ASArray : BuiltinClass
    {
        List<Value> _values;

        public ASArray(Value[] args) : base()
        {
            _values = new List<Value>(args);


            // list of builtin variables
            _builtinVariablesGet.Add("length", () => Value.FromInteger(_values.Count));
        }
    }
}
