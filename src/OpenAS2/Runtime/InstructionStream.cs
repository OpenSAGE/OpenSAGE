using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Runtime.Opcodes;

namespace OpenAS2.Runtime
{
    public sealed class InstructionStream
    {
        /// <summary>
        /// the list of instructions overall
        /// </summary>
        public InstructionCollection Instructions { get; private set; }
        /// <summary>
        /// the current instruction
        /// </summary>
        public int Index { get; private set;  }

        public InstructionStream(InstructionCollection instructions)
        {
            Instructions = instructions;
            Index = 0;
        }

        /// <summary>
        /// Get an instruction. Skip paddings
        /// </summary>
        /// <returns></returns>
        public Instruction GetInstructionNoMove()
        {
            if (Index < 0 || Instructions.Count < Index - 1)
                throw new IndexOutOfRangeException();
            return Instructions.GetInstructionByIndex(Index);
        }

        public void ToNextInstruction()
        {
            if (Index < 0 || Instructions.Count < Index - 1)
                throw new IndexOutOfRangeException();
            ++Index;
        }

        /// <summary>
        /// Get an instruction and move to the next instruction right away. Skip paddings
        /// </summary>
        /// <returns></returns>
        public Instruction GetInstruction()
        {
            if (Index < 0 || Instructions.Count < Index - 1)
                throw new IndexOutOfRangeException();
            return Instructions.GetInstructionByIndex(Index++);
        }

        /// <summary>
        /// Extract a list of instructions from the current stream
        /// </summary>
        /// <param name="bytes">The size of the instructions to be extracted in bytes</param>
        /// <returns></returns>
        public InstructionCollection GetInstructions(int bytes, bool offsetIndex = true, bool createEnd = false)
        {
            // get the amount of instructions contained in that byterange
            var startPosition = Instructions.GetPositionByIndex(Index);
            var endPosition = startPosition + bytes;


            var subRange = Instructions.GetPositionedInstructions().Skip(Index).TakeWhile((kv) => kv.Key < endPosition);
            if (subRange.Any() && subRange.First().Key != startPosition) // sanity check
            {
                throw new InvalidOperationException("Didn't not get the right instructions!");
            }

            var instructions = new SortedList<int, Instruction>();
            foreach (var (position, instruction) in subRange)
            {
                instructions.Add(position, instruction);
            }
            if (offsetIndex)
                Index += instructions.Count;

            if (createEnd)
                instructions.Add(Instructions.GetPositionByIndex(Index), new End());

            return new InstructionCollection(instructions);
        }


        /// <summary>
        /// Tells that this should be the last interation 
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            return Index == Instructions.Count || Index == -1;
        }

        /// <summary>
        /// Move the instruction stream
        /// </summary>
        /// <param name="offset">The offset how much to move in bytes</param>
        public void Branch(int offset) { Index = GetBranchDestination(offset, Index); }
        public int GetBranchDestination(int offset, int index)
        {
            var startPosition = Instructions.GetPositionByIndex(index);
            var destinationPosition = startPosition + offset;
            return Instructions.GetIndexByPosition(destinationPosition);
        }

        // TODO: OOB Check?
        public void GotoIndex(int index)
        {
            Index = index;
        }
    }
}
