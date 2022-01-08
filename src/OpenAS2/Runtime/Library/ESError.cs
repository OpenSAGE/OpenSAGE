using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAS2.Runtime.Library
{
    public class WrappedESError: Exception
    {
        public ESError E { get; private set; }

        public WrappedESError(ESError e)
        {
            E = e;
        }
    }
    public class ESError : ESObject
    {
        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            // properties
        };

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {
            // ["prototype"] = (avm) => Property.D(Value.FromObject(avm.GetPrototype("Function")), true, false, false),
        };

        public ESError(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces): base(classIndicator, extensible, prototype, interfaces)
        {

        }

        public ESError(VirtualMachine vm, string? message) : base(vm, "Error")
        {
            if (message != null)
                IPut("message", Value.FromString(message));
        }

        public static Value IConstructAndCall(ExecutionContext ec, ESObject tv, IList<Value> args)
        {
            if (args.Count > 0 && !args.First().IsUndefined())
                return Value.FromObject(new ESError(ec.Avm, args.First().ToString()));
            else
                return Value.FromObject(new ESError(ec.Avm, null));
        }
    }
}
