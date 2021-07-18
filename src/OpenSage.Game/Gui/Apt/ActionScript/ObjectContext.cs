using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript.Library;

namespace OpenSage.Gui.Apt.ActionScript
{
    // ECMA-262 #8.3.1
    public class Property
    {
        public bool Enumerable { get; set; }
        public bool Configurable { get; set; }

        public static NamedDataProperty D(Value val, bool w, bool e, bool c) {
            return new NamedDataProperty()
            {
                Value = val,
                Writable = w,
                Enumerable = e,
                Configurable = c,
            };
        }
        public static NamedAccessoryProperty A(Func<VM, ObjectContext, Value> g, Action<VM, ObjectContext, Value> s, bool e, bool c)
        {
            return new NamedAccessoryProperty()
            {
                Get = g, 
                Set = s, 
                Enumerable = e,
                Configurable = c,
            };
        }
    }
    public class NamedDataProperty: Property
    {
        public Value Value { get; set; }
        public bool Writable { get; set; }
    }
    public class NamedAccessoryProperty: Property
    {
        public Func<VM, ObjectContext, Value> Get { get; set; }
        public Action<VM, ObjectContext, Value> Set { get; set; }
    }

    public class ObjectContext
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ObjectContext __proto__
        {
            get { return GetMember("__proto__").ToObject(); }
            set { SetMember("__proto__", Value.FromObject(value)); }
        }
        public ObjectContext prototype
        {
            get { return GetMember("prototype").ToObject(); }
            set { SetMember("prototype", Value.FromObject(value)); }
        }
        public Function constructor
        {
            get { return GetMember("constructor").ToFunction(); }
            set { SetMember("constructor", Value.FromFunction(value)); }
        }

        /// <summary>
        /// The item that this context is connected to
        /// </summary>
        public DisplayItem Item { get; private set; }

        /// <summary>
        /// Contains functions and member variables
        /// </summary>
        public Dictionary<string, Value> Variables { get; set; }
        private Dictionary<string, Property> Properties;

        // public List<Value> Constants { get; set; }

        // internal properties

        protected ObjectContext Prototype;
        protected string Class => "Object";
        protected bool Extensible;

        private static ObjectContext _createPrototypes()
        {
            var _op = new ObjectContext(false);
            FunctionPrototype = new ObjectContext(false);
            _foc = new SpecOp((a, b, c) => new ObjectContext(), false);
            _ffc = new SpecOp((a, b, c) => throw new NotImplementedException("Nonetheless this is not the real ActionScript."), false); ;

            FunctionPrototype.constructor = _ffc;
            FunctionPrototype.__proto__ = _op;
            _foc.__proto__ = FunctionPrototype;
            _ffc.__proto__ = FunctionPrototype;
            _foc.prototype = _op;
            _ffc.prototype = FunctionPrototype;

            _op.constructor = _foc;
            return _op;
        }

        // class inheritance
        protected internal static Function _foc { get; private set; }
        protected internal static Function _ffc { get; private set; }
        public static readonly ObjectContext ObjectPrototype = _createPrototypes(); // = new ObjectContext(false) { __proto__ = null, constructor = Function.ObjectConstructor };
        public static ObjectContext FunctionPrototype { get; private set; } // = new ObjectContext() { constructor = Function.FunctionConstructor };

        protected static Dictionary<string, Property> _properties = new Dictionary<string, Property>
        {
            ["constructor"] = Property.D(Value.FromFunction(new SpecOp(
                 (vm, tv, args) => { tv.Prototype = vm.ObjectPrototype; }
                 )), true, false, false),
            ["__proto__"] = Property.A(
                 (vm, tv) => Value.FromObject(tv.Prototype),
                 (vm, tv, val) => tv.Prototype = val.ToObject(), 
                 false, false),
        };

        protected static Dictionary<string, Property> _staticProperties = new Dictionary<string, Property>
        {
            ["prototype"] = Property.D(Value.Undefined(), true, false, false),
        };

        protected static Dictionary<string, Property> _methods = new Dictionary<string, Property>
        {
            ["addProperty"] = Property.D(Value.FromFunction(new SpecOp(
                 (vm, tv, args) => {
                     string name = args.Length > 0 ? args[0].ToString() : null;
                     Function getter = args.Length > 1 ? args[1].ToFunction() : null;
                     Function setter = args.Length > 2 ? args[2].ToFunction() : null;
                     tv.AddProperty(name, getter, setter);
                 }
                 )), true, false, false),
            ["hasOwnProperty"] = Property.D(Value.FromFunction(new SpecOp(
                 (vm, tv, args) => {
                     var name = args.Length > 0 ? args[0].ToString() : null;
                     var ans = tv.HasOwnMember(name);
                     vm.CurrentContext().Push(Value.FromBoolean(ans));
                 }
                 )), true, false, false),
            ["isPropertyEnumerable"] = Property.D(Value.FromFunction(new SpecOp(
                 (vm, tv, args) => {
                     var name = args.Length > 0 ? args[0].ToString() : null;
                     var ans = tv.IsPropertyEnumerable(name);
                     vm.CurrentContext().Push(Value.FromBoolean(ans));
                 }
                 )), true, false, false),
            ["isPrototypeOf"] = Property.D(Value.FromFunction(new SpecOp(
                 (vm, tv, args) => {
                     var name = args.Length > 0 ? args[0].ToObject() : null;
                     var ans = tv.IsPrototypeOf(name);
                     vm.CurrentContext().Push(Value.FromBoolean(ans));
                 }
                 )), true, false, false),
            ["toString"] = Property.D(Value.FromFunction(new SpecOp(
                 (vm, tv, args) => {
                     var ans = tv.ToString();
                     vm.CurrentContext().Push(Value.FromString(ans));
                 }
                 )), true, false, false),
        };
        /*
        public static ObjectContext GetPrototype(VM vm)
        {
            var base_proto = Prototype.Prototype;
            Prototype = base_proto;
            foreach (var prop in _properties)
            {
                AddMember(prop.Key, prop.Value);
            }
        }

        public static Function GetConstructor(VM vm)
        {
            var obj_proto = new ObjectContext((VM) null);
            var func_proto = new ObjectContext((VM) null);
            var _foc = new SpecOp((a, b, c) => { }, false);
            var _ffc = new SpecOp((a, b, c) => throw new NotImplementedException("Nonetheless this is not the real ActionScript."), false); ;

            func_proto.constructor = _ffc;
            func_proto.__proto__ = obj_proto;
            _foc.__proto__ = func_proto;
            _ffc.__proto__ = func_proto;
            _foc.prototype = obj_proto;
            _ffc.prototype = func_proto;

            obj_proto.constructor = _foc;

            // Function constr = new SpecOp()
        }
        */

        // equivalent to [[Construct]]
        public ObjectContext(VM vm)
        {
            Variables = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);

            Prototype = vm.ObjectPrototype;
        }

        /// <summary>
        /// this ActionScript object is not bound bound to an item, e.g. for global object
        /// </summary>
        public ObjectContext()
        {
            //Actionscript variables are not case sensitive!
            Variables = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
            // Constants = new List<Value>();

            __proto__ = ObjectPrototype;

            // SetMember("hasOwnProperty", Value.FromFunction(new FunctionCaller(
            //     (vm, tv, args) => Value.FromBoolean(tv.HasMember(args[0].ToString()))
            //     ))) ;
        }

        internal protected ObjectContext(bool JustUsedToCreateObjectPrototype)
        {
            Variables = new Dictionary<string, Value>(StringComparer.OrdinalIgnoreCase);
            // Constants = new List<Value>();

            __proto__ = null;
        }

        /// <summary>
        /// this ActionScript object is bound to an item
        /// </summary>
        /// <param name="item"></param>
        /// the item that this context is bound to
        public ObjectContext(DisplayItem item) : this()
        {
            Item = item;

            //initialize item dependent properties
            InitializeProperties();
        }

        public virtual void AddProperty(string name, Function getter, Function setter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// return a variable, when not present return Undefined
        /// equivalent to the specop [[Get]]
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual Value GetMember(string name)
        {
            if (IsBuiltInVariable(name))
            {
                return GetBuiltInVariable(name);
            }

            if (Variables.TryGetValue(name, out var result))
            {
                return result;
            }

            logger.Warn($"[WARN] Undefined variable: {name}");
            return Value.Undefined();
        }


        public virtual void SetMember(string name, Value val)
        {
            logger.Warn("Not comprehensive function: SetMember");
            if (IsBuiltInVariable(name))
            {
                SetBuiltInVariable(name, val);
            }
            Variables[name] = val;
        }

        public virtual bool HasMember(string name)
        {
            if (IsBuiltInVariable(name))
            {
                throw new NotImplementedException();
            }
            else
            {
                return Variables.ContainsKey(name);
            }
            
        }

        public virtual bool HasOwnMember(string name)
        {
            throw new NotImplementedException();
            return false;
        }
        public virtual bool IsPropertyEnumerable(string name)
        {
            throw new NotImplementedException();
            return false;
        }

        public virtual void DeleteMember(string name)
        {
            if (IsBuiltInVariable(name))
            {
                throw new NotImplementedException();
            }
            else
            {
                if (HasMember(name)) Variables.Remove(name);
            }
        }
        /// <summary>
        ///
        /// this: should be a prototype (has "constructor": function)
        /// theClass: should be an instance (has "prototype": constructor function)
        /// </summary>
        /// <param name="theClass"></param>
        /// <returns></returns>
        public bool IsPrototypeOf(ObjectContext theClass)
        {
            var ans = false;
            var proto = __proto__;
            while (proto != null)
            {
                if (proto == theClass)
                {
                    ans = true;
                    break;
                }
                proto = proto.__proto__;
            }
            return ans;
        }

        /// <summary>
        /// Check wether or not a string is a builtin flash variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual bool IsBuiltInVariable(string name)
        {
            if (name == "__proto__" || name == "prototype" || name == "constructor")
                return false;
            return Builtin.IsBuiltInVariable(name);
        }

        /// <summary>
        /// Get builtin variable
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public virtual Value GetBuiltInVariable(string name)
        {
            return Builtin.GetBuiltInVariable(name, this);
        }

        /// <summary>
        /// Set a builtin flash variable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        public virtual void SetBuiltInVariable(string name, Value val)
        {
            Builtin.SetBuiltInVariable(name, this, val);
        }

        /// <summary>
        /// Check whether or not a string is a builtin flash function
        /// </summary>
        /// <param name="name">function name</param>
        /// <returns></returns>
        public virtual bool IsBuiltInFunction(string name)
        {
            return Builtin.IsBuiltInFunction(name);
        }

        /// <summary>
        /// Execute a builtin function maybe move builtin functions elsewhere
        /// </summary>
        /// <param name="actx"></param>
        /// <param name="name">function name</param>
        /// <param name="args"></param>
        public virtual void CallBuiltInFunction(ActionContext actx, string name, Value[] args)
        {
            Builtin.CallBuiltInFunction(name, actx, this, args);
        }


        public bool IsFunction()  { return this is Function || __proto__ == FunctionPrototype;  }
        public bool IsConstructor() { return IsFunction() && prototype.IsPrototype(); }
        public bool IsPrototype() { return constructor != null && constructor.IsFunction(); }

        public bool InstanceOf(ObjectContext cst) // TODO Not complete
        {
            return cst.IsFunction() && (cst.prototype == this.__proto__);
        }

        // properties

        private void InitializeProperties()
        {
            //TODO: avoid new fancy switch
            switch (Item.Character)
            {
                case Text t:
                    Variables["textColor"] = Value.FromString(t.Color.ToHex());
                    break;
            }
        }

        /// <summary>
        /// used by text
        /// </summary>
        /// <param name="value">value name</param>
        /// <returns></returns>
        public Value ResolveValue(string value, ObjectContext ctx)
        {
            var path = value.Split('.');
            var obj = ctx.GetParent();
            var member = path.Last();

            for (var i = 0; i < path.Length - 1; i++)
            {
                var fragment = path[i];

                if (Builtin.IsBuiltInVariable(fragment))
                {
                    obj = Builtin.GetBuiltInVariable(fragment, obj).ToObject();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            return obj.GetMember(member);
        }

        public ObjectContext GetParent()
        {
            ObjectContext result = null;

            if (Item.Parent != null)
            {
                result = Item.Parent.ScriptObject;
            }

            return result;
        }

        public Value GetProperty(PropertyType property)
        {
            Value result = null;

            switch (property)
            {
                case PropertyType.Target:
                    result = Value.FromString(GetTargetPath());
                    break;
                case PropertyType.Name:
                    result = Value.FromString(Item.Name);
                    break;
                case PropertyType.X:
                    result = Value.FromFloat(Item.Transform.GeometryTranslation.X);
                    break;
                case PropertyType.Y:
                    result = Value.FromFloat(Item.Transform.GeometryTranslation.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }

            return result;
        }

        public void SetProperty(PropertyType property, Value val)
        {
            switch (property)
            {
                case PropertyType.Visible:
                    Item.Visible = val.ToBoolean();
                    break;
                case PropertyType.XScale:
                    Item.Transform.Scale((float) val.ToFloat(), 0.0f);
                    break;
                case PropertyType.YScale:
                    Item.Transform.Scale(0.0f, (float) val.ToFloat());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Calculates the absolute target path
        /// </summary>
        /// <returns>the target</returns>
        private string GetTargetPath()
        {
            string path;

            if (GetParent() == null)
                path = "/";
            else
            {
                path = GetParent().GetTargetPath();
                path += Item.Name;
            }

            return path;
        }

        public override string ToString()
        {
            return Item == null ? "null item" : Item.Name;
        }

        public Value ToPrimitive()
        {
            return null;
        }

        public string ToStringDisp()
        {
            string ans = "{\n";
            foreach (string s in Variables.Keys)
            { 
                Variables.TryGetValue(s, out var v);
                ans = ans + s + ": " + v + ", \n";
            }
            ans = ans + "}";
            return ans;
        }
    }
}
