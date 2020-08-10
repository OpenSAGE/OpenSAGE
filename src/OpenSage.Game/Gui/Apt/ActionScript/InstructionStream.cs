using System;
using System.Collections.Generic;
using System.Linq;
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
        private int _index;

        public InstructionStream(InstructionCollection instructions)
        {
            _instructions = instructions;
            _index = 0;
        }

        /// <summary>
        /// Get an instruction and move to the next instruction right away. Skip paddings
        /// </summary>
        /// <returns></returns>
        public InstructionBase GetInstruction()
        {
            if (_index < 0)
            {
                throw new IndexOutOfRangeException();
            }

            if (_index - 1 > _instructions.Count)
            {
                throw new IndexOutOfRangeException();
            }

            return _instructions.GetInstructionByIndex(_index++);
        }

        /// <summary>
        /// Extract a list of instructions from the current stream
        /// </summary>
        /// <param name="bytes">The size of the instructions to be extracted in bytes</param>
        /// <returns></returns>
        public InstructionCollection GetInstructions(int bytes)
        {
            // get the amount of instructions contained in that byterange
            var startPosition = _instructions.GetPositionByIndex(_index);
            var endPosition = startPosition + bytes;


            var subRange = _instructions.GetPositionedInstructions().Skip(_index).TakeWhile((kv) => kv.Key < endPosition);
            if (subRange.Any() && subRange.First().Key != startPosition) // sanity check
            {
                throw new InvalidOperationException("Didn't not get the right instructions!");
            }

            var instructions = new SortedList<int, InstructionBase>();
            foreach (var (position, instruction) in subRange)
            {
                instructions.Add(position, instruction);
            }
            _index += instructions.Count;

            return new InstructionCollection(instructions);
        }


        /// <summary>
        /// Tells that this should be the last interation 
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            return _index == _instructions.Count;
        }

        /// <summary>
        /// Move the instruction stream
        /// </summary>
        /// <param name="offset">The offset how much to move in bytes</param>
        public void Branch(int offset)
        {
            var startPosition = _instructions.GetPositionByIndex(_index);
            var destinationPosition = startPosition + offset;
            _index = _instructions.GetIndexByPosition(destinationPosition);
        }
    }
}
