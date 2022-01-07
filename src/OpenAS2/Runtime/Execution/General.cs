using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenAS2.Base;
using OpenAS2.Runtime;

namespace OpenAS2.Runtime.Execution
{
    public static class General
    {
        public static bool ExecuteBinInput(ExecutionContext context, Instruction inst)
        {
            var a = context.Pop();
            var b = context.Pop();
            Value res = null;
            switch (inst.Type)
            {
                case InstructionType.Equals:
                    res = Value.FromBoolean(Value.NaiveEquals(a, b));
                    break;
                case InstructionType.Equals2:
                    res = Value.FromBoolean(Value.AbstractEquals(a, b, context));
                    break;
                case InstructionType.StringEquals:
                    res = Value.FromBoolean(b.ToString() == a.ToString());
                    break;
                case InstructionType.StrictEquals:
                    res = Value.FromBoolean(Value.StrictEquals(a, b, context));
                    break;


                case InstructionType.LessThan:
                    var arg1 = a.ToFloat();
                    var arg2 = b.ToFloat();

                    if (double.IsNaN(arg1)) arg1 = 0;
                    if (double.IsNaN(arg2)) arg2 = 0;

                    bool result = arg2 < arg1;
                    res = Value.FromBoolean(result);
                    break;
                case InstructionType.LessThan2:
                    res = Value.AbstractLess(b, a);
                    break;
                case InstructionType.StringCompare:
                    res = Value.FromBoolean(string.Compare(b.ToString(), a.ToString()) < 0);
                    break;
                case InstructionType.Greater:
                    res = Value.AbstractLess(a, b);
                    break;
                case InstructionType.StringGreater:
                    res = Value.FromBoolean(string.Compare(b.ToString(), a.ToString()) > 0);
                    break;


                case InstructionType.Add:
                    res = Value.FromFloat(b.ToFloat() + a.ToFloat());
                    break;
                case InstructionType.Subtract:
                    res = Value.FromFloat(b.ToFloat() - a.ToFloat());
                    break;
                case InstructionType.Multiply:
                    res = Value.FromFloat(b.ToFloat() * a.ToFloat());
                    break;
                case InstructionType.Divide:
                    var af = a.ToFloat();
                    var bf = b.ToFloat();
                    res = af != 0 ? Value.FromFloat(bf / af) : Value.FromFloat(float.NaN);
                    break;
                case InstructionType.Modulo:
                    res = Value.FromFloat(a.ToFloat() % b.ToFloat());
                    break;

                // The additive operator follows https://262.ecma-international.org/5.1/#sec-11.6.1
                case InstructionType.Add2:
                    res = (a.IsNumber() && b.IsNumber()) ?
                            Value.FromFloat(b.ToFloat() + a.ToFloat()) :
                            Value.FromString(b.ToString() + a.ToString());
                    break;
                case InstructionType.StringConcat:
                    res = Value.FromString(b.ToString() + a.ToString());
                    break;

                case InstructionType.LogicalAnd:
                    res = Value.FromBoolean(b.ToBoolean2() && a.ToBoolean2());
                    break;
                case InstructionType.LogicalOr:
                    res = Value.FromBoolean(b.ToBoolean2() || a.ToBoolean2());
                    break;
                case InstructionType.BitwiseAnd:
                    res = Value.FromInteger(a.ToInteger() & b.ToInteger());
                    break;
                case InstructionType.BitwiseOr:
                    res = Value.FromInteger(a.ToInteger() | b.ToInteger());
                    break;
                case InstructionType.BitwiseXOr:
                    res = Value.FromInteger(a.ToInteger() ^ b.ToInteger());
                    break;

                case InstructionType.ShiftLeft:
                    res = Value.FromInteger((b.ToInteger()) << (a.ToInteger() & 0b11111));
                    break;
                case InstructionType.ShiftRight:
                    res = Value.FromInteger((b.ToInteger()) >> (a.ToInteger() & 0b11111));
                    break;
                case InstructionType.ShiftRight2:
                    res = Value.FromInteger((int) (((uint) b.ToInteger()) >> (a.ToInteger() & 0b11111)));
                    break;

                default:
                    return false;
            }
            context.Push(res);
            return true;
        }

        public static bool ExecuteUnInput(ExecutionContext context, Instruction inst)
        {
            var a = context.Pop();
            Value res = null;
            switch (inst.Type)
            {
                case InstructionType.LogicalNot:
                    res = Value.FromBoolean(!a.ToBoolean2());
                    break;

                case InstructionType.Increment:
                    res = Value.FromInteger(a.ToInteger() + 1);
                    break;
                case InstructionType.Decrement:
                    res = Value.FromInteger(a.ToInteger() - 1);
                    break;

                case InstructionType.Ord:
                    return false; // TODO
                case InstructionType.Chr:
                    return false; // TODO
                case InstructionType.StringLength:
                    return false; // TODO
                case InstructionType.SubString:
                    return false; // TODO


                case InstructionType.MbOrd:
                    return false; // TODO
                case InstructionType.MbChr:
                    return false; // TODO
                case InstructionType.MbLength:
                    return false; // TODO
                case InstructionType.MbSubString:
                    return false; // TODO

                case InstructionType.ToInteger:
                    res = Value.FromInteger(a.ToInteger());
                    break;
                case InstructionType.ToNumber:
                    res = a.ToNumber(context);
                    break;
                case InstructionType.ToString:
                    res = Value.FromString(a.ToString());
                    break;

                case InstructionType.Random:
                    res = Value.FromInteger(new Random().Next(0, a.ToInteger()));
                    break;

                default:
                    return false;
            }
            context.Push(res);
            return true;
        }

        public static bool ExecuteStackRegOpr(ExecutionContext context, Instruction inst)
        {
            switch (inst.Type)
            {
                case InstructionType.SetRegister:
                    var val = context.Peek();
                    var reg = inst.Parameters[0].ToInteger();
                    context.SetRegister(reg, val);
                    break;
                case InstructionType.StackSwap:
                    context.SwapStack();
                    break;
                case InstructionType.Pop:
                    context.Pop();
                    break;
                case InstructionType.PushDuplicate:
                    context.Push(context.Peek());
                    break;
                case InstructionType.PushData:
                    foreach (var constant in inst.Parameters.Skip(1))
                        context.Push(constant.ResolveConstant(context));
                    break;

                case InstructionType.EA_PushUndefined:
                    context.Push(Value.Undefined());
                    break;
                case InstructionType.EA_PushNull:
                    context.Push(Value.Null());
                    break;
                case InstructionType.EA_PushFalse:
                    context.Push(Value.FromBoolean(false));
                    break;
                case InstructionType.EA_PushTrue:
                    context.Push(Value.FromBoolean(true));
                    break;
                case InstructionType.EA_PushZero:
                    context.Push(Value.FromInteger(0));
                    break;
                case InstructionType.EA_PushOne:
                    context.Push(Value.FromInteger(1));
                    break;

                case InstructionType.EA_PushByte:
                case InstructionType.EA_PushShort:
                case InstructionType.EA_PushLong:
                case InstructionType.EA_PushFloat:
                case InstructionType.EA_PushString:
                    context.Push(inst.Parameters[0]);
                    break;

                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_PushConstantWord:
                    context.PushConstant(inst.Parameters[0]);
                    break;

                case InstructionType.EA_PushRegister:
                    context.Push(inst.Parameters[0].ResolveRegister(context));
                    break;

                default:
                    return false;
            }
            return true;
        }

        public static bool Execute(ExecutionContext context, Instruction inst)
        {
            switch (inst.Type)
            {
                case InstructionType.Equals:
                case InstructionType.Equals2:
                case InstructionType.StringEquals:
                case InstructionType.StrictEquals:

                case InstructionType.LessThan:
                case InstructionType.LessThan2:
                case InstructionType.StringCompare:
                case InstructionType.Greater:
                case InstructionType.StringGreater:

                case InstructionType.Add:
                case InstructionType.Subtract:
                case InstructionType.Multiply:
                case InstructionType.Divide:
                case InstructionType.Modulo:

                case InstructionType.Add2:
                case InstructionType.StringConcat:

                case InstructionType.LogicalAnd:
                case InstructionType.LogicalOr:
                case InstructionType.BitwiseAnd:
                case InstructionType.BitwiseOr:
                case InstructionType.BitwiseXOr:

                case InstructionType.ShiftLeft:
                case InstructionType.ShiftRight:
                case InstructionType.ShiftRight2:

                    return ExecuteBinInput(context, inst);

                case InstructionType.LogicalNot:

                case InstructionType.Increment:
                case InstructionType.Decrement:

                case InstructionType.Ord:
                case InstructionType.Chr:
                case InstructionType.StringLength:
                case InstructionType.SubString:

                case InstructionType.MbOrd:
                case InstructionType.MbChr:
                case InstructionType.MbLength:
                case InstructionType.MbSubString:


                case InstructionType.ToInteger:
                case InstructionType.ToNumber:
                case InstructionType.ToString:

                case InstructionType.Random:

                    return ExecuteUnInput(context, inst);

                case InstructionType.SetRegister:
                case InstructionType.StackSwap:
                case InstructionType.Pop:
                case InstructionType.PushDuplicate:
                case InstructionType.PushData:

                case InstructionType.EA_PushUndefined:
                case InstructionType.EA_PushNull:
                case InstructionType.EA_PushFalse:
                case InstructionType.EA_PushTrue:
                case InstructionType.EA_PushZero:
                case InstructionType.EA_PushOne:
                case InstructionType.EA_PushByte:
                case InstructionType.EA_PushShort:
                case InstructionType.EA_PushLong:
                case InstructionType.EA_PushFloat:
                case InstructionType.EA_PushString:
                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_PushConstantWord:
                case InstructionType.EA_PushRegister:

                    return ExecuteStackRegOpr(context, inst);

                default:

                    return false;
            }
        }
    }
}
