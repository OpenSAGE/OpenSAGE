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
        private InstructionCollection _instructions;
        /// <summary>
        /// the current instruction
        /// </summary>
        private int _position;

        public InstructionStream(InstructionCollection instructions)
        {
            _instructions = instructions;
            _position = 0;
        }

        /// <summary>
        /// Get an instruction and move to the next instruction right away. Skip paddings
        /// </summary>
        /// <returns></returns>
        public InstructionBase GetInstruction()
        {
            if (_position - 1 > _instructions.Count)
            {
                throw new IndexOutOfRangeException();
            }


            //skip any possible padding
            if (_instructions[_position].Type == InstructionType.Padding)
            {
                ++_position;
            }


            return _instructions[_position++];
        }

        /// <summary>
        /// Calculate the byteoffset for an instruction
        /// </summary>
        /// <param name="instr">the index of the instruction</param>
        /// <returns></returns>
        private uint CalculateByteOffset(int instr)
        {
            uint size = 0;

            for (int i = 0; i < instr; ++i)
            {
                size += _instructions[i].Size;

                if (_instructions[i].Type != InstructionType.Padding)
                {
                    ++size;
                }

            }

            return size;
        }

        /// <summary>
        /// Extract a list of instructions from the current stream
        /// </summary>
        /// <param name="bytes">The size of the instructions to be extracted in bytes</param>
        /// <returns></returns>
        public InstructionCollection GetInstructions(int bytes)
        {
            //get the amount of instructions contained in that byterange
            int bytesCount = 0;
            int instrCount = 0;

            while (bytesCount < bytes)
            {
                var instr = _instructions[_position + instrCount];
                bytesCount += (int) instr.Size;
                if (_instructions[_position + instrCount].Type != InstructionType.Padding)
                    ++bytesCount;

                instrCount++;

            }

            if (bytesCount != bytes)
                throw new InvalidOperationException("Invalid bytesize");

            var result = _instructions
                .Skip(_position)
                .Take(instrCount)
                .ToList();
            _position += instrCount;

            return new InstructionCollection(result);
        }


        /// <summary>
        /// Tells that this should be the last interation 
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            return _position == _instructions.Count;
        }

        /// <summary>
        /// Move the instruction stream
        /// </summary>
        /// <param name="offset">The offset how much to move in bytes</param>
        public void Branch(int offset)
        {
            bool forward = offset >= 0;
            int bytesCount = 0;
            int instrCount = 0;

            if (forward)
            {
                while (bytesCount < offset)
                {
                    var instr = _instructions[_position + instrCount];
                    bytesCount += (int) instr.Size;
                    if (_instructions[_position + instrCount].Type != InstructionType.Padding)
                        ++bytesCount;

                    instrCount++;
                }
            }
            else
            {
                while (bytesCount > offset)
                {
                    var instr = _instructions[_position + instrCount];
                    bytesCount -= (int) instr.Size;
                    if (_instructions[_position + instrCount].Type != InstructionType.Padding)
                        --bytesCount;

                    instrCount--;
                }
            }

            _position += instrCount;
        }
    }
}
