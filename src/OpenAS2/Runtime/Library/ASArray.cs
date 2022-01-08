using System.Collections.Generic;
using System;

namespace OpenAS2.Runtime.Library
{
    class ASArray : ESObject
    {
        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            // properties
            ["constructor"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     ((ASArray) tv).IPrototype = actx.Avm.Prototypes["Array"];
                     ((ASArray) tv)._values = new List<Value>(args);
                     return Value.FromObject(tv);
                 }
                 , avm)), true, false, false),
            ["length"] = (avm) => PropertyDescriptor.A(
                (tv) => Value.FromInteger(((ASArray) tv).GetLength()),
                (tv, val) =>
                {
                    var len = val.ToInteger();
                    ((ASArray) tv).ChangeLength(len);
                }
                , false, false), 
            // methods
            // TODO
        };

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            
        };

        private List<Value> _values;
        public int GetLength() { return _values.Count; }
        public void ChangeLength(int len)
        {
            if (len < 0) throw new InvalidOperationException("Array length can not be less than 0.");
            else if (len == 0) _values.Clear();
            else if (len == _values.Count) return;
            else if (len < _values.Count)
            {
                while (len < _values.Count)
                    _values.RemoveAt(len);
            }
            else
            {
                while (len > _values.Count)
                    _values.Add(Value.Undefined());
            }
        }

        public ASArray(VirtualMachine vm) : this(null, vm) { }
        public ASArray(Value[] args, VirtualMachine vm): base(vm, "Array")
        {
            if (args == null) _values = new List<Value>();
            else _values = new List<Value>(args);
        }

        public Value[] GetValues()
        {
            Value[] ans = new Value[_values.Count];
            _values.CopyTo(0, ans, 0, _values.Count);
            return ans;
        }

        public Value GetValue(int index) { return _values[index]; }

        public override string ToString()
        {
            return $"[{String.Join(",", _values)}]";
        }
    }
}
