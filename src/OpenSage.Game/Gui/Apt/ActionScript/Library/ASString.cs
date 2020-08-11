using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    class ASString : ObjectContext
    {
        private readonly Dictionary<string, Func<Value[], Value>> _builtinFunctions;
        private readonly Dictionary<string, Func<Value>> _builtinVariablesGet;

        string _value;

        public ASString(string value)
        {
            _value = value;

            //list of builtin functions
            _builtinFunctions = new Dictionary<string, Func<Value[], Value>>
            {
                ["substr"] = (Value[] args) => args.Length == 1 ? Value.FromString(_value.Substring(args[0].ToInteger()))
                                                                : Value.FromString(_value.Substring(args[0].ToInteger(), args[1].ToInteger()))
            };

            // list of builtin variables
            _builtinVariablesGet = new Dictionary<string, Func<Value>>
            {
                ["length"] = () => Value.FromInteger(_value.Length)
            };
        }

        public override bool IsBuiltInVariable(string name)
        {
            return _builtinVariablesGet.ContainsKey(name);
        }

        public override Value GetBuiltInVariable(string name)
        {
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
            Value result = _builtinFunctions[name](args);
            if (result.Type != ValueType.Undefined)
            {
                actx.Stack.Push(result);
            }
        }
    }
}
