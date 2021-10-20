using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using OpenSage.FileFormats;
using OpenSage.FileFormats.Apt.ActionScript;
using OpenSage.Gui.Apt.ActionScript.Opcodes;

namespace OpenSage.Gui.Apt.ActionScript
{
    public sealed class InstructionCollection
    {
        private readonly SortedList<int, InstructionBase> _instructions;

        public int Count => _instructions.Count;

        public InstructionCollection(SortedList<int, InstructionBase> instructions)
        {
            _instructions = instructions;
        }

        public IReadOnlyCollection<KeyValuePair<int, InstructionBase>> GetPositionedInstructions()
        {
            return _instructions;
        }

        public ReadOnlyCollection<InstructionBase> GetInstructions()
        {
            return new ReadOnlyCollection<InstructionBase>(_instructions.Values);
        }

        public InstructionCollection AddEnd()
        {
            if (_instructions.Values.Last().Type == InstructionType.End)
                return this;
            var _inst = new SortedList<int, InstructionBase>(_instructions);
            _inst[_inst.Keys.Last() + 1] = new End();
            return new InstructionCollection(_inst);
        }

        public static InstructionCollection Native(Action<ExecutionContext> act)
        {
            var inst = new ExecNativeCode(act);
            var insts = new SortedList<int, InstructionBase> { [0] = inst, };
            return new InstructionCollection(insts);
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

        public InstructionBase GetInstructionByIndex(int index)
        {
            return _instructions.Values[index];
        }

        public InstructionBase GetInstructionByPosition(int position)
        {
            return _instructions[position];
        }

        public static InstructionCollection Parse(InstructionStorage insts)
        {

            var codes = insts.GetPositionedInstructions();
            var instructions = new SortedList<int, InstructionBase>();
            {
                
                foreach(var kvp in codes)
                {
                    var instructionPosition = kvp.Key;
                    var type = kvp.Value.Type;
                    var parameters = kvp.Value.Parameters;

                    InstructionBase instruction = null;
                    
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
                        case InstructionType.BitwiseAnd:
                            instruction = new BitwiseAnd();
                            break;
                        case InstructionType.BitwiseOr:
                            instruction = new BitwiseOr();
                            break;
                        case InstructionType.BitwiseXOr:
                            instruction = new BitwiseXOr();
                            break;
                        case InstructionType.Greater:
                            instruction = new Greater();
                            break;
                        case InstructionType.LessThan:
                            instruction = new LessThan();
                            break;
                        case InstructionType.LogicalAnd:
                            instruction = new LogicalAnd();
                            break;
                        case InstructionType.LogicalOr:
                            instruction = new LogicalOr();
                            break;
                        case InstructionType.LogicalNot:
                            instruction = new LogicalNot();
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
                        case InstructionType.CloneSprite:
                            instruction = new CloneSprite(); // NIE DOM
                            break;
                        case InstructionType.RemoveSprite:
                            instruction = new RemoveSprite(); // NIE DOM
                            break;
                        case InstructionType.Trace:
                            instruction = new Trace();
                            break;
                        case InstructionType.Random:
                            instruction = new RandomNumber();
                            break;
                        case InstructionType.Return:
                            instruction = new Return();
                            break;
                        case InstructionType.Modulo:
                            instruction = new Modulo();
                            break;
                        case InstructionType.TypeOf:
                            instruction = new TypeOf(); // TODO UINT Problem
                            break;
                        case InstructionType.Add2:
                            instruction = new Add2(); // TODO Type Conversion Problem
                            break;
                        case InstructionType.LessThan2:
                            instruction = new LessThan2(); // TODO Type Conversion Problem
                            break;
                        case InstructionType.ToString:
                            instruction = new ToStringOpCode();
                            break;
                        case InstructionType.PushDuplicate:
                            instruction = new PushDuplicate();
                            break;
                        case InstructionType.Increment:
                            instruction = new Increment();
                            break;
                        case InstructionType.Decrement:
                            instruction = new Decrement();
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
                            instruction = new GotoFrame(); // TODO need research
                            break;
                        case InstructionType.GetURL:
                            instruction = new GetUrl();
                            break;
                        case InstructionType.SetRegister:
                            instruction = new SetRegister();
                            break;
                        case InstructionType.GotoLabel:
                            instruction = new GotoLabel();
                            break;
                        case InstructionType.PushData: // NIE doubtful and not used in any games
                            throw new NotImplementedException();
                            instruction = new PushData();
                            break;
                        case InstructionType.BranchAlways:
                            instruction = new BranchAlways();
                            break;
                        case InstructionType.GetURL2:
                            instruction = new GetUrl2();
                            break;
                        // OOP Related
                        case InstructionType.ConstantPool:
                            instruction = new ConstantPool();
                            break;
                        case InstructionType.DefineFunction2: // TODO Flags?
                            instruction = new DefineFunction2();
                            break;
                        case InstructionType.DefineFunction:
                            instruction = new DefineFunction();
                            break;
                        case InstructionType.CallFunction:
                            instruction = new CallFunction();
                            break;
                        case InstructionType.EA_CallFunc:
                            instruction = new CallFunc(); // NIE don't know the difference, plan to implement when encountered
                            break;
                        case InstructionType.EA_CallFuncPop:
                            instruction = new CallFunctionPop();
                            break;
                        case InstructionType.CallMethod:
                            instruction = new CallMethod();
                            break;
                        case InstructionType.EA_CallMethod:
                            instruction = new EACallMethod();
                            break;
                        case InstructionType.EA_CallMethodPop:
                            instruction = new CallMethodPop();
                            break;
                        case InstructionType.DefineLocal:
                            instruction = new DefineLocal();
                            break;
                        case InstructionType.Var:
                            instruction = new DefineLocal2();
                            break;
                        case InstructionType.Delete:
                            instruction = new Delete();
                            break;
                        case InstructionType.Delete2:
                            instruction = new Delete2();
                            break;
                        case InstructionType.Enumerate2:
                            instruction = new Enumerate2();
                            break;
                        case InstructionType.Equals2:
                            instruction = new Equals2(); // TODO Follow ECMA-262 #11.9.3
                            break;
                        case InstructionType.GetMember:
                            instruction = new GetMember();
                            break;
                        case InstructionType.SetMember:
                            instruction = new SetMember();
                            break;
                        case InstructionType.InitArray:
                            instruction = new InitArray();
                            break;
                        case InstructionType.InitObject:
                            instruction = new InitObject();
                            break;
                        case InstructionType.NewMethod:
                            instruction = new NewMethod(); // TODO not sure if correct, the document is vague
                            break;
                        case InstructionType.NewObject:
                            instruction = new NewObject(); // TODO replace stack representations
                            break;


                        case InstructionType.BranchIfTrue:
                            instruction = new BranchIfTrue();
                            break;
                        case InstructionType.GotoFrame2:
                            instruction = new GotoFrame2();
                            break;
                        case InstructionType.EA_PushString:
                            instruction = new PushString();
                            break;
                        case InstructionType.EA_PushConstantByte:
                            instruction = new PushConstantByte();
                            break;
                        case InstructionType.EA_GetStringVar:
                            instruction = new GetStringVar();
                            break;
                        case InstructionType.EA_SetStringVar:
                            instruction = new SetStringVar();
                            break;
                        case InstructionType.EA_GetStringMember:
                            instruction = new GetStringMember();
                            break;
                        case InstructionType.EA_SetStringMember:
                            instruction = new SetStringMember();
                            break;
                        case InstructionType.EA_PushValueOfVar:
                            instruction = new PushValueOfVar();
                            break;

                        case InstructionType.EA_GetNamedMember:
                            instruction = new GetNamedMember();
                            break;
                        case InstructionType.EA_CallNamedFuncPop:
                            instruction = new CallNamedFuncPop();
                            break;
                        case InstructionType.EA_CallNamedFunc:
                            instruction = new CallNamedFunc();
                            break;
                        case InstructionType.EA_CallNamedMethodPop:
                            instruction = new CallNamedMethodPop();
                            break;
                        case InstructionType.EA_CallNamedMethod: 
                            instruction = new CallNamedMethod();
                            break;

                        case InstructionType.EA_PushFloat:
                            instruction = new PushFloat();
                            break;
                        case InstructionType.EA_PushByte:
                            instruction = new PushByte();
                            break;
                        case InstructionType.EA_PushShort:
                            instruction = new PushShort();
                            break;
                        case InstructionType.EA_PushLong: // TODO follow ECMA-262
                            instruction = new PushLong();
                            break;
                        case InstructionType.End: 
                            instruction = new End();
                            break;
                        case InstructionType.EA_PushRegister:
                            instruction = new PushRegister();
                            break;
                        case InstructionType.EA_PushConstantWord:
                            instruction = new PushConstantWord();
                            break;
                        case InstructionType.StrictEqual:
                            instruction = new StrictEquals();
                            break;
                        case InstructionType.Extends:
                            instruction = new Extends();
                            break;
                        case InstructionType.InstanceOf:
                            instruction = new InstanceOf();
                            break;
                        case InstructionType.ImplementsOp:
                            instruction = new ImplementsOp(); // NIE what is the interface list of an object?
                            break;
                        case InstructionType.CastOp:
                            instruction = new CastOp(); 
                            break;
                        case InstructionType.GetTime:
                            instruction = new GetTime();
                            break;

                        default:
                            throw new InvalidDataException("Unimplemented bytecode instruction:" + type.ToString());
                    }

                    if (instruction != null)
                    {
                        instruction.Parameters = new();
                        for (int i = 0; i < parameters.Count; ++i)
                            instruction.Parameters.Add(Value.FromStorage(parameters[i]));
                        instructions.Add(instructionPosition, instruction);
                    }
                }
            }

            return new InstructionCollection(instructions);
        }
    }
}
