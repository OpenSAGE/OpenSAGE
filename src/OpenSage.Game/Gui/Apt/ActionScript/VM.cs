using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// The actual ActionScript VM
    /// </summary>
    public sealed class VM
    {
        private ObjectContext _global;

        public VM()
        {
            _global = new ObjectContext();
        }

        public void Execute(List<InstructionBase> code, ObjectContext scope)
        {
            var stream = new InstructionStream(code);

            var instr = stream.GetInstruction();
            var context = new ActionContext() { Global = _global,
                                                Scope = scope,
                                                Apt = scope.Item.Context,
                                                Stream = stream };

            while (instr.Type!=InstructionType.End)
            {
                instr.Execute(context);

                instr = stream.GetInstruction();
            }
        }
    }
}
