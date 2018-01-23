using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class InstructionStream
    {
        /// <summary>
        /// the list of instructions overall
        /// </summary>
        private List<InstructionBase> _instructions;
        /// <summary>
        /// the current instruction
        /// </summary>
        private int _position;

        public InstructionStream(List<InstructionBase> instructions)
        {
            _instructions = instructions;
            _position = 0;
        }

        public InstructionBase GetInstruction()
        {
            if (_position - 1 > _instructions.Capacity)
                throw new IndexOutOfRangeException();

            //skip any possible padding
            if (_instructions[_position].Type == InstructionType.Padding)
                ++_position;

            return _instructions[_position++];
        }

        private uint CalculateByteOffset(int instr)
        {
            uint size = 0;

            for(int i=0;i<instr;++i)
            {
                size+=_instructions[i].Size;

                if (_instructions[i].Type != InstructionType.Padding)
                    ++size;
            }

            return size;
        }

        public List<InstructionBase> GetInstructions(int bytes)
        {
            //get the amount of instructions contained in that byterange
            int bytesCount = 0;
            int instrCount = 0;

            while(bytesCount < bytes)
            {
                var offset = CalculateByteOffset(_position + instrCount);
                var instr = _instructions[_position + instrCount];
                bytesCount += (int) instr.Size;
                if (_instructions[_position + instrCount].Type != InstructionType.Padding)
                    ++bytesCount;

                instrCount++;

            }

            if (bytesCount != bytes)
                throw new InvalidOperationException("Invalid bytesize");

            var result = _instructions.GetRange(_position, instrCount);
            _position += instrCount;

            return result;
        }
    }
}
