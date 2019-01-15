using System.Collections.Generic;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// The actual ActionScript VM
    /// </summary>
    public sealed class VM
    {
        public ObjectContext GlobalObject { get; }
        public ObjectContext ExternObject { get; }

        public VM()
        {
            GlobalObject = new ObjectContext();
            ExternObject = new ExternObject();
        }

        public Value Execute(Function func, Value[] args, ObjectContext scope)
        {
            var code = func.Instructions;

            var stream = new InstructionStream(code);

            var localScope = new ObjectContext(scope.Item)
            {
                Constants = func.Constants,
                Variables = scope.Variables
            };

            var context = new ActionContext(func.NumberRegisters)
            {
                Global = GlobalObject,
                Scope = localScope,
                Apt = scope.Item.Context,
                Stream = stream
            };

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
                for (var i = 0; i < func.Parameters.Count; i+=2)
                {
                    var reg = func.Parameters[i].ToInteger();
                    var name = func.Parameters[i+1].ToString();
                    var argIndex = i / 2;
                    bool provided = (argIndex) < args.Length;

                    if (reg != 0)
                    {
                        context.Registers[reg] = provided ? args[argIndex] : Value.Undefined();
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

            while (instr.Type != InstructionType.End)
            {
                instr.Execute(context);

                if (context.Return)
                    return context.Stack.Pop();

                if (stream.IsFinished())
                    break;

                instr = stream.GetInstruction();
            }

            return null;
        }

        public void Execute(InstructionCollection code, ObjectContext scope)
        {
            var stream = new InstructionStream(code);

            var instr = stream.GetInstruction();
            var context = new ActionContext()
            {
                Global = GlobalObject,
                Scope = scope,
                Apt = scope.Item.Context,
                Stream = stream
            };

            while (instr.Type != InstructionType.End)
            {
                instr.Execute(context);

                instr = stream.GetInstruction();
            }
        }
    }
}
