using System;
using System.Collections.Generic;
using System.Linq;
using OpenAS2.Compilation.Syntax;
using OpenAS2.Base;

namespace OpenAS2.Base
{
    using RawInstructionStorage = SortedList<int, RawInstruction>;
    public sealed class InstructionStream
    {

        private RawInstructionStorage _instructions;

        /// <summary>
        /// the index of the current instruction
        /// </summary>
        public int Index { get; private set;  }

        public InstructionStream(RawInstructionStorage instructions, bool createEnd = false)
        {
            _instructions = new(instructions);
            if (createEnd && _instructions.Last().Value.Type != InstructionType.End)
                _instructions.Add(
                    _instructions.Last().Key + Definition.GetParamLength(_instructions.Last().Value.Type),
                    RawInstruction.CreateEnd()
                );
            Index = 0;
        }

        public int GetPositionByIndex(int index)
        {
            return _instructions.Keys[index];
        }

        public int GetIndexByPosition(int position)
        {
            var index = _instructions.IndexOfKey(position);
            if (index > _instructions.Last().Key)
                // We branched behind the last valid instruction...
                return _instructions.Last().Key;
            return index;
        }

        /// <summary>
        /// Get an instruction. Skip paddings
        /// </summary>
        /// <returns></returns>
        public RawInstruction GetCurrentInstruction()
        {
            if (Index < 0 || _instructions.Count < Index - 1)
                throw new IndexOutOfRangeException();
            return _instructions.Values[Index];
        }

        public void ToNextInstruction()
        {
            if (Index < 0 || _instructions.Count < Index - 1)
                throw new IndexOutOfRangeException();
            ++Index;
        }

        /// <summary>
        /// Get an instruction and move to the next instruction right away. Skip paddings
        /// </summary>
        /// <returns></returns>
        public RawInstruction GetCurrentAndToNext()
        {
            if (Index < 0 || _instructions.Count < Index - 1)
                throw new IndexOutOfRangeException();
            return _instructions.Values[Index++];
        }

        /// <summary>
        /// Extract a list of instructions from the current stream
        /// </summary>
        /// <param name="byteCount">The size of the instructions to be extracted in bytes</param>
        /// <returns></returns>
        public RawInstructionStorage GetInstructions(int byteCount, bool skip = true, bool createEnd = false)
        {
            // get the amount of instructions contained in that byterange
            var startPosition = GetPositionByIndex(Index);
            var endPosition = startPosition + byteCount;


            var subRange = _instructions.Skip(Index).TakeWhile((kv) => kv.Key < endPosition);
            if (subRange.Any() && subRange.First().Key != startPosition) // sanity check
            {
                throw new InvalidOperationException("Did not get the right instructions!");
            }

            var instructions = new RawInstructionStorage();
            foreach (var (position, instruction) in subRange)
            {
                instructions.Add(position, instruction);
            }
            if (skip)
                Index += instructions.Count;

            if (createEnd && instructions.Last().Value.Type != InstructionType.End)
                instructions.Add(GetPositionByIndex(Index), RawInstruction.CreateEnd());

            return instructions;
        }


        /// <summary>
        /// Tells that this should be the last interation 
        /// </summary>
        /// <returns></returns>
        public bool IsFinished()
        {
            return Index >= _instructions.Count || Index < 0;
        }

        /// <summary>
        /// Move the instruction stream
        /// </summary>
        /// <param name="offset">The offset how much to move in bytes</param>
        public void Branch(int offset) { Index = GetBranchDestination(offset, Index); }
        public int GetBranchDestination(int offset, int index)
        {
            var startPosition = GetPositionByIndex(index);
            var destinationPosition = startPosition + offset;
            return GetIndexByPosition(destinationPosition);
        }

        public void GotoIndex(int index)
        {
            Index = index;
            if (Index < 0 || _instructions.Count < Index - 1)
                throw new IndexOutOfRangeException();
        }
    }
}
