using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Apt;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// The actual ActionScript VM
    /// </summary>
    public sealed class VM
    {
        private TimeInterval _lastTick;
        private Dictionary<string, ValueTuple<TimeInterval, int, Function, ObjectContext, Value[]>> _intervals;
        private Stack<ActionContext> _callStack;

        public ObjectContext GlobalObject { get; }
        public ActionContext GlobalContext { get; }
        public ObjectContext ExternObject { get; }

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
        public VM()
        {
            GlobalObject = new ObjectContext();
            GlobalContext = new ActionContext(GlobalObject, GlobalObject, null, 4);
            ExternObject = new ExternObject(this);
            _intervals = new Dictionary<string, ValueTuple<TimeInterval, int, Function, ObjectContext, Value[]>>();
            _callStack = new Stack<ActionContext>();
        }

        // interval operations

        public void CreateInterval(string name, int duration, Function func, ObjectContext ctx, Value[] args)
        {
            _intervals[name] = (_lastTick,
                                duration,
                                func,
                                ctx,
                                args
                                );
        }

        public void UpdateIntervals(TimeInterval current)
        {
            for (int i = 0; i < _intervals.Count; ++i)
            {
                var interval = _intervals.Values.ElementAt(i);

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

        // stack operations

        public void PushStack(ActionContext context) { _callStack.Push(context); }
        public void ForceReturn() { }

        public Value Execute(Function func, Value[] args, ObjectContext scope)
        {
            if (func == Function.FunctionConstructor)
            {
                throw new NotImplementedException("Unfortunately, we do not have an interpreter yet.");
            }
            else if (func == Function.ObjectConstructor)
            {
                return Value.FromObject(new ObjectContext());
            }

            var code = func.Instructions;            

            var localScope = new ObjectContext(scope.Item)
            {
                Constants = func.Constants,
                Variables = scope.Variables
            };

            var context = GetActionContext(func.NumberRegisters, code, localScope, scope.Item.Character.Container.Constants.Entries);
            var stream = context.Stream;
            //new ActionContext()
            //{
            //    Global = GlobalObject,
            //    Scope = localScope,
            //    Apt = scope.Item.Context,
            //    Stream = stream,
            //    Constants = scope.Item.Character.Container.Constants.Entries
            //};

            //parameters in the old version are just stored as local variables
            if (!func.IsNewVersion)
            {
                for (var i = 0; i < func.Parameters.Count; ++i)
                {
                    var name = func.Parameters[i].ToString();
                    bool provided = i < args.Length;

                    context.Params[name] = provided ? args[i] : Value.Undefined();
                }
            }
            else
            {
                for (var i = 0; i < func.Parameters.Count; i += 2)
                {
                    var reg = func.Parameters[i].ToInteger();
                    var name = func.Parameters[i + 1].ToString();
                    var argIndex = i / 2;
                    bool provided = (argIndex) < args.Length;

                    if (reg != 0)
                    {
                        context.SetRegister(reg, provided ? args[argIndex] : Value.Undefined());
                    }
                    else
                    {
                        context.Params[name] = provided ? args[argIndex] : Value.Undefined();
                    }
                }
            }

            if (func.IsNewVersion)
            {
                context.Preload(func.Flags);
            }

            var instr = stream.GetInstruction();
            InstructionBase prevInstr = null;

            while (instr.Type != InstructionType.End)
            {
                instr.Execute(context);

                if (context.Return)
                    return context.Pop();

                if (stream.IsFinished())
                    break;

                prevInstr = instr;
                instr = stream.GetInstruction();
            }

            return Value.Undefined();
        }

        public ActionContext GetActionContext(int numRegisters, List<ConstantEntry> consts, ActionContext outerVar, ObjectContext thisVar, InstructionCollection code)
        {
            if (thisVar is null) thisVar = GlobalObject;
            var context = new ActionContext(GlobalObject, thisVar, outerVar, numRegisters)
            {
                Apt = thisVar.Item != null ? thisVar.Item.Context : null,
                Stream = new InstructionStream(code),
                GlobalConstantPool = consts,
            };
            return context;
        }

        public ActionContext GetActionContext(int numRegisters, InstructionCollection code, ObjectContext scope, List<ConstantEntry> consts)
        {
            var stream = new InstructionStream(code);
        
            var context = new ActionContext(GlobalObject, scope, null, numRegisters)
            {
                Apt = scope.Item.Context,
                Stream = stream,
                GlobalConstantPool = consts,
            };
            return context;
        }

        public InstructionBase ExecuteOnce(ActionContext context)
        {
            var instr = context.Stream.GetInstruction();
            instr.Execute(context);
            return instr;
        }
        public void Execute(InstructionCollection code, ObjectContext scope, List<ConstantEntry> consts) { Execute(GetActionContext(4, code, scope, consts)); }
        public void Execute(ActionContext context)
        { 
            var stream = context.Stream;
            var instr = stream.GetInstruction();

            InstructionBase prev = null;

            while (instr.Type != InstructionType.End)
            {
                instr.Execute(context);

                prev = instr;
                instr = stream.GetInstruction();
            }
        }

        public void Handle(ActionContext context, string url, string target)
        {
            UrlHandler.Handle(CommandHandler, MovieHandler, context, url, target);
        }
    }
}
