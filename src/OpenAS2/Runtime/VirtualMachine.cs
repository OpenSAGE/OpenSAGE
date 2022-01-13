using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Compilation.Syntax;
using OpenAS2.Runtime.Library;
using OpenAS2.Base;

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
        public Dictionary<string, ESFunction> Constructors { get; private set; }
        // public Dictionary<ESObject, Type> PrototypesInverse { get; private set; }

        public ESObject? GetPrototype(string name) { return Prototypes.TryGetValue(name, out var value) ? value : null; }

        // TODO check!
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
            Constructors[className] = newProto.constructor;
            // PrototypesInverse[newProto] = classType;
        }

        public ESFunction ObjCst { get; private set; }
        public ESFunction FuncCst { get; private set; }
        public ESFunction ErrCst { get; private set; }

        public VirtualMachine(DomHandler? dom = null)
        {
            Dom = dom; // TODO

            _intervals = new Dictionary<string, ValueTuple<TimeInterval, int, ESFunction, ESObject, Value[]>>();
            _paused = false;

            _callStack = new();
            _execQueue = new();

            // initialize prototypes and constructors of Object and Function class

            var objProto = new ESObject("Object", true, null, null);
            var funcProto = new ESFunction("Function", true, objProto, null, FunctionUtils.ReturnUndefined, FunctionUtils.ReturnUndefined, null);
            var errProto = new ESError("Error", true, objProto, null);
            ObjCst = new ESFunction("Function", true, funcProto, null, ESObject.ICallObj, ESObject.IConstructObj, new string[1] { "arg1" });
            FuncCst = new ESFunction("Function", true, funcProto, null, ESFunction.IConstructAndCall, ESFunction.IConstructAndCall, new string[1] { "code" });
            ErrCst = new ESFunction("Function", true, funcProto, null, ESError.IConstructAndCall(null), ESError.IConstructAndCall(null), new string[1] { "message" });

            ObjCst.ConnectPrototype(objProto, true);
            FuncCst.ConnectPrototype(funcProto, true);
            ErrCst.ConnectPrototype(errProto, true);

            // TODO strict mode

            Prototypes = new() { ["Object"] = objProto, ["Function"] = funcProto, ["Error"] = errProto };
            Constructors = new() { ["Object"] = ObjCst, ["Function"] = FuncCst, ["Error"] = ErrCst };

            // TODO seriously? this seems useless
            // PrototypesInverse = new Dictionary<ESObject, Type>() { [objProto] = typeof(ESObject), [funcProto] = typeof(DefinedFunction), [errProto] = typeof(ESError) };

            foreach (var p in ESObject.PropertiesDefined)
                objProto.IDefineOwnProperty(null, p.Key, p.Value(this));
            foreach (var p in ESFunction.PropertiesDefined)
                funcProto.IDefineOwnProperty(null, p.Key, p.Value(this));
            foreach (var p in ESError.PropertiesDefined)
                errProto.IDefineOwnProperty(null, p.Key, p.Value(this));

            foreach (var p in ESObject.StaticPropertiesDefined)
                ObjCst.IDefineOwnProperty(null, p.Key, p.Value(this));
            foreach (var p in ESFunction.StaticPropertiesDefined)
                FuncCst.IDefineOwnProperty(null, p.Key, p.Value(this));
            foreach (var p in ESError.StaticPropertiesDefined)
                ErrCst.IDefineOwnProperty(null, p.Key, p.Value(this));

            foreach (var ne in ESError.NativeErrorList)
            {
                var nerrCst = new ESFunction("Function", true, funcProto, null, ESError.IConstructAndCall(ne), ESError.IConstructAndCall(ne), new string[1] { "message" });
                var nerrProto = new ESError("Error", true, objProto, null);
                nerrCst.ConnectPrototype(nerrProto, true);
                Prototypes[ne] = nerrProto;
                Constructors[ne] = nerrCst;
                foreach (var p in ESError.PropertiesDefined)
                    nerrProto.IDefineOwnProperty(null, p.Key, p.Value(this));
                foreach (var p in ESError.StaticPropertiesDefined)
                    nerrCst.IDefineOwnProperty(null, p.Key, p.Value(this));
                // PrototypesInverse[nerrProto] = typeof(ESError);
            }


            // initialize built-in classes
            foreach (var c in Builtin.BuiltinClasses)
            {
                if (c.Key == "Object" || c.Key == "Function" || c.Key == "Error")
                    continue;
                RegisterClass(c.Key, c.Value);
            }

            // initialize global vars and methods
            GlobalObject = new ESObject(this); // TODO replace it to Stage
            GlobalContext = new ExecutionContext(this, GlobalObject, GlobalObject, null, null, null, 4) { DisplayName = "Global Execution Context", };
            ExternObject = new ExternObject(this);
            PushContext(GlobalContext);

            // expose builtin stuffs
            // classes
            foreach (var c in Prototypes)
                GlobalObject.IPut(GlobalContext, c.Key, Value.FromFunction(c.Value.constructor));
            // variables
            foreach (var c in Builtin.BuiltinVariables)
                GlobalObject.IDefineOwnProperty(GlobalContext, c.Key, c.Value(this));
            // functions
            foreach (var c in Builtin.BuiltinFunctions)
            {
                var f = Value.FromFunction(new NativeFunction(this, c.Value));
                GlobalObject.IDefineOwnProperty(GlobalContext, c.Key, PropertyDescriptor.D(f, false, false, false));
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
                    interval.Item3.ICall(interval.Item4, interval.Item3, interval.Item5); // TODO wtf?
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

        public Value ConstructError(string name = "Error", string message = "", ExecutionContext? ec = null)
        {
            ec = ec ?? CurrentContext();
            var fl = Constructors.TryGetValue(name, out var cst1);
            var cst = fl ? cst1 : ErrCst;
            message = fl ? message : $"({name}){message}";
            var err = cst.IConstruct(ec, cst, new Value[] { Value.FromString(message) });
            // TODO
            return err.Value;
        }

        public void ThrowError(ESError err, ExecutionContext? ec = null)
        {
            throw new WrappedESError(err);
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


        // context, execution & debug
        public ExecutionContext CreateContext(
            ExecutionContext outerContext,
            ESObject thisVar,
            int numRegisters,
            IList<Value> consts,
            InstructionStream stream,
            IList<ConstantEntry>? globalConstants = null, 
            string? name = null)
        {
            if (thisVar is null) thisVar = GlobalObject;
            var context = new ExecutionContext(this, GlobalObject, thisVar, outerContext,
                stream,
                overrideGlobalConstPool: globalConstants,
                overrideConstPool: consts,
                displayName: name,
                numRegisters: numRegisters);
            return context;
        }

        /// <summary>
        /// Execute once in the current ActionContext.
        /// </summary>
        /// <returns></returns>
        public RawInstruction? ExecuteOnce(bool ignoreBreakpoint = false) // TODO comprehensive logic needs observation
        {
            RawInstruction? ans = null;

            if (!CurrentContext().Halted)
            {
                // TODO check breakpoint
                ans = CurrentContext().ExecuteOnceLocal();
            }
            // TODO should it be returning the new EC?

            // If a defined function is invoked, the current context should be the function's
            // If a native function is invoked, no context is created, so it should still be itself

            // 3 situations can lead to Halt == true :
            // End(); Return(); stream.IsFinished().
            
            while (CurrentContext().Halted || CurrentContext().HasResultCallback())
            {
                CurrentContext().TryClearCallback();
                if (!IsCurrentContextGlobal())
                    PopContext();
                CurrentContext().TryClearCallback();
            }

            // In most cases, special operations requiring being executed after function return.

            CurrentContext().TryClearCallback();

            return ans;
        }

        public RawInstruction? ExecuteUntilHalt()
        {
            var context = CurrentContext();
            RawInstruction? ans = null;
            while (!IsCurrentContextGlobal() && !context.Halted && _paused == false)
                ans = ExecuteOnce();
            return ans;
        }

        public RawInstruction? ExecuteUntilGlobal()
        {
            RawInstruction? ans = null;
            while (!IsCurrentContextGlobal() && _paused == false)
                ans = ExecuteUntilHalt();
            return ans;
        }

        public RawInstruction? ExecuteUntilEmpty()
        {
            RawInstruction? ans = null;
            while ((HasContextInQueue() || (!IsCurrentContextGlobal())) && _paused == false)
            {
                if (IsCurrentContextGlobal())
                    PushContext(DequeueContext());
                ans = ExecuteUntilGlobal();
            }
            return ans;
        }


    }
}
