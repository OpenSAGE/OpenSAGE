using System.Collections.Generic;
using System;

namespace OpenAS2.Runtime.Library
{
    class ASString : ASObject
    {
        public static new Dictionary<string, Func<VirtualMachine, Property>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, Property>>()
        {
            // properties
            ["constructor"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     ((ASString) tv).PrototypeInternal = actx.Avm.Prototypes["String"];
                     ((ASString) tv)._value = args[0].ToString();
                     return Value.FromObject(tv);
                 }
                 , avm)), true, false, false),
            ["length"] = (avm) => Property.A(
                (tv) => Value.FromInteger(((ASString) tv).GetLength()),
                null
                , false, false),
            // methods
            ["substr"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (vm, tv, args) => ((ASString) tv).substr(args)
                 , avm)), true, false, false),
            // TODO
        };

        public static new Dictionary<string, Func<VirtualMachine, Property>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, Property>>()
        {
            
        };

        private string _value;

        public ASString(VirtualMachine vm) : this(null, vm) { }
        public ASString(Value value, VirtualMachine vm) : base(vm, "String")
        {
            if (value == null) _value = "null";
            else _value = value.ToString();
        }
        public int GetLength() { return _value.Length; }

        public Value substr(Value[] args)
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

        public override string ToString()
        {
            return _value;
        }
    }
}
