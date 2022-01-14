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
            ["name"] = _ => PropertyDescriptor.D(Value.FromString("Error"), true, false, true),
            ["message"] = _ => PropertyDescriptor.D(Value.FromString(""), true, false, true),
            // message defined in .ctor
        };

        public static new Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };

        public static new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>()
        {

        };

        public static new Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

        };

        public static string[] NativeErrorList = new string[] { "EvalError", "RangeError", "ReferenceError", "SyntaxError", "URIError", "TypeError" };

        public ESError(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces): base(classIndicator, extensible, prototype, interfaces)
        {

        }

        public ESError(ExecutionContext ec, string? message, string? name = null) : base(ec.Avm, "Error")
        {
            if (name != null)
                IPut(ec, "name", Value.FromString(name));
            if (message != null)
                IPut(ec, "message", Value.FromString(message));
        }


        public static ESCallable.Result IConstructAndCall(ExecutionContext ec, ESObject tv, IList<Value> args, string? name)
        {
            if (args.Count > 0 && !args.First().IsUndefined())
                return ESCallable.Return(Value.FromObject(new ESError(ec, args.First().ToString(), name)));
            else
                return ESCallable.Return(Value.FromObject(new ESError(ec, null, name)));
        }

        public static ESCallable.Func IConstructAndCall(string? name) { return (a, b, c) => IConstructAndCall(a, b, c, name); }


        public override string ToString()
        {
            return base.ToString();
        }
    }
}
