using System;

namespace OpenSage.Gui.Apt.ActionScript.Library
{
    class ASString : BuiltinClass
    {
        string _value;

        public ASString(string value) : base()
        {
            _value = value;

            //list of builtin functions
            _builtinFunctions.Add("substr", substr);

            // list of builtin variables
            _builtinVariablesGet.Add("length", () => Value.FromInteger(_value.Length));
        }

        private Value substr(Value[] args)
        {
            if (args.Length == 1)
            {
                return Value.FromString(_value.Substring(args[0].ToInteger()));
            }
            else if (args.Length == 2)
            {
                return Value.FromString(_value.Substring(args[0].ToInteger(), args[1].ToInteger()));
            }
            else
            {
                throw new InvalidOperationException("Argument count invalid!");
            }
        }
    }
}
