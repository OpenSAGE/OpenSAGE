using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Base;
using OpenAS2.Compilation;
using OpenAS2.Runtime.Library;
using OpenAS2.Runtime.Execution;
using OpenAS2.Runtime.Dom;

namespace OpenAS2.Runtime
{
    public abstract class Scope
    {
        public Scope? Outer { get; private set; }

        public VirtualMachine Avm { get; private set; }

        public abstract ESObject ThisUsed { get; protected set; }

        public string DisplayName { get; set; } // only used for display purpose

        public Scope(
            VirtualMachine avm,
            Scope? outerContext,
            string? displayName = null
            )
        {
            // assignments
            Avm = avm;
            if (outerContext == this) outerContext = null;
            Outer = outerContext; // null if the most outside

            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "[Unnamed]" : displayName;

        }

        // basics

        public bool IsOutermost() { return Outer == null || Outer == this; }

        public override string ToString()
        {
            return DisplayName == null ? "[Scope]" : $"[{DisplayName}]";
        }

        // constant operations

        public abstract bool HasPropertyOnLocal(string name, out PropertyDescriptor? prop);
        public abstract bool PutPropertyOnLocal(string name, PropertyDescriptor prop);
        public abstract bool DeletePropertyOnLocal(string name);

        protected bool HasPropertyOnChain(string name, out PropertyDescriptor? prop, out bool local, out Scope? s)
        {
            var ans = false;
            s = null;
            prop = null;
            local = false;
            var env = this;
            while (env != null)
            {
                if (env.HasPropertyOnLocal(name, out prop))
                {
                    ans = true;
                    s = env;
                    if (env == this)
                        local = true;
                    break;
                }
                env = env.Outer;
            }
            if (!ans)
                prop = null;
            return ans;
        }

        public bool DeletePropertyOnChain(string name)
        {
            var hasVal = HasPropertyOnChain(name, out var prop, out var _, out var s);
            return hasVal && prop!.Configurable && s!.DeletePropertyOnLocal(name);
        }

        public ESCallable.Result GetValueOnChain(ExecutionContext ec, string name)
        {
            var hasVal = HasPropertyOnChain(name, out var prop, out var _, out var s);
            if (!hasVal)
            {
                Logger.Warn($"[WARN] Undefined property: {name}");
                return ESCallable.Return(Value.Undefined());
            }
            else
            {
                if (prop is NamedAccessoryProperty pa)
                    return pa.Get(ec, s!.ThisUsed, null);
                else
                {
                    var pd = (NamedDataProperty) prop!;
                    return ESCallable.Return(pd.Value);
                }
            }
        }

        public ESCallable.Result SetValueOnLocal(ExecutionContext ec, string name, Value val)
        {
            var hasVal = HasPropertyOnLocal(name, out var prop);
            if (!hasVal)
            {
                var ans = PutPropertyOnLocal(name, PropertyDescriptor.D(val, true, false, true));
                return ESCallable.Return(Value.FromBoolean(ans));
            }
            else
            {
                if (prop is NamedAccessoryProperty pa)
                    if (pa.Set != null)
                        return pa.Set(ec, ThisUsed, null);
                    else
                        return ESCallable.Return(Value.FromBoolean(false));
                else
                {
                    var pd = (NamedDataProperty) prop!;
                    if (pd.Writable)
                        pd.Value = val;
                    return ESCallable.Return(Value.FromBoolean(pd.Writable));
                }
            }
        }

    }

    public class ObjectScope : Scope
    {
        public ESObject TheObject { get; set; }
        public ObjectScope(
            VirtualMachine avm,
            ESObject obj, 
            Scope? outerContext,
            string? displayName = null
            ) : base(avm, outerContext, displayName)
        {
            TheObject = obj;
        }
        public override ESObject ThisUsed { get => TheObject; protected set => throw new InvalidOperationException(); }

        public override bool DeletePropertyOnLocal(string name)
        {
            return TheObject.DeleteOwnProperty(name, forceDelete: false);
        }

        public override bool HasPropertyOnLocal(string name, out PropertyDescriptor? prop)
        {
            prop = TheObject.IGetProperty(name, out var own);
            return prop != null;
        }

        public override bool PutPropertyOnLocal(string name, PropertyDescriptor prop)
        {
            var ansRes = TheObject.IDefineOwnProperty(null, name, prop, doThrow: false);
            var ans = ansRes.Value.ToBoolean();
            return ans;
        }
    }

    public class RecordScope : Scope
    {
        private Dictionary<string, PropertyDescriptor> _properties;
        public RecordScope(
            VirtualMachine avm,
            Scope? outerContext,
            string? displayName = null
            ) : base(avm, outerContext, displayName)
        {
            _properties = new();
        }
        public override ESObject ThisUsed { get => throw new InvalidOperationException(); protected set => throw new InvalidOperationException(); }

        public override bool DeletePropertyOnLocal(string name)
        {
            var flag = _properties.TryGetValue(name, out var _);
            if (flag)
                _properties.Remove(name);
            return flag;
        }

        public override bool HasPropertyOnLocal(string name, out PropertyDescriptor? prop)
        {
            return _properties.TryGetValue(name, out prop);
        }

        public override bool PutPropertyOnLocal(string name, PropertyDescriptor prop)
        {
            var flag = _properties.TryGetValue(name, out var prop2);
            if (!prop2!.Writable)
                return false;
            else
            {
                _properties[name] = prop;
                return true;
            }
        }
    }
}
