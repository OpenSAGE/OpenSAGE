using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Runtime.Library;
using OpenAS2.Compilation.Syntax;
using OpenAS2.Runtime.Dom;

namespace OpenAS2.Runtime
{
    public class ESObject
    {
        // global definitions

        public VirtualMachine? VM { get; set; } // not really useful 

        // most general

        protected readonly Dictionary<string, PropertyDescriptor> _properties;

        public Dictionary<string, PropertyDescriptor>.KeyCollection GetAllProperties() { return _properties.Keys; }


        public virtual void ForceAddDataProperty(string name, Value v, bool writable = true, bool enumerable = false, bool configurable = true)
        {
            var prop = PropertyDescriptor.D(v, writable, enumerable, configurable);
            _properties[name] = prop;
        }
        public virtual void ForceAddAccessoryProperty(string name, ESFunction? getter, ESFunction? setter, bool enumerable = false, bool configurable = true)
        {
            var prop = PropertyDescriptor.A(getter, setter, enumerable, configurable);
            _properties[name] = prop;
        }

        // internal properties

        // required

        public ESObject? IPrototype { get; protected set; }
        public string IClass { get; protected set; }
        public bool IExtensible { get; protected set; }

        // elective 

        public virtual Value IPrimitiveValue => throw new InvalidOperationException();

        // accessory 

        protected readonly List<ESObject> _interfaces;

        public virtual string ITypeOfResult => "object";


        // constructor

        // base constructor
        public ESObject(string? classIndicator, bool extensible, ESObject? prototype, IEnumerable<ESObject>? interfaces)
        {
            IClass = classIndicator ?? "Object";
            IExtensible = extensible;
            IPrototype = prototype;
            _interfaces = interfaces != null ? new(interfaces) : new();

            //Actionscript variables are not case sensitive!
            _properties = new Dictionary<string, PropertyDescriptor>(StringComparer.OrdinalIgnoreCase);
        }

        // a more convenient constructor
        public ESObject(VirtualMachine vm, string? classIndicator, string? protoIndicator = null, bool extensible = true) : this(classIndicator, extensible, null, null) // 3, 5, 6
        {
            VM = vm;
            classIndicator = classIndicator ?? "Object";
            protoIndicator = protoIndicator ?? "Object";
            // 4
            if (vm != null && vm.Prototypes.TryGetValue(classIndicator, out var proto))
                IPrototype = proto;
            else if (vm != null && vm.Prototypes.TryGetValue(protoIndicator, out proto))
                IPrototype = proto;
            else if (vm != null && vm.Prototypes.TryGetValue("Object", out proto))
                IPrototype = proto;

            // 7, 8
        }

        // 262 v5.1 15.2.1
        // Do not simplify it due to the reflection
        public ESObject(VirtualMachine vm) : this(vm, null) { }

        // [[Construct]]
        public static ESCallable.Result IConstructObj(ExecutionContext ec, ESObject tv, IList<Value>? args)
        {
            if (HasArgs(args))
            {
                var arg = args!.First();
                if (arg.Type == ValueType.Object)
                {
                    var aobj = arg.ToObject();
                    if (aobj is HostObject sobj)
                    {
                        throw new NotImplementedException();
                    }
                    else
                        return ESCallable.Return(arg);
                }
                else if (arg.Type == ValueType.String || arg.Type == ValueType.Boolean || arg.IsNumber())
                    return arg.ToObject(ec);
                else
                    return ESCallable.Return(Value.FromObject(new ESObject(ec.Avm)));
            }
            else
                return ESCallable.Return(Value.FromObject(new ESObject(ec.Avm)));
        }

        // [[Call]]
        public static ESCallable.Result ICallObj(ExecutionContext ec, ESObject tv, IList<Value>? args)
        {
            if (!HasArgs(args) && (args!.First().IsNull() || args!.First().IsUndefined()))
                return ESCallable.Return(Value.FromObject(new ESObject(ec.Avm)));
            else
                return args!.First().ToObject(ec);
        }

        // utility functions
        // internal functions 

        /// <summary>
        /// returns null as undefined, otherwise return the property
        /// </summary>
        /// <param name="name">the name to search</param>
        /// <param name="own">if it is an own property</param>
        /// <returns></returns>
        public PropertyDescriptor? IGetProperty(string name, out bool own, bool onlySearchOwn = false)
        {
            var ret = _properties.TryGetValue(name, out var prop); // && !prop.IsUndefNDP();
            prop = ret ? prop : null;
            var obj = this;
            own = ret;
            while (!ret && !onlySearchOwn)
            {
                obj = obj.IPrototype;
                if (obj == null)
                    break;
                ret = obj._properties.TryGetValue(name, out prop); // && !prop.IsUndefNDP();
                prop = ret ? prop : null;
            }
            return prop;
        }

        public bool DeleteOwnProperty(string name, bool forceDelete = false)
        {
            if (_properties.TryGetValue(name, out var prop) && (prop.Configurable || forceDelete))
            {
                _properties.Remove(name);
                return true;
            }
            return false;
        }

        public void ASSetFlags(string name, int set, int clear)
        {
            var prop = IGetProperty(name, out var own);
            if (prop == null || !own) return;
            var swp = (set & 4) > 0;
            var sdp = (set & 2) > 0;
            var shid = (set & 1) > 0;
            var cwp = (clear & 4) > 0;
            var cdp = (clear & 2) > 0;
            var chid = (clear & 1) > 0;
            if (swp) prop!.Writable = false;
            if (sdp) prop!.Configurable = false;
            if (shid) prop!.Enumerable = false;
            if (cwp) prop!.Writable = true;
            if (cdp) prop!.Configurable = true;
            if (chid) prop!.Enumerable = true;
        }

        public virtual ESCallable.Result IGet(ExecutionContext ec, string name)
        {
            var prop = IGetProperty(name, out var own);
            if (prop == null)
                return ESCallable.Return(Value.Undefined());
            if (prop is NamedDataProperty pd)
                return ESCallable.Return(pd.Value);
            else
            {
                var pa = (NamedAccessoryProperty)prop!;
                var fres = pa.Get(ec, this, null);
                return fres;
            }
        }

        public bool ICanPut(string name) { return ICanPut(name, out var _, out var _); }
        public virtual bool ICanPut(string name, out PropertyDescriptor? prop, out bool own)
        {
            prop = IGetProperty(name, out own);
            if (prop is NamedAccessoryProperty pa)
                return pa.Set != null;
            else if (prop != null)
            {
                var pd = (NamedDataProperty)prop!;
                if (own)
                    return pd.Writable;
                else
                    return pd.Writable && IExtensible;
            }
            return IExtensible;
        }

        public virtual ESCallable.Result IPut(ExecutionContext ec, string name, Value val, bool doThrow = false)
        {
            if (ICanPut(name, out var prop, out var own))
            {
                if (own && prop is NamedDataProperty pd)
                {
                    pd.Value = val;
                    // does it need to call DOP?
                }
                else if (prop is NamedAccessoryProperty pa)
                {
                    var sres = pa.Set!(ec, this, new Value[] { val });
                    return sres;
                }
                else // create a new property
                    return IDefineOwnProperty(ec, name, PropertyDescriptor.D(val, true, true, true), doThrow);
            }
            else if (doThrow)
            {
                var err = ec.ConstrutError("TypeError");
                return ESCallable.Throw(err);
            }
            return new ESCallable.Result();
        }

        public virtual bool IHasProperty(string name)
        {
            var prop = IGetProperty(name, out var _);
            return prop == null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="ec"></param>
        /// <param name="hint">1: string 2: number</param>
        /// <returns></returns>
        public virtual ESCallable.Result IDefaultValue(ExecutionContext ec, int hint = 1)
        {
            if (hint != 1 && hint != 2)
                return IDefaultValue(ec, 1);
            var f1 = hint == 1 ? "toString" : "valueOf";
            var f2 = hint == 1 ? "valueOf" : "toString";
            var ts = TryCall(ec, f1, null);
            ts.AddRecallCode(res =>
            {
                if (res.IsNormalOrReturn())
                {
                    var prim = Value.IsPrimitive(res.Value);
                    if (prim)
                        return res;
                    else
                    {
                        var vo = TryCall(ec, f2, null);
                        vo.AddRecallCode(res =>
                        {
                            if (res.IsNormalOrReturn())
                                if (Value.IsPrimitive(res.Value))
                                    return res;
                                else
                                    return ESCallable.Throw(ec.ConstrutError("TypeError"));
                            else
                                return res;
                        });
                        return vo;
                    }
                }
                else
                    return res;
            });
            return ts;
        }


        public virtual ESCallable.Result IDefineOwnProperty(ExecutionContext? ec, string name, PropertyDescriptor desc, bool doThrow = false)
        {
            var ans = true;
            var pcurrent = IGetProperty(name, out var own, true);
            if (pcurrent == null)
                if (!IExtensible)
                    ans = false;
                else
                    _properties[name] = desc;
            else
            {
                PropertyDescriptor current = pcurrent!;
                if (!current.Configurable && (
                     (desc.Configurable && desc.HasConfigurable) ||
                     (desc.Enumerable != current.Enumerable && desc.HasEnumerable) ||
                     (desc is NamedDataProperty != current is NamedDataProperty)
                    ))
                    ans = false;
                else
                {
                    if (desc is NamedDataProperty != current is NamedDataProperty)
                    {
                        desc.Enumerable = desc.HasEnumerable ? desc.Enumerable : current.Enumerable;
                        desc.Configurable = desc.HasConfigurable ? desc.Configurable : current.Configurable;
                        _properties[name] = desc;
                    }
                    else
                    {
                        if (desc is NamedDataProperty dd && current is NamedDataProperty cd)
                        {
                            if (!cd.Configurable)
                                if (!cd.Writable)
                                    if (desc.Writable)
                                        ans = false;
                                    else
                                        ans = Value.SameValue(dd.Value, cd.Value);
                                else
                                    _properties[name] = dd; // TODO not sure
                            else
                                _properties[name] = dd;
                        }
                        else if (desc is NamedAccessoryProperty da && current is NamedAccessoryProperty ca)
                        {
                            if (!ca.Configurable && (da.Get != ca.Get || da.Set != ca.Set))
                                ans = false;
                            else
                                _properties[name] = da;
                        }
                        else
                            throw new InvalidOperationException(); // what the hell........
                    }
                }
            }
            if (!ans && doThrow && ec != null)
            {
                var err = ec.ConstrutError("TypeError");
                return ESCallable.Throw(err);
            }
            return ESCallable.Return(Value.FromBoolean(ans));
        }

        public virtual ESCallable.Result IDeleteValue(ExecutionContext ec, string name, bool doThrow = false)
        {
            var prop = IGetProperty(name, out var _, true);
            var ans = true;
            if (prop != null)
            {
                if (prop.Configurable)
                    _properties.Remove(name);
                else
                {
                    ans = false;
                }

            }
            if (!ans && doThrow)
            {
                var err = ec.ConstrutError("TypeError");
                return ESCallable.Throw(err);
            }
            return ESCallable.Return(Value.FromBoolean(ans));
        }



        // utils
        public ESCallable.Result TryCall(ExecutionContext ec, string fname, IList<Value>? args)
        {
            var call = IGet(ec, fname);
            call.AddRecallCode(res =>
            {
                var f = res.Value;
                if (!f.IsCallable())
                    return ESCallable.Throw(ec.ConstrutError("TypeError"));
                else
                    return f.ToFunction().ICall(ec, this, args);
            });
            return call;
        }

        public ESCallable.Result ResolveValue(ExecutionContext ec, string value)
        {
            var path = value.Split('.');
            var obj = this;
            var member = path.Last();

            ESCallable.Result? ans = null;

            for (var i = 0; i < path.Length - 1; i++)
            {
                var fragment = path[i];

                if (obj.IHasProperty(fragment))
                {
                    if (ans == null)
                        ans = obj.IGet(ec, fragment);
                    else
                    {
                        ans .AddRecallCode((res) => res.Value.ToObject(ec))
                            .AddRecallCode((res) => res.Value.ToObject().IGet(ec, fragment));
                    }
                }
                else
                {
                    var err = ESCallable.Throw(ec.ConstrutError("Error", "Undefined proprety"));
                    if (ans != null)
                        ans.AddRecallCode(_ => err);
                    else
                        ans = err;
                    break;
                }
            }
            if (ans == null)
                ans = obj.IGet(ec, member);
            else
                ans.AddRecallCode((res) => res.Value.ToObject().IGet(ec, member));
            return ans;
        }

        public static PropertyDescriptor AddFunction(ESCallable.Func f, VirtualMachine vm, bool w = true, bool e = false, bool c = true)
        {
            return PropertyDescriptor.D(Value.FromFunction(new NativeFunction(vm, f)), w, e, c);
        }

        public void DefineAllProperties(ExecutionContext? ec, Dictionary<string, Func<PropertyDescriptor>> props, bool doThrow = false)
        {
            foreach (var (k, v) in props)
                IDefineOwnProperty(ec, k, v(), doThrow);
        }

        public void DefineAllMethods(ExecutionContext? ec, VirtualMachine vm, Dictionary<string, ESCallable.Func> props)
        {
            foreach (var (k, v) in props)
                IDefineOwnProperty(ec, k, PropertyDescriptor.D(Value.FromFunction(new NativeFunction(vm, v)), true, false, true));
        }


        // library declaration
        // under and only under prototype
        // otherwise it should be the task of [[Construct]]
        public static Dictionary<string, Func<PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>() // __proto__
        {
            // this one actually should not be defined in property list
            // still, I write this function for convenience
            ["__proto__"] = () => PropertyDescriptor.A(
                 (ec, tv, args) => ESCallable.Return(tv.IPrototype == null ? Value.Null() : Value.FromObject(tv.IPrototype)),
                 (ec, tv, args) =>
                 {
                     var flag = args != null && args.Count > 0;
                     if (flag)
                         tv.IPrototype = args![0].ToObject();
                     return new ESCallable.Result();
                 },
                 false, false),
        };

        public static bool HasArgs(IList<Value>? args, uint leastCount = 0) { return args != null && args.Count >= leastCount; }

        public static Dictionary<string, ESCallable.Func> MethodsDefined = new Dictionary<string, ESCallable.Func>()
        {

            // methods
            ["toString"] = (ec, tv, args) =>
                {
                    var ans = tv == null ? "[object Undefined]" : tv.ToString();
                    return ESCallable.Return(Value.FromString(ans));
                },
            ["toLocaleString"] =
                (ec, tv, args) => tv.TryCall(ec, "toString", args),
            ["valueOf"] =
                (ec, tv, args) =>
                {
                    if (tv is HostObject)
                        throw new NotImplementedException();
                    else
                        return ESCallable.Return(Value.FromObject(tv));
                },

            ["hasOwnProperty"] =
                (ec, tv, args) =>
                {
                    var ans = false;
                    if (HasArgs(args))
                        ans = tv.IGetProperty(args![0].ToString(), out var _, onlySearchOwn: true) != null;
                    return ESCallable.Return(Value.FromBoolean(ans));
                },
            ["isPrototypeOf"] =
                (ec, tv, args) =>
                {
                    var ans = false;
                    if (HasArgs(args) && args![0].Type == ValueType.Object)
                        tv.IsPrototypeOf(args[0].ToObject());
                    return ESCallable.Return(Value.FromBoolean(ans));
                },
            ["propertyIsEnumerable"] =
                 (ec, tv, args) =>
                 {
                     var ans = false;
                     if (HasArgs(args))
                         ans = tv.IsPropertyEnumerable(args![0].ToString());
                     return ESCallable.Return(Value.FromBoolean(ans));
                 },
            ["isPropertyEnumerable"] =
                (ec, tv, args) => tv.TryCall(ec, "propertyIsEnumerable", args),

            ["addProperty"] =
                 (ec, tv, args) =>
                 {
                     var ans = false;
                     if (HasArgs(args))
                     {
                         var name = args![0].ToString();
                         var getter = args.Count > 1 ? args[1] : null;
                         var setter = args.Count > 2 ? args[2] : null;
                         ans = !string.IsNullOrWhiteSpace(name) && getter != null && (setter == null || setter.IsCallable());
                         if (ans)
                             tv.ForceAddAccessoryProperty(name, getter!.ToFunction(), setter != null ? setter.ToFunction() : null);
                     }
                     return ESCallable.Return(Value.FromBoolean(ans));
                 },




        };

        public static Dictionary<string, Func<PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<PropertyDescriptor>>()
        {
        };

        public static Dictionary<string, ESCallable.Func> StaticMethodsDefined = new Dictionary<string, ESCallable.Func>()
        {
            ["getPrototypeOf"] =
                (ec, tv, args) =>
                {
                    if (args != null && args.Count > 0 && args[0].Type == ValueType.Object)
                        return ESCallable.Return(Value.FromObject(args[0].ToObject()!.IPrototype));
                    return ESCallable.Throw(ec.ConstrutError("TypeError"));
                },
            ["getOwnPropertyDescriptor"] =
                (ec, tv, args) =>
                {
                    if (args != null && args.Count > 0 && args[0].Type == ValueType.Object)
                    {
                        var obj = args[0].ToObject();
                        if (args.Count > 1)
                        {
                            var name = args[1].ToString();
                            var prop = obj.IGetProperty(name, out var _, onlySearchOwn: true);
                            var ans = prop == null ? Value.Undefined() : Value.FromObject(PropertyDescriptor.IFormPropertyDescriptor(ec, prop!));
                            return ESCallable.Return(ans);
                        }
                        return ESCallable.Return(Value.Undefined());
                    }
                    return ESCallable.Throw(ec.ConstrutError("TypeError"));
                },


        };


        // standard library

        public static bool EqualsES(ESObject x, ESObject y, ExecutionContext? ec = null)
        {
            return object.Equals(x, y); // TODO
        }

        public virtual bool IsPropertyEnumerable(string name)
        {
            var thisVar = this;
            while (thisVar != null)
            {
                if (thisVar.IGetProperty(name, out var _, onlySearchOwn: true) != null)
                {
                    return thisVar._properties[name].Enumerable;
                }
                thisVar = thisVar.IPrototype;
            }
            return false;
        }


        // judgements

        public bool IsPrototypeOf(ESObject theClass)
        {
            var ans = false;
            var proto = theClass.IPrototype;
            while (proto != null)
            {
                if (proto == this)
                {
                    ans = true;
                    break;
                }
                proto = proto.IPrototype;
            }
            return ans;
        }

        public ESCallable.Result InstanceOf(ExecutionContext ec, ESObject cst) // TODO Not complete
        {
            if (cst is ESFunction esf)
            {
                var tproto = IPrototype;
                var cproto2 = esf.IGet(ec, "prototype");
                cproto2.AddRecallCode(res =>
                {
                    var ret = false;
                    if (res.Value.Type == ValueType.Object)
                    {
                        var cproto = res.Value.ToObject();
                        while (tproto != null)
                        {
                            if (cproto == tproto) ret = true;
                            tproto = tproto.IPrototype;
                        }
                    }
                    return ESCallable.Return(Value.FromBoolean(ret));
                });
                return cproto2;
            }
            return ESCallable.Throw(ec.ConstrutError("TypeError"));
        }

        // conversion & debug

        public override string ToString()
        {
            return $"[object {IClass}]";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hint">0 = none, 1 = string, 2 = number</param>
        /// <returns></returns>


        public string ToStringDisp()
        {
            string ans = "{\n";
            foreach (string s in _properties.Keys)
            {
                _properties.TryGetValue(s, out var v);
                ans = ans + s + ": " + v!.ToString() + ", \n";
            }
            ans = ans + "}";
            return ans;
        }

        public (string[], string[]) ToListDisp()
        {
            var ans1 = new string[_properties.Keys.Count];
            var ans2 = GetAllProperties().ToArray();
            for (int i = 0; i < _properties.Count; ++i)
            {
                var k = ans2[i];
                _properties.TryGetValue(k, out var v);
                ans1[i] = $"{k}: {v!}";
                ans2[i] = k;
            }
            return (ans1, ans2);
        }
    }
}
