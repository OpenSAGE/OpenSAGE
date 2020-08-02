using System;
using System.Collections.Generic;
using System.Text;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    class ASArray : ObjectContext
    {
        private readonly Dictionary<string, Action<Value[]>> _builtinFunctions;
        private readonly Dictionary<string, Func<Value>> _builtinVariablesGet;
        private readonly Dictionary<string, Action<Value>> _builtinVariablesSet;

        List<Value> _values;

        public ASArray(Value[] args)
        {
            _values = new List<Value>(args);

            //list of builtin functions
            _builtinFunctions = new Dictionary<string, Action<Value[]>>
            {
            };

            // list of builtin variables
            _builtinVariablesGet = new Dictionary<string, Func<Value>>
            {
                ["length"] = () => Value.FromInteger(_values.Count)
            };
        }

        public override bool IsBuiltInVariable(string name)
        {
            if (int.TryParse(name, out int index))
            {
                return index < _values.Count;
            }

            return _builtinVariablesGet.ContainsKey(name);
        }

        public override Value GetBuiltInVariable(string name)
        {
            if (int.TryParse(name, out int index))
            {
                return _values[index];
            }

            return _builtinVariablesGet[name]();
        }

        public override bool IsBuiltInFunction(string name)
        {
            if (_builtinFunctions.ContainsKey(name))
            {
                return true;
            }

            return false;
        }

        public override void CallBuiltInFunction(ActionContext actx, string name, Value[] args)
        {
            _builtinFunctions[name](args);
        }
    }
}
