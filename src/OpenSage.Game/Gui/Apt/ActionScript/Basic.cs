using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class ConstantPool : IInstruction
    {
        public InstructionType Type => InstructionType.ConstantPool;
        public List<Value> Parameters { get; set; }
        public bool Aligned => true;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
