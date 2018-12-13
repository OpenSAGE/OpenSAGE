using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using OpenSage.Data.Utilities.Extensions;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class InstructionCollection : Collection<InstructionBase>
    {
        private BinaryReader _reader;
        private uint _offset;

        public InstructionCollection(Stream input)
        {
            _reader = new BinaryReader(input);
            _offset = _reader.ReadUInt32();
        }

        public InstructionCollection(IList<InstructionBase> list) : base(list)
        {
        }

        public void Parse()
        {
            var current = _reader.BaseStream.Position;
            _reader.BaseStream.Seek(_offset, SeekOrigin.Begin);
            bool parsing = true;
            bool branched = false;
            int branchBytes = 0;

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
                        Items.Add(new Padding(padding));
                        if (branched)
                        {
                            branchBytes -= (int) padding;

                            if (branchBytes <= 0)
                            {
                                branched = false;
                                branchBytes = 0;
                            }
                        }
                    }
                }

                InstructionBase instruction = null;
                List<Value> parameters = new List<Value>();

                switch (type)
                {
                    case InstructionType.ToNumber:
                        instruction = new ToNumber();
                        break;
                    case InstructionType.NextFrame:
                        instruction = new NextFrame();
                        break;
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
                    case InstructionType.StringEquals:
                        instruction = new StringEquals();
                        break;
                    case InstructionType.Pop:
                        instruction = new Pop();
                        break;
                    case InstructionType.ToInteger:
                        instruction = new ToInteger();
                        break;
                    case InstructionType.GetVariable:
                        instruction = new GetVariable();
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
                    case InstructionType.SetProperty:
                        instruction = new SetProperty();
                        break;
                    case InstructionType.Trace:
                        instruction = new Trace();
                        break;
                    case InstructionType.Delete:
                        instruction = new Delete();
                        break;
                    case InstructionType.Delete2:
                        instruction = new Delete2();
                        break;
                    case InstructionType.DefineLocal:
                        instruction = new DefineLocal();
                        break;
                    case InstructionType.CallFunction:
                        instruction = new CallFunction();
                        break;
                    case InstructionType.Return:
                        instruction = new Return();
                        break;
                    case InstructionType.NewObject:
                        instruction = new NewObject();
                        break;
                    case InstructionType.InitArray:
                        instruction = new InitArray();
                        break;
                    case InstructionType.InitObject:
                        instruction = new InitObject();
                        break;
                    case InstructionType.TypeOf:
                        instruction = new InitObject();
                        break;
                    case InstructionType.Add2:
                        instruction = new Add2();
                        break;
                    case InstructionType.LessThan2:
                        instruction = new LessThan2();
                        break;
                    case InstructionType.Equals2:
                        instruction = new Equals2();
                        break;
                    case InstructionType.ToString:
                        instruction = new ToString();
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
                    case InstructionType.Increment:
                        instruction = new Increment();
                        break;
                    case InstructionType.Decrement:
                        instruction = new Decrement();
                        break;
                    case InstructionType.CallMethod:
                        instruction = new CallMethod();
                        break;
                    case InstructionType.Enumerate2:
                        instruction = new Enumerate2();
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
                    case InstructionType.EA_CallFunc:
                        instruction = new CallFunc();
                        break;
                    case InstructionType.EA_CallMethodPop:
                        instruction = new CallMethodPop();
                        break;
                    case InstructionType.BitwiseXOr:
                        instruction = new BitwiseXOr();
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
                    case InstructionType.EA_PushNull:
                        instruction = new PushNull();
                        break;
                    case InstructionType.EA_PushUndefined:
                        instruction = new PushUndefined();
                        break;
                    case InstructionType.GotoFrame:
                        instruction = new GotoFrame();
                        parameters.Add(Value.FromInteger(_reader.ReadInt32()));
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
                    case InstructionType.DefineFunction2:
                        {
                            instruction = new DefineFunction2();
                            var name = _reader.ReadStringAtOffset();
                            var nParams = _reader.ReadUInt32();
                            var nRegisters = _reader.ReadByte();
                            var flags = _reader.ReadUInt24();

                            //list of parameter strings
                            var paramList = _reader.ReadFixedSizeListAtOffset<FunctionArgument>(() => new FunctionArgument()
                            {
                                Register = _reader.ReadInt32(),
                                Parameter = _reader.ReadStringAtOffset(),
                            },nParams);

                            parameters.Add(Value.FromString(name));
                            parameters.Add(Value.FromInteger((int)nParams));
                            parameters.Add(Value.FromInteger((int)nRegisters));
                            parameters.Add(Value.FromInteger((int)flags));
                            foreach (var param in paramList)
                            {
                                parameters.Add(Value.FromInteger(param.Register));
                                parameters.Add(Value.FromString(param.Parameter));
                            }
                            //body size of the function
                            parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                            //skip 8 bytes
                            _reader.ReadUInt64();
                        }
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
                    case InstructionType.BranchAlways:
                        instruction = new BranchAlways();
                        if (!branched)
                        {
                            branchBytes = _reader.ReadInt32();
                            parameters.Add(Value.FromInteger(branchBytes));

                            if (branchBytes > 0)
                            {
                                branchBytes += (int) instruction.Size + 1;
                                branched = true;
                            }
                        }
                        else
                        {
                            parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                        }
                        break;
                    case InstructionType.GetURL2:
                        instruction = new GetUrl2();
                        break;
                    case InstructionType.DefineFunction:
                        {
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
                        }
                        break;
                    case InstructionType.BranchIfTrue:
                        instruction = new BranchIfTrue();
                        if (!branched)
                        {
                            branchBytes = _reader.ReadInt32();
                            parameters.Add(Value.FromInteger(branchBytes));

                            if (branchBytes > 0)
                            {
                                branchBytes += (int) instruction.Size + 1;
                                branched = true;
                            }
                        }
                        else
                        {
                            parameters.Add(Value.FromInteger(_reader.ReadInt32()));
                        }
                        break;
                    case InstructionType.GotoFrame2:
                        instruction = new GotoFrame2();
                        parameters.Add(Value.FromInteger(_reader.ReadByte()));
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
                    case InstructionType.EA_SetStringVar:
                        instruction = new SetStringMember();
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
                    case InstructionType.EA_CallNamedFuncPop:
                        instruction = new CallNamedFuncPop();
                        parameters.Add(Value.FromConstant(_reader.ReadByte()));
                        break;
                    case InstructionType.EA_CallNamedFunc:
                        instruction = new CallNamedFunc();
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
                    case InstructionType.EA_PushShort:
                        instruction = new PushShort();
                        parameters.Add(Value.FromInteger(_reader.ReadUInt16()));
                        break;
                    case InstructionType.End:
                        instruction = new End();

                        if (!branched)
                            parsing = false;
                        break;
                    case InstructionType.EA_CallNamedMethod:
                        instruction = new CallNamedMethod();
                        parameters.Add(Value.FromConstant(_reader.ReadByte()));
                        break;
                    case InstructionType.Var:
                        instruction = new Var();

                        break;
                    case InstructionType.EA_PushRegister:
                        instruction = new PushRegister();
                        //code at `case InstructionType.SetRegister` used _reader.ReadInt32() to read register number
                        //but if I use that here, I will end up with unknown instruction values.
                        //so it looks like here it read byte instead of Int32. - Lanyi
                        parameters.Add(Value.FromInteger(_reader.ReadByte()));
                        break;
                    case InstructionType.EA_PushConstantWord:
                        instruction = new PushConstantWord();
                        parameters.Add(Value.FromConstant(_reader.ReadUInt16()));
                        break;
                    case InstructionType.EA_CallFuncPop:
                        instruction = new CallFunctionPop();
                        break;
                    case InstructionType.StrictEqual:
                        instruction = new StrictEquals();
                        break;
                    default:
                        throw new InvalidDataException("Unimplemented bytecode instruction:" + type.ToString());
                }

                if (instruction != null)
                {
                    instruction.Parameters = parameters;
                    Items.Add(instruction);
                }

                if (branched)
                {
                    branchBytes -= (int) instruction.Size + 1;

                    if (branchBytes <= 0)
                    {
                        branched = false;
                    }
                }
            }
            _reader.BaseStream.Seek(current, SeekOrigin.Begin);
        }
    }
}
