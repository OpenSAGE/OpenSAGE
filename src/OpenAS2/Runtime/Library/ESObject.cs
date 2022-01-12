using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Runtime.Library;
using OpenAS2.Compilation.Syntax;

namespace OpenAS2.Runtime
{ 
    public class ESObject
    {
        // global definitions

        public VirtualMachine? VM { get; set; } // not really useful 

        // most general

        protected readonly Dictionary<string, PropertyDescriptor> _properties;

        public Dictionary<string, PropertyDescriptor>.KeyCollection GetAllProperties() { return _properties.Keys; }

        public virtual void ForceAddAccessoryProperty(ExecutionContext context, string name, ESFunction? getter, ESFunction? setter, bool enumerable = false, bool configurable = true)
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
        public ESObject(VirtualMachine vm, string? classIndicator, bool extensible = true) : this(classIndicator, extensible, null, null) // 3, 5, 6
        {
            VM = vm;
            // 4
            if (vm != null && vm.Prototypes.TryGetValue(classIndicator ?? "Object", out var proto)) 
                IPrototype = proto;
            else if (vm != null && vm.Prototypes.TryGetValue("Object", out var protoObj))
                IPrototype = protoObj;

            // 7, 8
        }

        // 262 v5.1 15.2.1
        // Do not simplify it due to the reflection
        public ESObject(VirtualMachine vm) : this(vm, null) { }

        // [[Construct]]
        public static ESCallable.Result IConstructObj(ExecutionContext ec, ESObject tv, IList<Value> args)
        {
            if (args.Count > 0)
            {
                var arg = args.First();
                if (arg.Type == ValueType.Object)
                {
                    var aobj = arg.ToObject();
                    if (aobj is StageObject sobj)
                    {
                        throw new NotImplementedException();
                    }
                    else
                        return ESCallable.Return(arg);
                }
                else if (arg.Type == ValueType.String || arg.Type == ValueType.Boolean || arg.IsNumber())
                    return ESCallable.Return(arg.ToObject(ec.Avm));
                else
                    return ESCallable.Return(Value.FromObject(new ESObject(ec.Avm)));
            }
            else
                return ESCallable.Return(Value.FromObject(new ESObject(ec.Avm)));
        }

        // [[Call]]
        public static ESCallable.Result ICallObj(ExecutionContext ec, ESObject tv, IList<Value> args)
        {
            if (args.Count == 0 && (args.First().IsNull() || args.First().IsUndefined()))
                return ESCallable.Return(Value.FromObject(new ESObject(ec.Avm)));
            else
                return ESCallable.Return(args.First().ToObject(ec.Avm));
        }

        // utility functions

        /// <summary>
        /// in case of NDP, if it is undefined return false
        /// in case of NAP do not care if it is or not
        /// </summary>
        /// <param name="name">the name to search</param>
        /// <param name="own">if it is an own property</param>
        /// <returns></returns>
        public PropertyDescriptor? UGetPropDesc(string name, out bool own, bool onlySearchOwn = false)
        {
            var ret = _properties.TryGetValue(name, out var prop) && !prop.IsUndefNDP();
            prop = ret ? prop : null;
            var obj = this;
            own = ret;
            while (!ret && !onlySearchOwn)
            {
                obj = obj.IPrototype;
                if (obj == null)
                    break;
                ret = obj._properties.TryGetValue(name, out prop) && !prop.IsUndefNDP();
                prop = ret ? prop : null;
            }
            return prop;
        }

        // internal functions 

        public void ASSetFlags(string name, int set, int clear)
        {
            var prop = UGetPropDesc(name, out var own);
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

        public ESCallable.Result IHasProperty(ExecutionContext ec, string name)
        {
            var prop = UGetPropDesc(name, out var own);
            if (prop == null)
                return ESCallable.Return(Value.FromBoolean(false));
            if (prop is NamedDataProperty)
                return ESCallable.Return(Value.FromBoolean(true));
            else
            {
                var pa = (NamedAccessoryProperty) prop!;
                var fres = pa.Get(ec, this, null);
                fres.SetRecallCode(x =>
                    ESCallable.Return(Value.FromBoolean( !(x.Type != ResultType.Return || x.Value.IsUndefined()) ))
                    );
                return fres;
            }
        }

        public ESCallable.Result IGet(ExecutionContext ec, string name)
        {
            var prop = UGetPropDesc(name, out var own);
            if (prop == null)
                return ESCallable.Return(Value.Undefined());
            if (prop is NamedDataProperty pd)
                return ESCallable.Return(pd.Value);
            else
            {
                var pa = (NamedAccessoryProperty) prop!;
                var fres = pa.Get(ec, this, null);
                return fres;
            }
        }



        public virtual Value IGet(string name)
        {
            return IGet(null, name).Value;
        }

        public virtual bool ICanPut(string name, out PropertyDescriptor? prop, ExecutionContext? ec = null)
        {
            // unnecessary
            // if (IHasOwnProperty(name, out var prop))
            //    return prop!.Writable;
            prop = UGetPropDesc(name, out var _);
            if (IPrototype != null && prop != null)
                return prop.Writable;
            else
                return IExtensible;
        }

        public virtual void IPut(string name, Value val, ExecutionContext? ec = null, bool doThrow = false)
        {
            ec = ec ?? (VM != null ? VM.GlobalContext : null);
            ESError? e = null;
            if (ICanPut(name, out var prop))
            {
                if (prop != null)
                {
                    if (prop is NamedAccessoryProperty pa)
                    {
                        var sres = pa.Set!(ec, this, new Value[] { val });
                        if (sres.Type == ResultType.Throw)
                            e = (ESError) sres.Value.ToObject();
                    }
                        
                    else
                    {
                        var pd = (NamedDataProperty) prop;
                        if (IHasOwnProperty(name, out var prop2)) // assume equivalent to containskey?
                            if (prop != prop2)
                                throw new InvalidOperationException("This should not happen");
                            else
                                pd.Value = val;
                    }
                }
                else // create a new property
                    IDefineOwnProperty(name, PropertyDescriptor.D(val, true, true, true), doThrow);
            }
            else if (doThrow) 
            {
                if (ec == null)
                    throw new WrappedESError(e);
                else
                    throw new NotImplementedException(); // don't know how or what to do...
            }
        }

        public virtual bool IDefineOwnProperty(string name, PropertyDescriptor desc, bool doThrow = false)
        {
            var ans = true;
            var fcurrent = IHasOwnProperty(name, out var pcurrent);
            if (!fcurrent)
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
                        throw new NotImplementedException(); // what the hell........implement aster met
                    }
                }
            }
            if (!ans && doThrow)
            {
                // TODO reject
            }
            return ans;
        }

        public virtual bool IDelete(string name, bool doThrow = false)
        {
            if (IHasOwnProperty(name, out var prop))
            {
                // not necessary?
                // if (prop is NamedDataProperty && ((NamedDataProperty) prop).Value.Type == ValueType.Undefined)
                //     return true;
                if (prop!.Configurable)
                {
                    _properties.Remove(name);
                    return true;
                }
                else
                {
                    if (doThrow)
                    {
                        // TODO reject
                    }
                    return false;
                }
                    
            }
            return true;
        }

        public virtual Value IDefaultValue(int hint = 0, ExecutionContext? actx = null)
        {
            if (hint == 1)
            {
                var ts = TryCall("toString", actx, null);
                if (Value.IsPrimitive(ts))
                    return ts;
                var vo = TryCall("valueOf", actx, null);
                if (Value.IsPrimitive(vo))
                    return vo;
                // todo throw exception inside vm?
                throw new NotImplementedException("TypeError Exception");
            }
            else if (hint == 2)
            {
                var vo = TryCall("valueOf", actx, null);
                if (Value.IsPrimitive(vo))
                    return vo;
                var ts = TryCall("toString", actx, null);
                if (Value.IsPrimitive(ts))
                    return ts;
                // todo throw exception inside vm?
                throw new NotImplementedException("TypeError Exception");
            }
            else
            {
                // TODO date object
                return IDefaultValue(2, actx);
            }
        }

        // utils
        public Value TryCall(string fname, ExecutionContext? ec, IList<Value>? args)
        {
            // TODO what if ec is null?
            var f = IGet(fname);
            if (!f.IsCallable())
            {
                // TODO throw
                throw new NotImplementedException("TypeError");
            }
            else
                return f.ToFunction().ICall(ec, this, args);
        }

        public static PropertyDescriptor AddFunction(ESCallable.Func f, VirtualMachine vm, bool w = true, bool e = false, bool c = true)
        {
            return PropertyDescriptor.D(Value.FromFunction(new NativeFunction(vm, f)), w, e, c);
        }

        // library declaration
        // under and only under prototype
        // otherwise it should be the task of [[Construct]]
        public static Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>() // __proto__
        {
            // this one actually should not be defined in property list
            // still, I write this function for convenience
            ["__proto__"] = (avm) => PropertyDescriptor.A(
                 (tv) => tv.IPrototype == null ? Value.Null() : Value.FromObject(tv.IPrototype),
                 (tv, val) => tv.IPrototype = val.ToObject(),
                 false, false),

            // methods
            ["toString"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) =>
                {
                    var ans = tv == null ? "[object Undefined]" : tv.ToString();
                    return Value.FromString(ans);
                }
                , avm)), true, false, true),
            ["toLocaleString"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) => tv.TryCall("toString", actx, args)
                , avm)), true, false, true),
            ["valueOf"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) =>
                {
                    if (tv is StageObject)
                        throw new NotImplementedException();
                    else
                        return Value.FromObject(tv);
                }
                , avm)), true, false, true),

            ["hasOwnProperty"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) => {
                    if (args == null || args.Count == 0)
                        return Value.FromBoolean(false);
                    var ans = tv.IHasOwnProperty(args[0].ToString());
                    return Value.FromBoolean(ans);
                }
                , avm)), true, false, true),
            ["isPrototypeOf"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) => {
                    if (args == null || args.Count == 0 || args[0].Type != ValueType.Object)
                        return Value.FromBoolean(false);
                    var theClass = args[0].ToObject()!;
                    var ans = tv.IsPrototypeOf(theClass);
                    return Value.FromBoolean(ans);
                }
                , avm)), true, false, true),

            ["propertyIsEnumerable"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     if (args == null || args.Count == 0)
                         return Value.FromBoolean(false);
                     var name = args[0].ToString();
                     var ans = tv.IsPropertyEnumerable(name);
                     return Value.FromBoolean(ans);
                 }
                 , avm)), true, false, true),
            ["isPropertyEnumerable"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) => tv.TryCall("propertyIsEnumerable", actx, args)
                , avm)), true, false, true),

            ["addProperty"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) =>
                 {
                     string name = args.Count > 0 ? args[0].ToString() : null;
                     ESFunction getter = args.Count > 1 ? args[1].ToFunction() : null;
                     ESFunction setter = args.Count > 2 ? args[2].ToFunction() : null;
                     tv.ForceAddAccessoryProperty(actx, name, getter, setter);
                     return null;
                 }
                 , avm)), true, false, true),

            


        };

        public static Dictionary<string, Func<VirtualMachine, PropertyDescriptor>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>() // constructor
        {
            ["getPrototypeOf"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) =>
                    (args != null && args.Count > 0 && args[0].Type == ValueType.Object) ?
                    Value.FromObject(args[0].ToObject()!.IPrototype) :
                    throw new NotImplementedException()
                , avm)), true, false, true),
            ["getOwnPropertyDescriptor"] = (avm) => PropertyDescriptor.D(Value.FromFunction(new NativeFunction(
                (actx, tv, args) =>
                {
                    var ans = tv == null ? "[object Undefined]" : tv.ToString();
                    return Value.FromString(ans);
                }
                , avm)), true, false, true),
        };



        // just for convenience

        public ESObject __proto__
        {
            get { return IGet("__proto__").ToObject(); }
            set { IPut("__proto__", Value.FromObject(value)); }
        }
        public ESObject prototype
        {
            get { return IGet("prototype").ToObject(); }
            set { IPut("prototype", Value.FromObject(value)); }
        }
        public ESFunction constructor
        {
            get { return IGet("constructor").ToFunction(); }
            set { IPut("constructor", Value.FromFunction(value)); }
        }



        // standard library

        public static bool EqualsES(ESObject x, ESObject y, ExecutionContext? actx = null)
        {
            return object.Equals(x, y); // TODO
        }

        public virtual bool IsPropertyEnumerable(string name)
        {
            var thisVar = this;
            while (thisVar != null)
            {
                if (thisVar.IHasOwnProperty(name))
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

        public bool IsFunction() { return this is ESFunction; }
        public bool IsConstructor() { return IsFunction() && prototype.IsPrototype(); }
        public bool IsPrototype() { return constructor != null && constructor.IsFunction(); }

        public bool InstanceOf(ESObject cst) // TODO Not complete
        {
            if (!cst.IsFunction()) return false; // not even a constructor
            var tproto = __proto__;
            var cproto = cst.prototype;
            while (tproto != null)
            {
                if (cproto == tproto) return true;
                tproto = tproto.__proto__;
            }
            return false;
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


        public string ToStringDisp(ExecutionContext actx)
        {
            string ans = "{\n";
            foreach (string s in _properties.Keys)
            {
                _properties.TryGetValue(s, out var v);
                ans = ans + s + ": " + v.ToString(actx) + ", \n";
            }
            ans = ans + "}";
            return ans;
        }

        public (string[], string[]) ToListDisp(ExecutionContext actx)
        {
            var ans1 = new string[_properties.Keys.Count];
            var ans2 = GetAllProperties().ToArray();
            for (int i = 0; i < _properties.Count; ++i)
            {
                var k = ans2[i];
                _properties.TryGetValue(k, out var v);
                ans1[i] = $"{k}: {v.ToString(actx)}";
                ans2[i] = k;
            }
            return (ans1, ans2);
        }
    }
}
