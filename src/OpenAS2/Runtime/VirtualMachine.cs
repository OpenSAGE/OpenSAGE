using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Runtime.Opcodes;
using OpenAS2.Runtime.Library;
using OpenAS2.Base;
using InstructionBase = OpenAS2.Runtime.Opcodes.InstructionBase;

namespace OpenAS2.Runtime
{
    /// <summary>
    /// The actual ActionScript VM
    /// </summary>
    public sealed class VirtualMachine
    {

        private TimeInterval _lastTick;
        private DateTime _pauseTick;
        private bool _paused;
        private Dictionary<string, ValueTuple<TimeInterval, int, ESFunction, ESObject, Value[]>> _intervals;

        public DomHandler Dom { get; private set; }

        private Stack<ExecutionContext> _callStack;
        private Queue<ExecutionContext> _execQueue;

        public ESObject GlobalObject { get; }
        public ExecutionContext GlobalContext { get; }
        public ESObject ExternObject { get; }
        
        public Dictionary<string, ESObject> Prototypes { get; private set; }
        public Dictionary<ESObject, Type> PrototypesInverse { get; private set; }

        public ESObject GetPrototype(string name) { return Prototypes.TryGetValue(name, out var value) ? value : null; }

        public void RegisterClass(string className, Type classType)
        {
            var cst_param = new VirtualMachine[] { this };
            var newProto = (ESObject) Activator.CreateInstance(classType, cst_param)!;
            var props = (Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>) classType.GetField("PropertiesDefined").GetValue(null)!;
            var stats = (Dictionary<string, Func<VirtualMachine, PropertyDescriptor>>) classType.GetField("StaticPropertiesDefined").GetValue(null)!;
            foreach (var p in props)
                newProto.IDefineOwnProperty(p.Key, p.Value(this));
            if (!newProto.IHasOwnProperty("constructor"))
                newProto.constructor = new NativeFunction((actx, tv, args) => Value.Undefined(), this); // TODO not sure if it is correct
            var cst = newProto.constructor; 
            cst.prototype = newProto;
            foreach (var p in stats)
                cst.IDefineOwnProperty(p.Key, p.Value(this));
            Prototypes[className] = newProto;
            PrototypesInverse[newProto] = classType;
        }

        public VirtualMachine(DomHandler? dom = null)
        {
            Dom = dom; // TODO

            _intervals = new Dictionary<string, ValueTuple<TimeInterval, int, ESFunction, ESObject, Value[]>>();
            _paused = false;

            _callStack = new Stack<ExecutionContext>();
            _execQueue = new Queue<ExecutionContext>();

            // initialize prototypes and constructors of Object and Function class
            Prototypes = new Dictionary<string, ESObject>();

            var objProto = new ESObject("Object", true, null, null);
            var funcProto = new ESFunction("Function", true, objProto, null, FunctionUtils.ReturnUndefined, FunctionUtils.ReturnUndefined, null);
            var errProto = new ESError("Error", true, objProto, null);
            var objCst = new ESFunction("Function", true, funcProto, null, ESObject.ICallObj, ESObject.IConstructObj, new string[1] { "arg1" });
            var funcCst = new ESFunction("Function", true, funcProto, null, ESFunction.IConstructAndCall, ESFunction.IConstructAndCall, new string[1] { "code" });
            var errCst = new ESFunction("Function", true, funcProto, null, ESError.IConstructAndCall, ESError.IConstructAndCall, new string[1] { "message" });

            objCst.ConnectPrototype(objProto, true);
            funcCst.ConnectPrototype(funcProto, true);
            errCst.ConnectPrototype(errProto, true);

            // TODO strict mode


            Prototypes["Object"] = objProto;
            Prototypes["Function"] = funcProto;
            Prototypes["Error"] = errProto;

            PrototypesInverse = new Dictionary<ESObject, Type>() { [objProto] = typeof(ESObject), [funcProto] = typeof(DefinedFunction), [errProto] = typeof(ESError). };

            foreach (var p in ESObject.PropertiesDefined)
                objProto.IDefineOwnProperty(p.Key, p.Value(this));
            foreach (var p in ESFunction.PropertiesDefined)
                funcProto.IDefineOwnProperty(p.Key, p.Value(this));
            foreach (var p in ESError.PropertiesDefined)
                errProto.IDefineOwnProperty(p.Key, p.Value(this));

            foreach (var p in ESObject.StaticPropertiesDefined)
                objCst.IDefineOwnProperty(p.Key, p.Value(this));
            foreach (var p in ESFunction.StaticPropertiesDefined)
                funcCst.IDefineOwnProperty(p.Key, p.Value(this));
            foreach (var p in ESError.StaticPropertiesDefined)
                errCst.IDefineOwnProperty(p.Key, p.Value(this));


            // initialize built-in classes
            foreach (var c in Builtin.BuiltinClasses)
            {
                if (c.Key == "Object" || c.Key == "Function" || c.Key == "Error")
                    continue;
                RegisterClass(c.Key, c.Value);
            }

            // initialize global vars and methods
            GlobalObject = new ESObject(this); // TODO replace it to Stage
            GlobalContext = new ExecutionContext(this, GlobalObject, GlobalObject, null, 4) { DisplayName = "GlobalContext", };
            ExternObject = new ExternObject(this);
            PushContext(GlobalContext);

            // expose builtin stuffs
            // classes
            foreach (var c in Prototypes)
                GlobalObject.IPut(c.Key, Value.FromFunction(c.Value.constructor));
            // variables
            foreach (var c in Builtin.BuiltinVariables)
                GlobalObject.IDefineOwnProperty(c.Key, c.Value(this));
            // functions
            foreach (var c in Builtin.BuiltinFunctions)
            {
                var f = Value.FromFunction(new NativeFunction(c.Value, this));
                GlobalObject.IDefineOwnProperty(c.Key, PropertyDescriptor.D(f, false, false, false));
            }
        }

        // interval & debug operations

        public void CreateInterval(string name, int duration, ESFunction func, ESObject context, Value[] args)
        {
            _intervals[name] = (_lastTick,
                                duration, // milliseconds
                                func,
                                context,
                                args
                                );
        }

        public void UpdateIntervals(TimeInterval current)
        {
            foreach (var interval_kv in _intervals)
            {
                var interval = interval_kv.Value;

                if (current.TotalTime.TotalMilliseconds > (interval.Item1.TotalTime.TotalMilliseconds + interval.Item2))
                {
                    EnqueueContext(interval.Item3, interval.Item4, interval.Item5, "Interval");
                    interval.Item1 = current;
                }
            }
            _lastTick = current;
            ExecuteUntilEmpty();
        }
         
        public void ClearInterval(string name)
        {
            _intervals.Remove(name);
        }

        public void Pause()
        {
            if (!_paused)
            {
                _paused = true;
                _pauseTick = DateTime.Now;
            }
        }

        public bool Paused() { return _paused; }

        public void Resume()
        {
            if (_paused)
            {
                _paused = false;
                var span = DateTime.Now.Subtract(_pauseTick);
                _lastTick = new TimeInterval(_lastTick.TotalTime.Add(span), _lastTick.DeltaTime);
            }
        }

        // error handling?

        public void ThrowError(ESError e)
        {
            // TODO
            throw new Exception(e.ToString());
        }


        // stack & queue operations

        public void PushContext(ExecutionContext context) { _callStack.Push(context); }
        public ExecutionContext PopContext()
        {
            if (IsCurrentContextGlobal())
                throw new InvalidOperationException("Gloabl execution context is not possible to pop.");
            else
                return _callStack.Pop();
        }
        public ExecutionContext CurrentContext() { return _callStack.Peek(); }
        public bool IsCurrentContextGlobal() { return _callStack.Count == 1 || CurrentContext().IsOutermost(); }

        public string DumpContextStack()
        {
            var stack_val = _callStack.ToArray();
            var ans = string.Join("\n", stack_val.Select(x => x.ToString()).ToArray());
            return ans;
        }

        public string[] ListContextStack()
        {
            var ans = new string[_callStack.Count];
            for (int i = 0; i < _callStack.Count; ++i)
            {
                ans[i] = $"[{i}]{(_callStack.ElementAt(i).ToString())}";
            }
            return ans;
        }

        public ExecutionContext GetStackContext(int index) { return _callStack.ElementAt(index); }

        public void EnqueueContext(ExecutionContext context) { _execQueue.Enqueue(context); }
        public ExecutionContext DequeueContext() { return _execQueue.Dequeue(); }
        public ExecutionContext CurrentContextInQueue() { return _execQueue.Peek(); }
        public bool HasContextInQueue() { return _execQueue.Count > 0; }

        public string DumpContextQueue()
        {
            var stack_val = _execQueue.ToArray();
            var ans = string.Join("\n", stack_val.Select(x => x.ToString()).ToArray());
            return ans;
        }

        public string[] ListContextQueue()
        {
            var ans = new string[_execQueue.Count];
            for (int i = 0; i < _execQueue.Count; ++i)
            {
                ans[i] = $"[{i}]{(_execQueue.ElementAt(i).ToString())}";
            }
            return ans;
        }

        public ExecutionContext GetQueueContext(int index) { return _execQueue.ElementAt(index); }
        public void EnqueueContext(ESFunction f, ESObject thisVar, Value[] args, string name = null)
        {
            if (f is DefinedFunction fd)
                EnqueueContext(fd.GetContext(this, args, thisVar));
            else
            {
                Action<ExecutionContext> f1 = (_) => f.ICall(GlobalContext, thisVar, args);
                EnqueueContext(GetActionContext(GlobalContext, thisVar, 4, null, InstructionCollection.Native(f1), name));
            }
        }

        // context, execution & debug
        public ExecutionContext GetActionContext(ExecutionContext outerVar, ESObject thisVar, int numRegisters, List<Value> consts, InstructionCollection code, string name = null)
        {
            if (thisVar is null) thisVar = GlobalObject;
            var context = new ExecutionContext(this, GlobalObject, thisVar, outerVar, numRegisters)
            {
                GlobalConstantPool = outerVar.GlobalConstantPool,
                Stream = new InstructionStream(code),
                Constants = consts,
                DisplayName = name, 
            };
            if (context.Stream.GetInstructionNoMove().Type == InstructionType.ConstantPool)
                context.Stream.GetInstructionNoMove().Execute(context);
            return context;
        }

        public ExecutionContext GetActionContext(ESObject thisVar, int numRegisters, List<ConstantEntry> consts, InstructionCollection code, string name = null)
        {
            if (thisVar is null) thisVar = GlobalObject;
            var context = new ExecutionContext(this, GlobalObject, thisVar, GlobalContext, numRegisters)
            {
                GlobalConstantPool = consts,
                Stream = new InstructionStream(code),
                DisplayName = name, 
            };
            if (context.Stream.GetInstructionNoMove().Type == InstructionType.ConstantPool)
                context.Stream.GetInstructionNoMove().Execute(context);
            return context;
        }

        /// <summary>
        /// Execute once in the current ActionContext.
        /// </summary>
        /// <returns></returns>
        public Instruction ExecuteOnce(bool ignoreBreakpoint = false) // TODO comprehensive logic needs observation
        {
            Instruction ans = null;

            var context = CurrentContext();

            var stream = context.Stream;
            if (stream.IsFinished())
                context.Halt = true;
            else
            {
                ans = stream.GetInstructionNoMove();
                if (!ignoreBreakpoint && ans.Breakpoint)
                {
                    Pause();
                    return ans;
                }
                else
                {
                    stream.ToNextInstruction();
                    ans.Execute(context);
                }
                    
            }
            // If a defined function is invoked, the current context should be the function's
            // If a native function is invoked, no context is created, so it should still be itself
            context = CurrentContext(); 

            // 3 situations can lead to Halt == true :
            // End(); Return(); stream.IsFinished().
            if (context.Halt)
            {
                if (!IsCurrentContextGlobal())
                {
                    PopContext();
                    context = CurrentContext();
                }
                else
                {
                    throw new InvalidOperationException("This situation should never happen.");
                }
            }

            // In some cases, special operations requiring being executed after function return,
            // Including function return, new object.
            while (context.HasRecallCode())
                context.PopRecallCode().Execute(context);
            return ans;
        }

        public Instruction ExecuteUntilHalt()
        {
            var context = CurrentContext();
            Instruction ans = null;
            while (!IsCurrentContextGlobal() && !context.Halt && _paused == false)
                ans = ExecuteOnce();
            return ans;
        }

        public Instruction ExecuteUntilGlobal()
        {
            Instruction ans = null;
            while (!IsCurrentContextGlobal() && _paused == false)
                ans = ExecuteUntilHalt();
            return ans;
        }

        public Instruction ExecuteUntilEmpty()
        {
            Instruction ans = null;
            while ((HasContextInQueue() || (!IsCurrentContextGlobal())) && _paused == false)
            {
                if (IsCurrentContextGlobal())
                    PushContext(DequeueContext());
                ans = ExecuteUntilGlobal();
            }
            return ans;
        }

        public Value Execute(ESFunction func, Value[] args, ESObject thisVar)
        {
            var ret = func.ICall(CurrentContext(), thisVar, args);
            if (func is DefinedFunction)
                ExecuteUntilHalt();
            return ret.ResolveReturn();
        }

    }
}
