using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAS2.Runtime.Library
{
    public enum ResultType
    {
        Executing = 1,
        Normal = 2,
        Return = 4,
        Throw = 8
    }
    public class ESCallable
    {
        public delegate Result Func(ExecutionContext context, ESObject thisVar, IList<Value>? args);

        public class Result
        {
            public static readonly ExecutionContext NativeContext = new(null, null, null, null, null, null,
                numRegisters: 0,
                displayName: "[Native Code]");

            /// <summary>
            /// the code is guarenteed to be called either res.Type == Normal or Return.
            /// </summary>
            /// <param name="res"></param>
            /// <returns></returns>
            public delegate Result? RecallCode(Result res);

            private readonly ResultType _type;
            private readonly ExecutionContext? _context;
            private List<RecallCode> _recalls = new();
            private readonly Value? _value;

            public ResultType Type { get { return _context == null ? _type : _context.Result; } }
            public Value Value { get { return (_context == null ? _value : (_context.ReturnValue)) ?? Value.Undefined(); } }
            public ExecutionContext Context => _context ?? NativeContext;

            public Result(ExecutionContext ec, RecallCode? recall = null)
            {
                _context = ec;
                _recalls = new();
                if (recall != null) AddRecallCode(recall);
                _type = ResultType.Executing;
            } // special type to definedfunction
            public Result() { _type = ResultType.Normal; } // normal, empty, empty

            public Result(ResultType t, Value? v) { _type = t; _value = v; }

            public Result AddRecallCode(RecallCode rc) { _recalls.Add(rc); return this; }

            /// <summary>
            /// if push nothing back, create a function to return null or a Result with Value == null; elsewhere something will be pushed;
            /// if executing, the context must be on the stack, continue running it
            /// </summary>
            /// <returns></returns>
            public Result? ExecuteRecallCode()
            {
                Result? res = this;
                LinkedList<RecallCode> ltmp = new(res._recalls);
                while (ltmp.Count > 0)
                {
                    var rc = ltmp.First();
                    ltmp.RemoveFirst();
                    res = rc(res!);
                    if (res == null) // push nothing to stack
                        break;
                    else if (res.Type == ResultType.Throw) // exception is thrown, return it
                        break;
                    else if (res.Type == ResultType.Executing) // requires further execution
                    {
                        res._recalls.AddRange(ltmp);
                        break;
                    }
                    else if (res._recalls.Count > 0) // more recall codes
                        foreach (var f in Enumerable.Reverse(res._recalls))
                            ltmp.AddFirst(f);
                    // else do nothing but continuing execution
                }
                return res;
            }

            // judgement

            public bool IsNormalOrReturn() { return Type == ResultType.Normal || Type == ResultType.Return; }

            // some recall codes

            public static RecallCode RCBoolCheck(Func<Value, bool> f)
            {
                return res =>
                {
                    if (res.IsNormalOrReturn())
                        return Return(Value.FromBoolean(f(res.Value)));
                    else
                        return res;
                };
            }
        }

        public static Result Normal(Value? v) { return new(ResultType.Normal, v); }
        public static Result Return(Value v) { return new(ResultType.Return, v); }
        public static Result Throw(Value v) { return new(ResultType.Throw, v); }
        public static Result Throw(ESError e) { return new(ResultType.Throw, Value.FromObject(e)); }
    }



    // ECMA-262 v5.1 #8.3.1
    public class PropertyDescriptor
    {

        public bool Enumerable { get; set; }
        public bool Configurable { get; set; }
        public virtual bool Writable { get; set; }

        public readonly bool HasEnumerable = true;
        public readonly bool HasConfigurable = true;

        public static NamedDataProperty D(Value val, bool w, bool e, bool c)
        {
            return new NamedDataProperty(val, w, e, c);
        }
        public static NamedAccessoryProperty A(ESCallable.Func? g, ESCallable.Func? s, bool e, bool c)
        {
            return new NamedAccessoryProperty(g, s, e, c);
        }
        public static NamedAccessoryProperty A(ESFunction? g, ESFunction? s, bool e, bool c)
        {
            return new NamedAccessoryProperty(g, s, e, c);
        }

        public static PropertyDescriptor Copy(PropertyDescriptor p)
        {
            if (p is NamedDataProperty dd)
            {
                return new NamedDataProperty(dd.Value, dd.Writable, dd.Enumerable, dd.Configurable);
            }
            else
            {
                var da = (NamedAccessoryProperty)p;
                return new NamedAccessoryProperty(da.Get, da.Set, da.Enumerable, da.Configurable);
            }
        }
        public virtual string? ToString(ExecutionContext ec)
        {
            return ToString();
        }


        // utilities

        public bool IsUndefNDP() { return this is NamedDataProperty d && d.Value.IsUndefined(); }


        // static methods

        public static bool IIsAccessorDescriptor(PropertyDescriptor desc) { return desc is NamedAccessoryProperty; }
        public static bool IIsDataDescriptor(PropertyDescriptor desc) { return desc is NamedDataProperty; }
        public static bool IIsGenericDescriptor(PropertyDescriptor desc) { return !(desc is NamedDataProperty) && !(desc is NamedAccessoryProperty); }

        public static ESObject IFormPropertyDescriptor(ExecutionContext ec, PropertyDescriptor desc)
        {
            var ret = new ESObject(ec.Avm);
            if (desc is NamedDataProperty dd)
            {
                ret.IDefineOwnProperty(ec, "value", D(dd.Value, true, true, true), false);
                ret.IDefineOwnProperty(ec, "writable", D(Value.FromBoolean(dd.Writable), true, true, true), false);
            }
            else
            {
                var da = (NamedAccessoryProperty)desc;
                ret.IDefineOwnProperty(ec, "get", D(Value.FromFunction(new NativeFunction(ec.Avm, da.Get)), true, true, true), false);
                ret.IDefineOwnProperty(ec, "set", D(Value.FromFunction(new NativeFunction(ec.Avm, da.Set)), true, true, true), false);
            }
            ret.IDefineOwnProperty(ec, "enumerable", D(Value.FromBoolean(desc.Enumerable), true, true, true), false);
            ret.IDefineOwnProperty(ec, "configurable", D(Value.FromBoolean(desc.Configurable), true, true, true), false);
            return ret;
        }
        public static PropertyDescriptor IToPropertyDescriptor(ESObject obj)
        {
            throw new NotImplementedException();
        }

    }
    public class NamedDataProperty : PropertyDescriptor
    {
        public Value Value { get; set; }
        public NamedDataProperty(Value? val, bool w = false, bool e = false, bool c = false)
        {
            Value = val ?? Value.Undefined();
            Writable = w;
            Enumerable = e;
            Configurable = c;
        }

        public override string ToString()
        {
            return $"NDP({Value.ToStringWithType()} (W={Writable}, E={Enumerable}, C={Configurable})";
        }
    }
    public class NamedAccessoryProperty : PropertyDescriptor
    {
        public static readonly Func<ESObject, Value> DefaultPropertyGet = x => Value.Undefined();
        public ESCallable.Func Get { get; set; }

        private ESCallable.Func? _set;
        private bool _writable;
        public ESCallable.Func? Set
        {
            get { return _writable ? _set : null; }
            set { _set = value; }
        }
        public override bool Writable
        {
            get { return _writable && _set != null; }
            set { _writable = value; }
        }

        private ESFunction? _getter;
        private ESFunction? _setter;

        public NamedAccessoryProperty(ESCallable.Func? g = null, ESCallable.Func? s = null, bool e = false, bool c = false)
        {
            Get = g ?? FunctionUtils.ReturnUndefined;
            Set = s;
            Enumerable = e;
            Configurable = c;
            Writable = true;
        }

        public NamedAccessoryProperty(ESFunction? g = null, ESFunction? s = null, bool e = false, bool c = false)
        {
            var gc = g != null ? g.ICall : null;
            var sc = s != null ? s.ICall : null;
            Get = gc ?? FunctionUtils.ReturnUndefined;
            Set = sc;
            Enumerable = e;
            Configurable = c;
            Writable = true;
            _getter = g;
            _setter = s;
        }

        public (ESFunction?, ESFunction?) ToFunctions(VirtualMachine? vm = null)
        {
            var r1 = _getter ?? ((Get != null && vm != null) ? _getter = new NativeFunction(vm, Get) : null);
            var r2 = _setter ?? ((Set != null && vm != null) ? _setter = new NativeFunction(vm, Set) : null);
            return (r1, r2);
        }

        public override string ToString()
        {
            return $"NAP(Get: {Get}, Set: {Set}, E={Enumerable}, C={Configurable})";
        }
    }


}
