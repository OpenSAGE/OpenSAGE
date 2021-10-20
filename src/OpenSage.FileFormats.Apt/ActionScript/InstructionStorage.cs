using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using OpenSage.FileFormats;
using static System.Text.Encoding;

namespace OpenSage.FileFormats.Apt.ActionScript
{
    // Provides some helper functions to parse instructions
    // Implements IDisposable, will automatically seek back the stream once disposed.
    public sealed class InstructionParseHelper : IDisposable
    {
        public readonly int StartPosition;
        public int FurthestBranchDestination;

        // Current Position (at the time of call) relative to StartPosition
        public int CurrentPosition => (int) (_inputStream.Position);

        private Stream _inputStream;
        private long _prevPos;
        private bool _disposed;

        public InstructionParseHelper(Stream input, long instructionStartPosition)
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
        public bool CanParse(SortedList<int, InstructionCode> instructions)
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

    public sealed class InstructionStorage : IDataStorage
    {
        private readonly SortedList<int, InstructionCode> _instructions;

        public int Count => _instructions.Count;

        public InstructionStorage(SortedList<int, InstructionCode> instructions)
        {
            _instructions = instructions;
        }

        public IReadOnlyCollection<KeyValuePair<int, InstructionCode>> GetPositionedInstructions()
        {
            return _instructions;
        }

        public ReadOnlyCollection<InstructionCode> GetInstructions()
        {
            return new ReadOnlyCollection<InstructionCode>(_instructions.Values);
        }

        public string Serialize()
        {
            List<string> s = new();
            foreach (var (pos, val) in _instructions)
                s.Append(val.Serialize());
            return JsonSerializer.Serialize(s);
        }

        public static InstructionStorage Deserialize(string str)
        {
            
            var s = JsonSerializer.Deserialize<List<string>>(str);
            var k = s.Select(x => (0, InstructionCode.Deserialize(x)));
            SortedList<int, InstructionCode> l = new();
            foreach (var (kk, kv) in k)
                l.Add(kk, kv);
            return new InstructionStorage(l);
        }

        public InstructionStorage AddEnd()
        {
            if (_instructions.Values.Last().Type == InstructionType.End)
                return this;
            var _inst = new SortedList<int, InstructionCode>(_instructions);
            _inst[_inst.Keys.Last() + 1] = new InstructionCode(InstructionType.End, new());
            return new InstructionStorage(_inst);
        }

        public int GetPositionByIndex(int index)
        {
            return _instructions.Keys[index];
        }

        public int GetIndexByPosition(int position)
        {
            var index = _instructions.IndexOfKey(position);

            if (index > _instructions.Last().Key)
            {
                // We branched behind the last valid instruction...
                return _instructions.Last().Key;
            }

            return index;
        }

        public InstructionCode GetInstructionByIndex(int index)
        {
            return _instructions.Values[index];
        }

        public InstructionCode GetInstructionByPosition(int position)
        {
            return _instructions[position];
        }

        public static List<RawParamType> GetParamSequence(InstructionType type)
        {
            var paramSequence = new List<RawParamType>();

            switch (type)
            {
                // no parameters
                case InstructionType.ToNumber:
                case InstructionType.NextFrame:
                case InstructionType.Play:
                case InstructionType.Stop:
                case InstructionType.Add:
                case InstructionType.Subtract:
                case InstructionType.Multiply:
                case InstructionType.Divide:
                case InstructionType.BitwiseAnd:
                case InstructionType.BitwiseOr:
                case InstructionType.BitwiseXOr:
                case InstructionType.Greater:
                case InstructionType.LessThan:
                case InstructionType.LogicalAnd:
                case InstructionType.LogicalOr:
                case InstructionType.LogicalNot:
                case InstructionType.StringEquals:
                case InstructionType.Pop:
                case InstructionType.ToInteger:
                case InstructionType.GetVariable:
                case InstructionType.SetVariable:
                case InstructionType.StringConcat:
                case InstructionType.GetProperty:
                case InstructionType.SetProperty:
                case InstructionType.CloneSprite:
                case InstructionType.RemoveSprite:
                case InstructionType.Trace:
                case InstructionType.Random:
                case InstructionType.Return:
                case InstructionType.Modulo:
                case InstructionType.TypeOf:
                case InstructionType.Add2:
                case InstructionType.LessThan2:
                case InstructionType.ToString:
                case InstructionType.PushDuplicate:
                case InstructionType.Increment:
                case InstructionType.Decrement:
                case InstructionType.EA_PushThis:
                case InstructionType.EA_PushZero:
                case InstructionType.EA_PushOne:
                case InstructionType.EA_PushThisVar:
                case InstructionType.EA_PushGlobalVar:
                case InstructionType.EA_ZeroVar:
                case InstructionType.EA_PushTrue:
                case InstructionType.EA_PushFalse:
                case InstructionType.EA_PushNull:
                case InstructionType.EA_PushUndefined:
                case InstructionType.CallFunction:
                case InstructionType.EA_CallFunc:
                case InstructionType.EA_CallFuncPop:
                case InstructionType.CallMethod:
                case InstructionType.EA_CallMethod:
                case InstructionType.EA_CallMethodPop:
                case InstructionType.DefineLocal:
                case InstructionType.Var:
                case InstructionType.Delete:
                case InstructionType.Delete2:
                case InstructionType.Enumerate2:
                case InstructionType.Equals2:
                case InstructionType.GetMember:
                case InstructionType.SetMember:
                case InstructionType.InitArray:
                case InstructionType.InitObject:
                case InstructionType.NewMethod:
                case InstructionType.NewObject:
                case InstructionType.StrictEqual:
                case InstructionType.Extends:
                case InstructionType.InstanceOf:
                case InstructionType.ImplementsOp:
                case InstructionType.CastOp:
                case InstructionType.GetTime:
                case InstructionType.End:
                case InstructionType.GetURL2:
                    break;

                // fixed type parameters

                case InstructionType.GotoFrame:
                case InstructionType.GotoFrame2:
                    paramSequence.Add(RawParamType.I32);
                    break;

                case InstructionType.EA_PushByte:
                    paramSequence.Add(RawParamType.UI8);
                    break;
                case InstructionType.EA_PushRegister:
                    paramSequence.Add(RawParamType.UI8);
                    paramSequence.Add(RawParamType.Register);
                    break;
                case InstructionType.EA_PushValueOfVar:
                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_GetNamedMember:
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                case InstructionType.EA_CallNamedMethodPop:
                case InstructionType.EA_CallNamedMethod:
                    paramSequence.Add(RawParamType.UI8);
                    paramSequence.Add(RawParamType.Constant);
                    break;

                case InstructionType.EA_PushShort:
                    paramSequence.Add(RawParamType.UI16);
                    break;
                case InstructionType.EA_PushConstantWord:
                    paramSequence.Add(RawParamType.UI16);
                    paramSequence.Add(RawParamType.Constant);
                    break;

                case InstructionType.SetRegister:
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.Register);
                    break;
                case InstructionType.EA_PushLong:
                    paramSequence.Add(RawParamType.UI32);
                    break;

                case InstructionType.GotoLabel:
                case InstructionType.EA_PushString:
                case InstructionType.EA_GetStringVar:
                case InstructionType.EA_SetStringVar:
                case InstructionType.EA_GetStringMember:
                case InstructionType.EA_SetStringMember:
                    paramSequence.Add(RawParamType.String);
                    break;

                case InstructionType.GetURL:
                    paramSequence.Add(RawParamType.String);
                    paramSequence.Add(RawParamType.String);
                    break;

                case InstructionType.EA_PushFloat:
                    paramSequence.Add(RawParamType.Float);
                    break;

                // REALLY WEIRD stuffs

                case InstructionType.PushData:
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.ArraySize);
                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.Constant);
                    paramSequence.Add(RawParamType.ArrayEnd);
                    break;
                case InstructionType.ConstantPool:
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.ArraySize);
                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.Constant);
                    paramSequence.Add(RawParamType.ArrayEnd);
                    break;
                case InstructionType.DefineFunction2:
                    paramSequence.Add(RawParamType.String); // name
                    paramSequence.Add(RawParamType.UI32); // nParams
                    paramSequence.Add(RawParamType.ArraySize);
                    paramSequence.Add(RawParamType.UI8); // nRegs
                    paramSequence.Add(RawParamType.UI24); // flags

                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.I32);
                    paramSequence.Add(RawParamType.String);
                    paramSequence.Add(RawParamType.ArrayEnd);

                    paramSequence.Add(RawParamType.I32); // codeLength
                    paramSequence.Add(RawParamType.Jump64);
                    break;
                case InstructionType.DefineFunction:
                    paramSequence.Add(RawParamType.String); // name
                    paramSequence.Add(RawParamType.UI32); // nParams
                    paramSequence.Add(RawParamType.ArraySize);

                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.String);
                    paramSequence.Add(RawParamType.ArrayEnd);

                    paramSequence.Add(RawParamType.I32); // codeLength
                    paramSequence.Add(RawParamType.Jump64);
                    break;

                case InstructionType.BranchAlways:
                case InstructionType.BranchIfTrue:
                    paramSequence.Add(RawParamType.I32);
                    paramSequence.Add(RawParamType.BranchOffset);
                    break;

                default:
                    throw new InvalidDataException("Unimplemented bytecode instruction parsing:" + type.ToString());
            }
            return paramSequence;
        }

        public static InstructionStorage Parse(Stream input, long instructionsPosition)
        {
            var instructions = new SortedList<int, InstructionCode>();
            using (var helper = new InstructionParseHelper(input, instructionsPosition))
            {
                var reader = helper.GetReader();
                while (helper.CanParse(instructions))
                {
                    //now reader the instructions
                    var instructionPosition = helper.CurrentPosition;
                    var type = reader.ReadByteAsEnum<InstructionType>();
                    var requireAlignment = InstructionAlignment.IsAligned(type);

                    if (requireAlignment)
                    {
                        reader.Align(4);
                    }

                    InstructionCode instruction = null;
                    var parameters = new List<ValueStorage>();
                    var paramSequence = GetParamSequence(type);

                    instruction = new InstructionCode(type, paramSequence);
                    instruction.Parse(reader);
                    instructions.Add(instructionPosition, instruction);
                }
            }

            return new InstructionStorage(instructions);
        }

        public void Write(BinaryWriter writer, MemoryPool memory)
        {
            foreach (var kvp in _instructions)
            {
                var instruction = kvp.Value;
                if (instruction.Type == InstructionType.Padding)
                    continue;

                writer.Write((Byte) instruction.Type);
                if (InstructionAlignment.IsAligned(instruction.Type))
                    while (writer.BaseStream.Position % Constants.IntPtrSize != 0)
                        writer.Write((Byte) 0);
                    
                instruction.Write(writer, memory);
            }

        }
    }
}
