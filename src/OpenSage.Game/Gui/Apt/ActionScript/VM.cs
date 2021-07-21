using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt;
using OpenSage.Gui.Apt.ActionScript.Opcodes;
using OpenSage.Gui.Apt.ActionScript.Library;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// The actual ActionScript VM
    /// </summary>
    public sealed class VM
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private TimeInterval _lastTick;
        private DateTime _pauseTick;
        private bool _paused;
        private Dictionary<string, ValueTuple<TimeInterval, int, Function, ObjectContext, Value[]>> _intervals;

        private Stack<ActionContext> _callStack;
        private Queue<ActionContext> _execQueue;

        public ObjectContext GlobalObject { get; }
        public ActionContext GlobalContext { get; }
        public ObjectContext ExternObject { get; }
        
        public Dictionary<string, ObjectContext> Prototypes { get; private set; }
        public Dictionary<ObjectContext, Type> PrototypesInverse { get; private set; }

        public ObjectContext GetPrototype(string name) { return Prototypes.TryGetValue(name, out var value) ? value : null; }

        // Delegate to call a function inside the engine
        public delegate void HandleCommand(ActionContext context, string command, string param);
        public HandleCommand CommandHandler;

        // Delegate to retrieve an internal variable from the engine
        public delegate Value HandleExternVariable(string variable);
        public HandleExternVariable VariableHandler;

        // Delegate to load another movie
        public delegate AptFile HandleExternalMovie(string movie);
        public HandleExternalMovie MovieHandler;

        public VM(HandleCommand hc, HandleExternVariable hev, HandleExternalMovie hem) : this()
        {
            CommandHandler = hc;
            VariableHandler = hev;
            MovieHandler = hem;
        }

        public void RegisterClass(string className, Type classType)
        {
            var cst_param = new VM[] { this };
            var newProto = (ObjectContext) Activator.CreateInstance(classType, cst_param);
            var props = (Dictionary<string, Func<VM, Property>>) classType.GetField("PropertiesDefined").GetValue(null);
            var stats = (Dictionary<string, Func<VM, Property>>) classType.GetField("StaticPropertiesDefined").GetValue(null);
            foreach (var p in props)
                newProto.SetOwnProperty(p.Key, p.Value(this));
            var cst = newProto.constructor;
            cst.prototype = newProto;
            foreach (var p in stats)
                cst.SetOwnProperty(p.Key, p.Value(this));
            Prototypes[className] = newProto;
            PrototypesInverse[newProto] = classType;
        }

        public ObjectContext ConstructClass(Function cst_func)
        {
            var proto = cst_func.prototype;
            while (proto != null)
            {
                if (PrototypesInverse.ContainsKey(proto)) break;
                proto = proto.__proto__;
            }
            var cst_param = new VM[] { this };
            var ret = (ObjectContext) Activator.CreateInstance(PrototypesInverse[proto], cst_param);
            ret.__proto__ = cst_func.prototype;
            return ret;
        }

        public VM()
        {
            _intervals = new Dictionary<string, ValueTuple<TimeInterval, int, Function, ObjectContext, Value[]>>();
            _paused = false;

            _callStack = new Stack<ActionContext>();
            _execQueue = new Queue<ActionContext>();

            // initialize prototypes and constructors of Object and Function class
            Prototypes = new Dictionary<string, ObjectContext>() { ["Object"] = new ObjectContext(null) };
            var obj_proto = Prototypes["Object"];
            var func_proto = new NativeFunction(obj_proto); // Set the PrototypeInternal property
            Prototypes["Function"] = func_proto;
            PrototypesInverse = new Dictionary<ObjectContext, Type>() { [obj_proto] = typeof(ObjectContext), [func_proto] = typeof(DefinedFunction), };

            foreach (var p in ObjectContext.PropertiesDefined)
                obj_proto.SetOwnProperty(p.Key, p.Value(this));
            foreach (var p in Function.PropertiesDefined)
                func_proto.SetOwnProperty(p.Key, p.Value(this));

            var obj_cst = obj_proto.constructor;
            var func_cst = func_proto.constructor;
            obj_cst.prototype = obj_proto;
            func_cst.prototype = func_proto;

            foreach (var p in ObjectContext.StaticPropertiesDefined)
                obj_cst.SetOwnProperty(p.Key, p.Value(this));
            foreach (var p in Function.StaticPropertiesDefined)
                func_cst.SetOwnProperty(p.Key, p.Value(this));

            // initialize built-in classes
            foreach (var c in Builtin.BuiltinClasses)
            {
                if (c.Key == "Object" || c.Key == "Function")
                    continue;
                RegisterClass(c.Key, c.Value);
            }

            // initialize global vars and methods
            GlobalObject = new ObjectContext(this); // TODO replace it to Stage
            GlobalContext = new ActionContext(GlobalObject, GlobalObject, null, 4);
            ExternObject = new ExternObject(this);
            PushContext(GlobalContext);

            // expose builtin stuffs
            // classes
            foreach (var c in Prototypes)
                GlobalObject.SetMember(c.Key, Value.FromFunction(c.Value.constructor));
            // variables
            foreach (var c in Builtin.BuiltinVariables)
                GlobalObject.SetOwnProperty(c.Key, c.Value(this));
            // functions
            foreach (var c in Builtin.BuiltinFunctions)
            {
                var f = Value.FromFunction(new NativeFunction(c.Value, this));
                GlobalObject.SetOwnProperty(c.Key, Property.D(f, false, false, false));
            }
        }

        // interval & debug operations

        public void CreateInterval(string name, int duration, Function func, ObjectContext ctx, Value[] args)
        {
            _intervals[name] = (_lastTick,
                                duration, // milliseconds
                                func,
                                ctx,
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
                    Execute(interval.Item3, interval.Item5, interval.Item4);
                    interval.Item1 = current;
                }
            }

            _lastTick = current;
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

        public void Resume()
        {
            if (_paused)
            {
                _paused = false;
                var span = DateTime.Now.Subtract(_pauseTick);
                _lastTick = new TimeInterval(_lastTick.TotalTime.Add(span), _lastTick.DeltaTime);
            }
        }

        // stack & queue operations

        public void PushContext(ActionContext context) { _callStack.Push(context); }
        public ActionContext PopContext()
        {
            if (IsCurrentContextGlobal())
                throw new InvalidOperationException("Gloabl execution context is not possible to pop.");
            else
                return _callStack.Pop();
        }
        public ActionContext CurrentContext() { return _callStack.Peek(); }
        public bool IsCurrentContextGlobal() { return _callStack.Count == 1 || CurrentContext().IsOutermost(); }
        public void ForceReturn() { }

        public void EnqueueContext(ActionContext context) { _execQueue.Enqueue(context); }
        public ActionContext DequeueContext() { return _execQueue.Dequeue(); }
        public ActionContext CurrentContextInQueue() { return _execQueue.Peek(); }
        public bool HasContextInQueue() { return _execQueue.Count > 0; }

        public void EnqueueContext(Function f, ObjectContext thisVar, Value[] args)
        {
            
        }
        public void EnqueueContext(InstructionCollection insts, ObjectContext thisVar, AptContext apt)
        {
            var context = GetActionContext(apt, thisVar, 4, null, insts);
            EnqueueContext(context);
        }

        // context, execution & debug

        public ActionContext GetActionContext(ActionContext outerVar, ObjectContext thisVar, int numRegisters, List<Value> consts, InstructionCollection code)
        {
            if (thisVar is null) thisVar = GlobalObject;
            var context = new ActionContext(GlobalObject, thisVar, outerVar, numRegisters)
            {
                Apt = outerVar.Apt,
                Stream = new InstructionStream(code),
                Constants = consts,
            };
            return context;
        }

        public ActionContext GetActionContext(AptContext apt, ObjectContext thisVar, int numRegisters, List<Value> consts, InstructionCollection code)
        {
            if (thisVar is null) thisVar = GlobalObject;
            var context = new ActionContext(GlobalObject, thisVar, GlobalContext, numRegisters)
            {
                Apt = apt,
                Stream = new InstructionStream(code),
                Constants = consts,
            };
            return context;
        }
        /*
        public ActionContext GetActionContext(int numRegisters, InstructionCollection code, ObjectContext scope, List<ConstantEntry> consts)
        {
            var stream = new InstructionStream(code);

            var context = new ActionContext(GlobalObject, scope, null, numRegisters)
            {
                Apt = scope.Item.Context,
                Stream = stream,
                Constants = consts,
            };
            return context;
        }
        */
        // execution


        /// <summary>
        /// Execute once in the current ActionContext.
        /// </summary>
        /// <returns></returns>
        public InstructionBase ExecuteOnce(bool ignoreBreakpoint = false) // TODO comprehensive logic needs observation
        {
            InstructionBase ans = null;

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

        public void ExecuteUntilHalt()
        {
            var context = CurrentContext();
            while (!IsCurrentContextGlobal() && !context.Halt && _paused == false)
                ExecuteOnce();
        }

        public void ExecuteUntilGlobal()
        {
            while (!IsCurrentContextGlobal() && _paused == false)
                ExecuteUntilHalt();
        }

        public void ExecuteUntilEmpty()
        {
            while ((HasContextInQueue() || (!IsCurrentContextGlobal())) && _paused == false)
            {
                if (IsCurrentContextGlobal())
                    PushContext(DequeueContext());
                ExecuteUntilGlobal();
            }
        }

        public Value Execute(Function func, Value[] args, ObjectContext thisVar)
        {
            var ret = func.Invoke(CurrentContext(), thisVar, args);
            if (func is DefinedFunction)
                ExecuteUntilHalt();
            return ret.ResolveReturn();
        }

        // TODO all should be replaced. This is an extremely dangerous call.
        public void Execute(InstructionCollection code, ObjectContext scope, AptContext apt)
        {
            PushContext(GetActionContext(apt, scope, 4, null, code));
            ExecuteUntilHalt();
        }

        public void Handle(ActionContext context, string url, string target)
        {
            UrlHandler.Handle(CommandHandler, MovieHandler, context, url, target);
        }
    }
}
