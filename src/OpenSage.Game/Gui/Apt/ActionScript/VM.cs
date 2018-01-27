using System.Collections.Generic;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// The actual ActionScript VM
    /// </summary>
    public sealed class VM
    {
        private ObjectContext _global;
        private ObjectContext _extern;

        public ObjectContext GlobalObject => _global;
        public ObjectContext ExternObject => _extern;

        public VM()
        {
            _global = new ObjectContext();
            _extern = new ExternObject();
        }

        public void Execute(Function func, List<Value> args, ObjectContext scope)
        {
            var code = func.Instructions;

            var stream = new InstructionStream(code);

            var paramList = new Dictionary<string, Value>();

            for (var i=0;i<func.Parameters.Count;++i)
            {
                var name = func.Parameters[i].ToString();
                bool provided = i < args.Count;

                paramList[name] =  provided ? args[i] : Value.Undefined();
            }

            var context = new ActionContext(4)
            {
                Global = _global,
                Scope = scope,
                Apt = scope.Item.Context,
                Stream = stream,
                Params = paramList
            };

            var instr = stream.GetInstruction();

            while (instr.Type != InstructionType.End)
            {
                instr.Execute(context);

                if (stream.IsFinished())
                    break;

                instr = stream.GetInstruction();
            }
        }

        public void Execute(InstructionCollection code, ObjectContext scope)
        {
            var stream = new InstructionStream(code);

            var instr = stream.GetInstruction();
            var context = new ActionContext()
            {
                Global = _global,
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
