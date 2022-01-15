using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenSage.FileFormats.Apt.ActionScript;
using static System.Text.Encoding;

namespace OpenSage.FileFormats.Apt
{
    // Provides some helper functions to parse instructions
    // Implements IDisposable, will automatically seek back the stream once disposed.
    public sealed class InstructionParsingHelper : IDisposable
    {
        public readonly int StartPosition;
        public int FurthestBranchDestination;

        // Current Position (at the time of call) relative to StartPosition
        public int CurrentPosition => (int) (_inputStream.Position);

        private Stream _inputStream;
        private long _prevPos;
        private bool _disposed;

        public InstructionParsingHelper(Stream input, long instructionStartPosition)
        {
            _disposed = false;

            _inputStream = input;
            _prevPos = _inputStream.Position;

            StartPosition = (int) instructionStartPosition;
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
        public bool CanParse(SortedList<int, RawInstruction> instructions)
        {
            return instructions.Count == 0 ||
                instructions.Last().Value.Type != InstructionType.End ||
                CurrentPosition <= FurthestBranchDestination;
        }

        // Will calculate FurthestBranchDestination with offset and the current RelativePosition
        public void ReportBranchOffset(int offset)
        {
            FurthestBranchDestination = Math.Max(FurthestBranchDestination, CurrentPosition + offset);
        }
    }

    public sealed class InstructionStorage : IMemoryStorage
    {
        private readonly SortedList<int, RawInstruction> _instructions;

        public int Count => _instructions.Count;

        public InstructionStorage(SortedList<int, RawInstruction> instructions)
        {
            _instructions = instructions;
        }

        public IReadOnlyCollection<KeyValuePair<int, RawInstruction>> GetPositionedInstructions()
        {
            return _instructions;
        }

        public ReadOnlyCollection<RawInstruction> GetInstructions()
        {
            return new ReadOnlyCollection<RawInstruction>(_instructions.Values);
        }

        public static InstructionStorage Parse(Stream input, long instructionsPosition)
        {
            var instructions = new SortedList<int, RawInstruction>();
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

    public static class InstructionBinaryIOUtils
    {
        public static RawInstruction Parse(InstructionType type, BinaryReader reader, InstructionParsingHelper helper)
        {
            var paramTypes = Definition.GetParamSequence(type);

            var parameters = new List<RawValue>();

            var cur_index = 0;
            var array_size = -1;
            var array_begin = -1;
            var array_read = -1;
            long old_offset = -1;
            while (cur_index < paramTypes.Count)
            {
                var t = paramTypes[cur_index];
                switch (t)
                {
                    case RawParamType.UI8:
                        parameters.Add(RawValue.FromInteger(reader.ReadByte()));
                        ++cur_index;
                        break;
                    case RawParamType.UI16:
                        parameters.Add(RawValue.FromInteger(reader.ReadUInt16()));
                        ++cur_index;
                        break;
                    case RawParamType.UI24:
                        parameters.Add(RawValue.FromUInteger(reader.ReadUInt24()));
                        ++cur_index;
                        break;
                    case RawParamType.UI32:
                        parameters.Add(RawValue.FromUInteger(reader.ReadUInt32()));
                        ++cur_index;
                        break;

                    case RawParamType.I8:
                        parameters.Add(RawValue.FromInteger(reader.ReadSByte()));
                        ++cur_index;
                        break;
                    case RawParamType.I16:
                        parameters.Add(RawValue.FromInteger(reader.ReadInt16()));
                        ++cur_index;
                        break;
                    case RawParamType.I32:
                        parameters.Add(RawValue.FromInteger(reader.ReadInt32()));
                        ++cur_index;
                        break;

                    case RawParamType.Jump8:
                        reader.ReadByte();
                        ++cur_index;
                        break;
                    case RawParamType.Jump16:
                        reader.ReadUInt16();
                        ++cur_index;
                        break;
                    case RawParamType.Jump32:
                        reader.ReadUInt32();
                        ++cur_index;
                        break;
                    case RawParamType.Jump64:
                        reader.ReadUInt64();
                        ++cur_index;
                        break;

                    case RawParamType.Float:
                        parameters.Add(RawValue.FromFloat(reader.ReadSingle()));
                        ++cur_index;
                        break;
                    case RawParamType.Double:
                        parameters.Add(RawValue.FromFloat(reader.ReadDouble()));
                        ++cur_index;
                        break;

                    case RawParamType.String:
                        parameters.Add(RawValue.FromString(reader.ReadStringAtOffset()));
                        ++cur_index;
                        break;

                    case RawParamType.Constant:
                        parameters[parameters.Count - 1] = parameters.Last().ToConstant();
                        ++cur_index;
                        break;
                    case RawParamType.Register:
                        parameters[parameters.Count - 1] = parameters.Last().ToRegister();
                        ++cur_index;
                        break;

                    case RawParamType.BranchOffset:
                        if (helper != null)
                            helper.ReportBranchOffset(parameters.Last().Number);
                        ++cur_index;
                        break;

                    case RawParamType.ArraySize:
                        array_size = parameters.Last().Number;
                        ++cur_index;
                        break;
                    case RawParamType.ArrayBegin:
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        else if (array_begin >= 0)
                            throw new InvalidOperationException("Unfortunately, we do not support cascaded arrays yet.");
                        else if (array_size == 0)
                        {
                            reader.ReadUInt32();
                            while (paramTypes[cur_index] != RawParamType.ArrayEnd)
                                ++cur_index;
                            ++cur_index;
                            break;
                        }
                        array_begin = cur_index;
                        array_read = 0;
                        ++cur_index;

                        var listOffset = reader.ReadUInt32();
                        old_offset = reader.BaseStream.Position;
                        reader.BaseStream.Seek(listOffset, SeekOrigin.Begin);

                        break;

                    case RawParamType.ArrayEnd:
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        ++array_read;
                        if (array_read >= array_size)
                        {
                            array_begin = -1;
                            array_read = -1;
                            array_size = -1;
                            reader.BaseStream.Seek(old_offset, SeekOrigin.Begin);
                            old_offset = -1;
                            ++cur_index;
                        }
                        else
                        {
                            cur_index = array_begin + 1;
                        }
                        break;


                    default:
                        throw new NotImplementedException();
                }
            }

            var ans = new RawInstruction(type, parameters);
            return ans;
        }

        public static void Write(this RawInstruction inst, BinaryWriter writer, BinaryMemoryChain pool)
        {
            var cur_index = 0;
            var cur_param_index = 0;
            var array_size = -1;
            var array_begin = -1;
            var array_read = -1;
            var orig_writer = writer;
            var orig_pool = pool;

            var paramTypes = Definition.GetParamSequence(inst.Type);
            var parameters = inst.Parameters;

            while (cur_index < paramTypes.Count)
            {
                var t = paramTypes[cur_index];
                var p = cur_param_index >= parameters.Count ? null : parameters[cur_param_index];
                var p_prev = cur_param_index == 0 ? null : parameters[cur_param_index - 1];
                ++cur_param_index;
                switch (t)
                {
                    case RawParamType.UI8:
                        writer.Write((Byte) p.Number);
                        ++cur_index;
                        break;
                    case RawParamType.UI16:
                        writer.Write((UInt16) p.Number);
                        ++cur_index;
                        break;
                    case RawParamType.UI24:
                        writer.WriteUInt24((uint) p.Number);
                        ++cur_index;
                        break;
                    case RawParamType.UI32:
                        writer.Write((UInt32) p.Number);
                        ++cur_index;
                        break;

                    case RawParamType.I16:
                        writer.Write((Int16) p.Number);
                        ++cur_index;
                        break;
                    case RawParamType.I32:
                        writer.Write((Int32) p.Number);
                        ++cur_index;
                        break;

                    case RawParamType.Jump8:
                        writer.Write((Byte) 0);
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.Jump16:
                        writer.Write((UInt16) 0);
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.Jump32:
                        writer.Write((UInt32) 0);
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.Jump64:
                        writer.Write((UInt64) 0);
                        --cur_param_index;
                        ++cur_index;
                        break;

                    case RawParamType.Float:
                        writer.Write((Single) p.Decimal);
                        ++cur_index;
                        break;
                    case RawParamType.Double:
                        writer.Write((Double) p.Decimal);
                        ++cur_index;
                        break;

                    case RawParamType.String:
                        writer.WriteStringAtOffset(p.String, pool);
                        ++cur_index;
                        break;

                    case RawParamType.Constant:
                    case RawParamType.Register:
                    case RawParamType.BranchOffset:
                        --cur_param_index;
                        ++cur_index;
                        break;

                    case RawParamType.ArraySize:
                        array_size = p_prev.Number;
                        --cur_param_index;
                        ++cur_index;
                        break;
                    case RawParamType.ArrayBegin:
                        --cur_param_index;
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        else if (array_begin >= 0)
                            throw new InvalidOperationException("Unfortunately, we do not support cascaded arrays yet.");

                        array_begin = cur_index;
                        pool.RegisterPostOffset((uint) writer.BaseStream.Position);
                        writer.Write((UInt32) 0);
                        array_read = 0;
                        writer = pool.Writer;
                        pool = pool.Post;

                        if (array_size == 0)
                        {
                            while (paramTypes[cur_index] != RawParamType.ArrayEnd)
                                ++cur_index;
                            break;
                        }
                        ++cur_index;
                        break;

                    case RawParamType.ArrayEnd:
                        --cur_param_index;
                        if (array_size < 0)
                            throw new InvalidOperationException("Array size must be assigned in prior.");
                        ++array_read;
                        if (array_read >= array_size)
                        {
                            array_begin = -1;
                            array_read = -1;
                            array_size = -1;
                            writer = orig_writer;
                            pool = orig_pool;
                            ++cur_index;
                        }
                        else
                        {
                            cur_index = array_begin + 1;
                        }
                        break;


                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
