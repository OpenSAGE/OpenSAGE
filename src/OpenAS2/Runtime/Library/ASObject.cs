using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Runtime.Library;
using OpenAS2.Runtime.Opcodes;

namespace OpenAS2.Runtime
{
    // ECMA-262 #8.3.1
    public class Property
    {
        public bool Enumerable { get; set; }
        public bool Configurable { get; set; }
        public bool Writable { get; set; }

        public static NamedDataProperty D(Value val, bool w, bool e, bool c) {
            return new NamedDataProperty()
            {
                Value = val,
                Writable = w,
                Enumerable = e,
                Configurable = c,
            };
        }
        public static NamedAccessoryProperty A(Func<ASObject, Value> g, Action<ASObject, Value> s, bool e, bool c)
        {
            return new NamedAccessoryProperty()
            {
                Get = g,
                Set = s,
                Enumerable = e,
                Configurable = c,
                Writable = true,
            };
        }

        public virtual string ToString(ExecutionContext actx)
        {
            return base.ToString();
        }
    }
    public class NamedDataProperty: Property
    {
        public Value Value { get; set; }
        public override string ToString(ExecutionContext actx)
        {
            return Value.ToStringWithType(actx);
        }
    }
    public class NamedAccessoryProperty: Property
    {
        public Func<ASObject, Value> Get { get; set; }
        public Action<ASObject, Value> Set { get; set; }

        public override string ToString(ExecutionContext actx)
        {
            return $"NAP(Get: {Get}, Set: {Set})";
        }
    }

    public class ASObject
    {

        public ASObject __proto__
        {
            get { return GetMember("__proto__").ToObject(); }
            set { SetMember("__proto__", Value.FromObject(value)); }
        }
        public ASObject prototype
        {
            get { return GetMember("prototype").ToObject(); }
            set { SetMember("prototype", Value.FromObject(value)); }
        }
        public ASFunction constructor
        {
            get { return GetMember("constructor").ToFunction(); }
            set { SetMember("constructor", Value.FromFunction(value)); }
        }


        /// <summary>
        /// Contains functions and member variables
        /// </summary>
        /// 
        protected Dictionary<string, Property> _properties;

        // internal properties

        public ASObject PrototypeInternal { get; protected set; }
        public string Class;
        public bool Extensible;

        // prototype declaration

        public static Dictionary<string, Func<VirtualMachine, Property>> PropertiesDefined = new Dictionary<string, Func<VirtualMachine, Property>> () // __proto__
        {
            // properties
            ["constructor"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     tv.PrototypeInternal = actx.Avm.Prototypes["Object"];
                     return Value.FromObject(tv);
                 }, avm)), true, false, false),
            ["__proto__"] = (avm) => Property.A(
                 (tv) => Value.FromObject(tv.PrototypeInternal),
                 (tv, val) => tv.PrototypeInternal = val.ToObject(),
                 false, false),
            // methods
            ["addProperty"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     string name = args.Length > 0 ? args[0].ToString() : null;
                     ASFunction getter = args.Length > 1 ? args[1].ToFunction() : null;
                     ASFunction setter = args.Length > 2 ? args[2].ToFunction() : null;
                     tv.AddProperty(actx, name, getter, setter);
                     return null;
                 }
                 , avm)), true, false, false),
            ["hasOwnProperty"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     var name = args.Length > 0 ? args[0].ToString() : null;
                     var ans = tv.HasOwnMember(name);
                     return Value.FromBoolean(ans);
                 }
                 , avm)), true, false, false),
            ["isPropertyEnumerable"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     var name = args.Length > 0 ? args[0].ToString() : null;
                     var ans = tv.IsPropertyEnumerable(name);
                     return Value.FromBoolean(ans);
                 }
                 , avm)), true, false, false),
            ["isPrototypeOf"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     var theClass = args.Length > 0 ? args[0].ToObject() : null;
                     var ans = tv.IsPrototypeOf(theClass);
                     return Value.FromBoolean(ans);
                 }
                 , avm)), true, false, false),
            ["toString"] = (avm) => Property.D(Value.FromFunction(new NativeFunction(
                 (actx, tv, args) => {
                     var ans = tv.ToString();
                     return Value.FromString(ans);
                 }
                 , avm)), true, false, false),
        };

        public static Dictionary<string, Func<VirtualMachine, Property>> StaticPropertiesDefined = new Dictionary<string, Func<VirtualMachine, Property>> () // constructor
        { 
            // ["prototype"] = (avm) => Property.D(Value.FromObject(avm.GetPrototype("Object")), true, false, false),
        };

        /// <summary>
        /// equivalent to [[Construct]]
        /// </summary>
        public ASObject(VirtualMachine vm, string prototype_indicator)
        {
            //Actionscript variables are not case sensitive!
            _properties = new Dictionary<string, Property>(StringComparer.OrdinalIgnoreCase);
            if (vm != null && vm.Prototypes.TryGetValue(prototype_indicator, out var proto))
                PrototypeInternal = proto;
            else if (vm != null && vm.Prototypes.TryGetValue("Object", out var protoObj))
                PrototypeInternal = protoObj;
            Class = prototype_indicator;
        }

        /// <summary>
        /// this ActionScript object is not bound bound to an item, e.g. for global object
        /// </summary>
        public ASObject(VirtualMachine vm) : this(vm, "Object") { } // Do not simplify it due to the reflection

        public Dictionary<string, Property>.KeyCollection GetAllProperties() { return _properties.Keys; }

        public virtual void AddProperty(ExecutionContext context, string name, ASFunction getter, ASFunction setter)
        {
            Func<ASObject, Value> fget = getter == null ? null : (tv) =>
            {
                var val = getter.Invoke(context, tv, new Value[0]);
                return val;
            };
            Action<ASObject, Value> fset = setter == null ? null : (tv, val) =>
            {
                setter.Invoke(context, tv, new Value[1] { val });
            };
            var prop = Property.A(fget, fset, false, true);
            _properties[name] = prop;
        }

        public virtual void SetOwnProperty(string name, Property prop)
        {
            _properties[name] = prop;
        }

        public void SetPropertyFlags(string name, int set, int clear)
        {
            if (!HasOwnMember(name)) return;
            var swp = (set & 4) > 0;
            var sdp = (set & 2) > 0;
            var shid = (set & 1) > 0;
            var cwp = (clear & 4) > 0;
            var cdp = (clear & 2) > 0;
            var chid = (clear & 1) > 0;
            var prop = _properties[name];
            if (swp) prop.Writable = false;
            if (sdp) prop.Configurable = false;
            if (shid) prop.Enumerable = false;
            if (cwp) prop.Writable = true;
            if (cdp) prop.Configurable = true;
            if (chid) prop.Enumerable = true;
        }

        public virtual Value GetOwnMember(string name, ASObject localOverride = null)
        {
            Value ans = Value.Undefined();
            if (_properties.TryGetValue(name, out var prop))
            {
                if (prop is NamedDataProperty)
                    ans = ((NamedDataProperty) prop).Value;
                else if (prop is NamedAccessoryProperty)
                {
                    var prop_ = (NamedAccessoryProperty) prop;
                    if (prop_.Get != null)
                        ans = prop_.Get(localOverride == null ? this : localOverride);
                    else
                        Logger.Warn($"[WARN] property's getter is null: {name}");
                }
            }
            return ans;
        }

        public virtual void SetOwnMember(string name, Value val, ASObject localOverride = null)
        {
            if (val == null)
                val = Value.FromObject(null);
            if (_properties.TryGetValue(name, out var prop))
            {
                if (prop.Writable)
                {
                    if (prop is NamedDataProperty prop_)
                    {
                        if (localOverride == null)
                        {
                            if (prop_.Configurable)
                                prop_.Enumerable = val.IsEnumerable();
                            prop_.Value = val;
                            _properties[name] = prop_;
                        }
                        else
                            localOverride.SetOwnMember(name, val, null);
                    }
                    
                    else if (prop is NamedAccessoryProperty prop__)
                    {
                        if (prop__.Set != null)
                            prop__.Set(localOverride == null ? this : localOverride, val);
                        else
                            Logger.Warn($"[WARN] property's setter is null: {name}");
                    }
                }
                else
                    Logger.Warn($"[WARN] Unwritable property: {name}");
            }
            else
            {
                var prop1 = Property.D(val, true, true, true);
                _properties[name] = prop1;
            }
        }

        public virtual bool HasOwnMember(string name)
        {
            return _properties.ContainsKey(name) &&
                    ((_properties[name] is NamedDataProperty &&
                    ((NamedDataProperty) _properties[name]).Value != null &&
                    ((NamedDataProperty) _properties[name]).Value.Type != ValueType.Undefined) ||
                    _properties[name] is NamedAccessoryProperty);
        }

        public virtual bool DeleteOwnMember(string name)
        {
            if (_properties.TryGetValue(name, out var prop))
            {
                // not necessary?
                // if (prop is NamedDataProperty && ((NamedDataProperty) prop).Value.Type == ValueType.Undefined)
                //     return true;
                if (prop.Configurable)
                {
                    _properties.Remove(name);
                    return true;
                }
                else
                {
                    Logger.Warn($"[WARN] Unconfigurable property: {name}");
                    return false;
                }
                    
            }
            return true;
        }

        /// <summary>
        /// return a variable, when not present return Undefined
        /// equivalent to the specop [[Get]]
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual Value GetMember(string name)
        {
            var thisVar = this;
            while (thisVar != null)
            {
                if (thisVar.HasOwnMember(name))
                {
                    var val = thisVar.GetOwnMember(name, this);
                    if (val.Type != ValueType.Undefined)
                        return val;
                }
                thisVar = thisVar.PrototypeInternal;
            }
            Logger.Warn($"[WARN] Undefined property: {name}");
            return Value.Undefined();
        }


        public virtual void SetMember(string name, Value val)
        {
            // case 1: property exists locally
            if (HasOwnMember(name))
                SetOwnMember(name, val);
            else
            {
                var thisVar = this;
                while (thisVar != null)
                {
                    // case 2: property exists on chain
                    if (thisVar.HasOwnMember(name))
                    {
                        thisVar.SetOwnMember(name, val, this);
                        return;
                    }
                    thisVar = thisVar.PrototypeInternal;
                }
                // case 3: property does not exist, create one
                SetOwnMember(name, val);
            }
        }

        public virtual bool HasMember(string name)
        {
            var thisVar = this;
            while (thisVar != null)
            {
                if (thisVar.HasOwnMember(name))
                {
                    return true;
                }
                thisVar = thisVar.PrototypeInternal;
            }
            return false;
        }

        public virtual bool DeleteMember(string name)
        {
            return DeleteOwnMember(name);
        }

        public virtual bool IsPropertyEnumerable(string name)
        {
            var thisVar = this;
            while (thisVar != null)
            {
                if (thisVar.HasOwnMember(name))
                {
                    return thisVar._properties[name].Enumerable;
                }
                thisVar = thisVar.PrototypeInternal;
            }
            return false;
        }


        // judgements

        /// <summary>
        ///
        /// this: should be a prototype (has "constructor": function)
        /// theClass: should be an instance (has "prototype": constructor function)
        /// </summary>
        /// <param name="theClass"></param>
        /// <returns></returns>
        public bool IsPrototypeOf(ASObject theClass)
        {
            var ans = false;
            var proto = theClass.PrototypeInternal;
            while (proto != null)
            {
                if (proto == this)
                {
                    ans = true;
                    break;
                }
                proto = proto.PrototypeInternal;
            }
            return ans;
        }

        public bool IsFunction() { return this is ASFunction; }
        public bool IsConstructor() { return IsFunction() && prototype.IsPrototype(); }
        public bool IsPrototype() { return constructor != null && constructor.IsFunction(); }

        public bool InstanceOf(ASObject cst) // TODO Not complete
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

        // conversion

        public override string ToString()
        {
            // TODO 
            return $"[{GetType().Name}]";
        }

        public Value ToStringAS(ExecutionContext actx = null)
        {
            var funcStr = "toString";
            if (HasMember(funcStr))
            {
                var v = GetMember(funcStr);
                if (v != null && v.IsCallable())
                {
                    var sret = v.ToFunction().Invoke(actx, this, new Value[] { });
                    return sret;
                }
                else
                    return null;
            }
            else
                return null;
        }

        public Value ValueOfAS(ExecutionContext actx = null)
        {
            var funcStr = "valueOf";
            if (HasMember(funcStr))
            {
                var v = GetMember(funcStr);
                if (v != null && v.IsCallable())
                {
                    var sret = v.ToFunction().Invoke(actx, this, new Value[] { });
                    return sret;
                }
                else
                    return null;
            }
            else
                return null;
        }

        public static bool EqualsAS(ASObject x, ASObject y, ExecutionContext actx = null)
        {
            return object.Equals(x, y); // TODO
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hint">0 = none, 1 = string, 2 = number</param>
        /// <returns></returns>
        public virtual Value DefaultValue(int hint = 0, ExecutionContext actx = null)
        {
            if (hint == 1)
            {
                var ts = ToStringAS(actx);
                if (Value.IsPrimitive(ts))
                    return ts;
                var vo = ValueOfAS(actx);
                if (Value.IsPrimitive(vo))
                    return vo;
                // todo throw exception inside vm?
                throw new NotImplementedException("TypeError Exception");
            }
            else if (hint == 2)
            {
                var vo = ValueOfAS(actx);
                if (Value.IsPrimitive(vo))
                    return vo;
                var ts = ToStringAS(actx);
                if (Value.IsPrimitive(ts))
                    return ts;
                // todo throw exception inside vm?
                throw new NotImplementedException("TypeError Exception");
            }
            else
            {
                // TODO date object
                return DefaultValue(2, actx);
            }
        }

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
