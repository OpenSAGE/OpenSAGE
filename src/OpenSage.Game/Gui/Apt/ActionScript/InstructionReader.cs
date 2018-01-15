using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    /// <summary>
    /// Reads a list of instructions from a stream
    /// </summary>
    public sealed class InstructionReader
    {
        private BinaryReader _reader;
        private uint _offset;

        public List<InstructionBase> Instructions { get; private set; }

        public InstructionReader(Stream input)
        {
            _reader = new BinaryReader(input);
            _offset = _reader.ReadUInt32();
            Instructions = new List<InstructionBase>();
        }

        public void Parse()
        {
            var current = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
            bool parsing = true;

            while (parsing)
            {
                //now reader the instructions
                var type = _reader.ReadByteAsEnum<InstructionType>();
                var aligned = InstructionAlignment.IsAligned(type);

                if (aligned)
                {
                    var padding = _reader.Align(4);
                    if (padding > 0)
                    {
                        Instructions.Add(new Padding(padding));
                    }
                }

                InstructionBase instruction = null;
                List<Value> parameters = new List<Value>();

                switch (type)
                {
                    case InstructionType.Play:
                        instruction = new Play();
                        break;
                    case InstructionType.Stop:
                        instruction = new Stop();
                        break;
                    case InstructionType.Add:
                        instruction = new Add();
                        break;
                    case InstructionType.Subtract:
                        instruction = new Subtract();
                        break;
                    case InstructionType.Multiply:
                        instruction = new Multiply();
                        break;
                    case InstructionType.Divide:
                        instruction = new Divide();
                        break;
                    case InstructionType.Not:
                        instruction = new Not();
                        break;
                    case InstructionType.Pop:
                        instruction = new Pop();
                        break;
                    case InstructionType.SetVariable:
                        instruction = new SetVariable();
                        break;
                    case InstructionType.StringConcat:
                        instruction = new StringConcat();
                        break;
                    case InstructionType.GetProperty:
                        instruction = new GetProperty();
                        break;
                    case InstructionType.Trace:
                        instruction = new Trace();
                        break;
                    case InstructionType.Return:
                        instruction = new Return();
                        break;
                    case InstructionType.Add2:
                        instruction = new Add2();
                        break;
                    case InstructionType.Equals2:
                        instruction = new Equals2();
                        break;
                    case InstructionType.PushDuplicate:
                        instruction = new PushDuplicate();
                        break;
                    case InstructionType.GetMember:
                        instruction = new GetMember();
                        break;
                    case InstructionType.SetMember:
                        instruction = new SetMember();
                        break;
                    case InstructionType.EA_PushThis:
                        instruction = new PushThis();
                        break;
                    case InstructionType.EA_PushZero:
                        instruction = new PushZero();
                        break;
                    case InstructionType.EA_PushOne:
                        instruction = new PushOne();
                        break;
                    case InstructionType.EA_CallMethodPop:
                        instruction = new CallMethodPop();
                        break;
                    case InstructionType.Greater:
                        instruction = new Greater();
                        break;
                    case InstructionType.EA_PushThisVar:
                        instruction = new PushThisVar();
                        break;
                    case InstructionType.EA_PushGlobalVar:
                        instruction = new PushGlobalVar();
                        break;
                    case InstructionType.EA_ZeroVar:
                        instruction = new ZeroVar();
                        break;
                    case InstructionType.EA_PushTrue:
                        instruction = new PushTrue();
                        break;
                    case InstructionType.EA_PushFalse:
                        instruction = new PushFalse();
                        break;
                    case InstructionType.EA_PushUndefined:
                        instruction = new PushUndefined();
                        parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                        break;
                    case InstructionType.GotoFrame:
                        instruction = new GotoFrame();
                        break;
                    case InstructionType.GetURL:
                        instruction = new GetUrl();
                        parameters.Add(Value.FromString(_reader.ReadStringAtOffset()));
                        parameters.Add(Value.FromString(_reader.ReadStringAtOffset()));
                        break;
                    case InstructionType.SetRegister:
                        instruction = new SetRegister();
                        parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                        break;
                    case InstructionType.ConstantPool:
                        {
                            instruction = new ConstantPool();
                            var count = _reader.ReadUInt32();
                            var constants = _reader.ReadFixedSizeArrayAtOffset<uint>(() => _reader.ReadUInt32(), count);

                            foreach (var constant in constants)
                            {
                                parameters.Add(Value.FromConstant(constant));
                            }
                        }
                        break;
                    case InstructionType.GotoLabel:
                        instruction = new GotoLabel();
                        parameters.Add(Value.FromString(_reader.ReadStringAtOffset()));
                        break;
                    case InstructionType.PushData:
                        {
                            instruction = new PushData();

                            var count = _reader.ReadUInt32();
                            var constants = _reader.ReadFixedSizeArrayAtOffset<uint>(() => _reader.ReadUInt32(), count);

                            foreach (var constant in constants)
                            {
                                parameters.Add(Value.FromConstant(constant));
                            }
                        }
                        break;
                    case InstructionType.GetURL2:
                        instruction = new GetUrl2();
                        break;
                    case InstructionType.DefineFunction:
                        instruction = new DefineFunction();
                        var name = _reader.ReadStringAtOffset();
                        //list of parameter strings
                        var paramList = _reader.ReadListAtOffset<string>(() => _reader.ReadStringAtOffset());

                        parameters.Add(Value.FromString(name));
                        parameters.Add(Value.FromInteger(paramList.Count));
                        foreach (var param in paramList)
                        {
                            parameters.Add(Value.FromString(param));
                        }
                        //body size of the function
                        parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                        //skip 8 bytes
                        _reader.ReadUInt64();
                        break;
                    case InstructionType.BranchAlways:
                        instruction = new BranchIfTrue();
                        parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                        break;
                    case InstructionType.BranchIfTtrue:
                        instruction = new BranchIfTrue();
                        parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                        break;
                    case InstructionType.EA_PushString:
                        instruction = new PushString();
                        //the constant id that should be pushed
                        parameters.Add(Value.FromString(_reader.ReadStringAtOffset()));
                        break;
                    case InstructionType.EA_PushConstantByte:
                        instruction = new PushConstantByte();
                        //the constant id that should be pushed
                        parameters.Add(Value.FromConstant(_reader.ReadByte()));
                        break;
                    case InstructionType.EA_GetStringVar:
                        instruction = new GetStringVar();
                        parameters.Add(Value.FromString(_reader.ReadStringAtOffset()));
                        break;
                    case InstructionType.EA_GetStringMember:
                        instruction = new GetStringMember();
                        parameters.Add(Value.FromString(_reader.ReadStringAtOffset()));
                        break;
                    case InstructionType.EA_SetStringMember:
                        instruction = new SetStringMember();
                        parameters.Add(Value.FromString(_reader.ReadStringAtOffset()));
                        break;
                    case InstructionType.EA_PushValueOfVar:
                        instruction = new PushValueOfVar();
                        //the constant id that should be pushed
                        parameters.Add(Value.FromConstant(_reader.ReadByte()));
                        break;
                    case InstructionType.EA_GetNamedMember:
                        instruction = new GetNamedMember();
                        parameters.Add(Value.FromConstant(_reader.ReadByte()));
                        break;
                    case InstructionType.EA_CallNamedMethodPop:
                        instruction = new CallNamedMethodPop();
                        parameters.Add(Value.FromConstant(_reader.ReadByte()));
                        break;
                    case InstructionType.EA_PushFloat:
                        instruction = new PushFloat();
                        parameters.Add(Value.FromFloat(_reader.ReadSingle()));
                        break;
                    case InstructionType.EA_PushByte:
                        instruction = new PushByte();
                        parameters.Add(Value.FromInteger(_reader.ReadByte()));
                        break;
                    case InstructionType.End:
                        parsing = false;
                        break;
                    default:
                        throw new InvalidDataException("Unimplemented bytecode instruction:" + type.ToString());
                }

                if (instruction != null)
                {
                    instruction.Parameters = parameters;
                    Instructions.Add(instruction);
                }
            }
            _reader.BaseStream.Seek(current, SeekOrigin.Begin);
        }
    }
}
