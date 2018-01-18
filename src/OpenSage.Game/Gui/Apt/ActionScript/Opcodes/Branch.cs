using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    //branch to a relative offset if the value on stack is true
    public sealed class BranchIfTrue : IInstruction
    {
        public InstructionType Type => InstructionType.BranchIfTtrue;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 4;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //branch to a relative offset
    public sealed class BranchAlways : IInstruction
    {
        public InstructionType Type => InstructionType.BranchAlways;
        public List<Value> Parameters { get; set; }
        public bool Aligned => false;
        public uint Size => 4;

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
