using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.FileFormats.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Library;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class ExecutionContext
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ASObject This { get; private set; }
        public ASObject Global { get; private set; }
        public ExecutionContext Outer { get; private set; } 
        public AptContext Apt { get; set; }
        public InstructionStream Stream { get; set; }
        public int RegisterCount { get; private set; }
        public Dictionary<string, Value> Params { get; private set; }
        private Dictionary<string, Value> _locals; 
        public bool Return { get; set; }
        public bool Halt { get; set; }
        public Value ReturnValue { get; set; }
        public List<ConstantEntry> GlobalConstantPool { get; set; }
        public List<Value> Constants { get; set; }
        public string DisplayName { get; set; } // only used for display purpose

        private Stack<Value> _stack;
        private Stack<InstructionBase> _immiexec;
        private Value[] _registers { get; set; }

        public ExecutionContext(ASObject globalVar, ASObject thisVar, ExecutionContext outerVar, int numRegisters = 0)
        {
            // assignments
            Global = globalVar;
            This = thisVar;
            if (outerVar == this) outerVar = null;
            Outer = outerVar; // null if the most outside
            RegisterCount = numRegisters;

            // initializations
            _stack = new Stack<Value>();
            _immiexec = new Stack<InstructionBase>();
            _registers = new Value[RegisterCount];
            Params = new Dictionary<string, Value>();
            _locals = new Dictionary<string, Value>();
            Constants = new List<Value>();
            Return = false;
        }

        // basics

        public bool IsOutermost() { return Outer == null || Outer == this; }

        public void PushRecallCode(InstructionBase inst) { _immiexec.Push(inst); }
        public InstructionBase PopRecallCode() { return _immiexec.Pop(); }
        public InstructionBase FirstRecallCode() { return _immiexec.Peek(); }
        public bool HasRecallCode() { return _immiexec.Count != 0; }

        public override string ToString()
        {
            return DisplayName == null ? "[ActionContext]" : $"[{DisplayName}]";
        }

        // constant operations

        public void ReformConstantPool(List<Value> Parameters)
        {
            var pool = new List<Value>();

            // The first parameter is the constant count, omit it
            for (var i = 1; i < Parameters.Count; ++i)
            {
                Value result;
                var entry = GlobalConstantPool[Parameters[i].ToInteger()];
                switch (entry.Type)
                {
                    case ConstantType.String:
                        result = Value.FromString((string) entry.Value);
                        break;
                    case ConstantType.Register:
                        result = Value.FromRegister((uint) entry.Value);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                pool.Add(result);
            }

            Constants = pool;
        }

        // register operations

        public bool RegisterStored(int id) { return _registers[id] != null; }

        public Value GetRegister(int id) {
            if (id < 0 || id >= RegisterCount)
            {
                throw new InvalidOperationException($"Register number {id} is not appropriate! it should be 0~{RegisterCount - 1}.");
            }
            else
            {
                return RegisterStored(id) ? _registers[id] : Value.Undefined() ;
            }
            
        }

        public void SetRegister(int id, Value val)
        {
            if (id < 0 || id >= RegisterCount)
            {
                throw new InvalidOperationException($"Register number {id} is not appropriate! it should be 0~{RegisterCount - 1}.");
            }
            else
            {
                _registers[id] = val;
            }
        }

        public string DumpRegister()
        {
            var ans = $"Total {RegisterCount} Registers";
            for (int i = 0; i < RegisterCount; ++i)
            {
                if (_registers[i] != null) ans = ans + $"[{i}]{_registers[i].ToStringWithType(this)}";
            }
            return ans;
        }

        public string[] ListRegister()
        {
            var ans = new string[RegisterCount];
            var null_str = "unused";
            for (int i = 0; i < RegisterCount; ++i)
            {
                ans[i] = $"[{i}]{(_registers[i] == null ? null_str :_registers[i].ToStringWithType(this))}";
            }
            return ans;
        }

        // stack operations

        public string DumpStack()
        {
            var stack_val = _stack.ToArray();
            var ans = string.Join("|", stack_val.Select(x => x.ToStringWithType(this)).ToArray());

            ans = string.Format("TOP|{0}|BOTTOM", ans);
            return ans;
        }

        public string[] ListStack()
        {
            var ans = new string[_stack.Count];
            for (int i = 0; i < _stack.Count; ++i)
            {
                ans[i] = $"[{i}]{_stack.ElementAt(i).ToStringWithType(this)}";
            }
            return ans;
        }

        public Value GetStackElement(int index) { return _stack.ElementAt(index); }

        public void Push(Value v)
        {
            _stack.Push(v);
        }

        public void Push(Value[] v)
        {
            if (v == null) return;
            foreach (var v_ in v)
                _stack.Push(v_);
        }

        public Value Peek()
        {
            var ret = _stack.Peek();
            return ret.Resolve(this);
        }

        public Value Pop()
        {
            if (_stack.Count == 0)
            {
                logger.Warn("[WARN] Trying to pop the stack while it is empty");
                return Value.Undefined();
            }
            var ret = _stack.Pop();
            return ret.Resolve(this);
        }

        public Value[] Pop(uint count)
        {
            if (count < 1) return null;
            var ans = new Value[count];
            for (int i = 0; i < count; ++i)
                ans[i] = Pop();
            return ans;
        }

        // parameters

        /// <summary>
        /// Check if a specific string is a parameter in the current params
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns></returns>
        public bool CheckParameter(string name)
        {
            return Params.ContainsKey(name);
        }

        /// <summary>
        /// Returns the value of a parameter. Must be used with CheckParameter
        /// </summary>
        /// <param name="name">parameter name</param>
        /// <returns></returns>
        public Value GetParameter(string name)
        {
            return Params[name];
        }

        /// <summary>
        /// Check if a specific string is a local value to this context
        /// </summary>
        /// <param name="name">variable name</param>
        /// <returns></returns>
        public bool HasValueOnLocal(string name)
        {
            if (!IsOutermost())
            {
                return _locals.ContainsKey(name);
            }
            else
            {
                return Global.HasMember(name);
            }
            
        }

        public bool HasValueOnChain(string name)
        {
            var ans = false;
            var env = this;
            while (env != null)
            {
                if (env.HasValueOnLocal(name))
                {
                    ans = true;
                    break;
                }
                env = env.Outer;
            }

            return ans;
        }

        /// <summary>
        /// Returns the value of a local variable. Must be used with CheckLocal
        /// </summary>
        /// <param name="name">local name</param>
        /// <returns></returns>
        public Value GetValueOnLocal(string name)
        {
            if (!IsOutermost())
            {
                return HasValueOnLocal(name) ? _locals[name] : Value.Undefined();
            }
            else
            {
                return Global.GetMember(name);
            }
        }

        /// <summary>
        /// Checks for special handled/global objects. After that checks for child objects of the
        /// current object
        /// </summary>
        /// <param name="name">the object name</param>
        /// <returns></returns>
        public Value GetValueOnChain(string name)
        {
            var ans = Value.Undefined();
            var env = this;
            while (env != null)
            {
                if (env.HasValueOnLocal(name))
                {
                    ans = env.GetValueOnLocal(name);
                    break;
                }
                env = env.Outer;
            }
            if (ans == null || ans.Type == ValueType.Undefined)
                logger.Warn($"[WARN] Undefined property: {name}");
            return ans;
        }

        public void SetValueOnLocal(string name, Value val)
        {
            if (!IsOutermost())
            {
                _locals[name] = val;
            }
            else
            {
                Global.SetMember(name, val);
            }
        }

        public void SetValueOnChain(string name, Value val)
        {
            var env = this;
            while (env != null)
            {
                if (env.HasValueOnLocal(name))
                {
                    env.SetValueOnLocal(name, val);
                    return;
                }
                env = env.Outer;
            }
            SetValueOnLocal(name, val);
        }

        public void DeleteValueOnLocal(string name)
        {
            if (!IsOutermost())
            {
                if (HasValueOnLocal(name)) _locals.Remove(name);
            }
            else
            {
                Global.DeleteMember(name);
            }
        }

        public void DeleteValueOnChain(string name)
        {
            var env = this;
            while (env != null)
            {
                if (env.HasValueOnLocal(name))
                {
                    env.DeleteValueOnLocal(name);
                    return;
                }
                env = env.Outer;
            }
        }

        /// <summary>
        /// Get the current target path
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Value GetTarget(string target)
        {
            //empty target means the current scope
            if (target.Length == 0)
                return Value.FromObject(This);

            //depending on wether or not this is a relative path or not
            ASObject obj = target.First() == '/' ? Apt.Root.ScriptObject : This;

            foreach (var part in target.Split('/'))
            {
                if (part == "..")
                {
                    obj = ((StageObject) obj).GetParent();
                }
                else
                {
                    obj = obj.GetMember(part).ToObject();
                }
            }

            return Value.FromObject(obj);
        }

        /// <summary>
        /// Call an object constructor. Either builtin or defined earlier
        /// </summary>
        /// <param name="constructor"></param>
        /// <returns></returns>
        public void ConstructObjectAndPush(string name, Value[] args) // TODO need reconstruction
        {
            ConstructObjectAndPush(GetValueOnChain(name), args);
        }

        public void ConstructObjectAndPush(Value funcVal, Value[] args)
        {
            if (funcVal.Type != ValueType.Object || !funcVal.ToObject().IsFunction())
            {
                throw new InvalidOperationException("Not a function");
            }
            else
            {
                var cst_func = funcVal.ToFunction();
                var thisVar = Apt.Avm.ConstructClass(cst_func);
                PushRecallCode(new InstructionPushValue(Value.FromObject(thisVar)));
                cst_func.Invoke(this, thisVar, args);
            }
        }

        /// <summary>
        /// Preload specified variables
        /// </summary>
        /// <param name="flags">the flags</param>
        public void Preload(FunctionPreloadFlags flags, Value args)
        {
            //preloaded variables start at register 1
            int reg = 1;

            //order is important
            if (flags.HasFlag(FunctionPreloadFlags.PreloadThis))
            {
                _registers[reg] = Value.FromObject(This);
                _registers[reg].DisplayString = "Preload This";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadArguments))
            {
                if (args == null)
                    throw new InvalidOperationException();
                _registers[reg] = args;
                _registers[reg].DisplayString = "Preload Arguments";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadSuper))
            {
                try
                {
                    _registers[reg] = Value.FromObject(This.__proto__.__proto__.constructor);
                }
                catch
                {
                    _registers[reg] = Value.FromObject(null);
                }
                _registers[reg].DisplayString = "Preload Super";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadRoot))
            {
                _registers[reg] = Value.FromObject(Apt.Root.ScriptObject);
                _registers[reg].DisplayString = "Preload Root";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadParent))
            {
                _registers[reg] = Value.FromObject(((StageObject) This).GetParent());
                _registers[reg].DisplayString = "Preload Parent";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadGlobal))
            {
                _registers[reg] = Value.FromObject(Apt.Avm.GlobalObject);
                _registers[reg].DisplayString = "Preload Global";
                ++reg;
            }
            if (flags.HasFlag(FunctionPreloadFlags.PreloadExtern))
            {
                _registers[reg] = Value.FromObject(Apt.Avm.ExternObject);
                _registers[reg].DisplayString = "Preload Extern";
                ++reg;
            }
            if (!flags.HasFlag(FunctionPreloadFlags.SupressSuper))
            {
                try
                {
                    _locals["super"] = Value.FromObject(This.__proto__.__proto__.constructor);
                }
                catch
                {
                    _locals["super"] = Value.FromObject(null);
                }
            }

            if (!flags.HasFlag(FunctionPreloadFlags.SupressArguments))
            {
                _locals["arguments"] = args;
            }

            if (!flags.HasFlag(FunctionPreloadFlags.SupressThis))
            {
                _locals["this"] = Value.FromObject(This);
            }
        }
    }
}
