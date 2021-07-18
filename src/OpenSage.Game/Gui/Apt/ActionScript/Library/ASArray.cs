using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    class ASArray : BuiltinClass
    {


        private List<Value> _values;

        public ASArray(Value[] args) : base()
        {
            _values = new List<Value>(args);


            // list of builtin variables
            _builtinVariablesGet.Add("length", () => Value.FromInteger(_values.Count));
        }

        public Value[] GetValues()
        {
            Value[] ans = new Value[_values.Count];
            _values.CopyTo(0, ans, 0, _values.Count);
            return ans;
        }
    }
}
