using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenAS2.Base;
using static System.Text.Encoding;

namespace OpenSage.FileFormats.Apt
{
    // Provides some helper functions to parse instructions
    // Implements IDisposable, will automatically seek back the stream once disposed.

    using RawInstructionStorage = SortedList<uint, RawInstruction>;

    public sealed class InstructionParsingHelper : IDisposable
    {
        public readonly uint StartPosition;
        public uint FurthestBranchDestination;

        // Current Position (at the time of call) relative to StartPosition
        public uint CurrentPosition => (uint) (_inputStream.Position);

        private Stream _inputStream;
        private long _prevPos;
        private bool _disposed;

        public InstructionParsingHelper(Stream input, long instructionStartPosition)
        {
            _disposed = false;

            _inputStream = input;
            _prevPos = _inputStream.Position;

            StartPosition = (uint) instructionStartPosition;
            _inputStream.Seek(StartPosition, SeekOrigin.Begin);
            FurthestBranchDestination = StartPosition;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _inputStream.Seek(_prevPos, SeekOrigin.Begin);
            }
        }

        public BinaryReader GetReader()
        {
            return new BinaryReader(_inputStream, UTF8, true);
        }

        // Check if we can continue to parse instructionsf
        public bool CanParse(RawInstructionStorage instructions)
        {
            return instructions.Count == 0 ||
                instructions.Last().Value.Type != InstructionType.End ||
                CurrentPosition <= FurthestBranchDestination;
        }

        // Will calculate FurthestBranchDestination with offset and the current RelativePosition
        public void ReportBranchOffset(uint offset)
        {
            FurthestBranchDestination = Math.Max(FurthestBranchDestination, CurrentPosition + offset);
        }
    }

    public sealed class InstructionStorage : IMemoryStorage
    {
        private readonly RawInstructionStorage _instructions;

        public int Count => _instructions.Count;

        public InstructionStorage(RawInstructionStorage instructions)
        {
            _instructions = instructions;
        }

        public RawInstructionStorage GetPositionedInstructions()
        {
            return _instructions;
        }

        public ReadOnlyCollection<RawInstruction> GetInstructions()
        {
            return new ReadOnlyCollection<RawInstruction>(_instructions.Values);
        }

        public InstructionStream CreateStream()
        {
            return new InstructionStream(_instructions);
        }

        public static InstructionStorage Parse(Stream input, long instructionsPosition)
        {
            var instructions = new RawInstructionStorage();
            using (var helper = new InstructionParsingHelper(input, instructionsPosition))
            {
                var reader = helper.GetReader();
                Console.WriteLine($"{helper.CurrentPosition} {helper.CurrentPosition % 4}");
                while (helper.CanParse(instructions))
                {
                    //now read the instructions
                    var instructionPosition = helper.CurrentPosition;
                    var type = reader.ReadByteAsEnum<InstructionType>();
                    var requireAlignment = Definition.IsAlignmentRequired(type);

                    if (requireAlignment)
                    {
                        reader.Align(Constants.IntPtrSize);
                    }

                    
                    var paramSequence = Definition.GetParamSequence(type);

                    var instruction = InstructionBinaryIOUtils.Parse(type, reader, helper);
                    instructions.Add(instructionPosition, instruction);
                }
            }

            return new InstructionStorage(instructions);
        }

        public void Write(BinaryWriter writer, BinaryMemoryChain memory)
        {
            // assume the writer is already aligned!
            foreach (var kvp in _instructions)
            {
                var instruction = kvp.Value;
                if (instruction.Type == InstructionType.Padding)
                    continue;

                writer.Write((Byte) instruction.Type);
                if (Definition.IsAlignmentRequired(instruction.Type))
                    writer.Align(Constants.IntPtrSize);
                    // while (writer.BaseStream.Position % Constants.IntPtrSize != 0)
                    //     writer.Write((Byte) 0);
                    
                instruction.Write(writer, memory);
            }

        }
    }


}
