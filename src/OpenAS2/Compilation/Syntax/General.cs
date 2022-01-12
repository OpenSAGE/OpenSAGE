using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenAS2.Base;
using OpenAS2.Runtime;
using OpenAS2.Compilation;

namespace OpenAS2.Compilation.Syntax
{
    public static class General
    {
        public static bool ExecuteBinInput(SyntaxNodePool np, RawInstruction inst)
        {
            var a = np.PopExpression();
            var b = np.PopExpression();
            SNExpression res = null;
            switch (inst.Type)
            {
                case InstructionType.Equals:
                case InstructionType.Equals2:
                case InstructionType.StringEquals:
                    // TODO string equals may differ from other 2
                    res = OprUtils.Equality(a, b);
                    break;
                case InstructionType.StrictEquals:
                    res = OprUtils.StrictEquality(a, b);
                    break;


                case InstructionType.LessThan:
                case InstructionType.LessThan2:
                case InstructionType.StringCompare:
                    res = OprUtils.LessThan(b, a);
                    break;
                case InstructionType.Greater:
                case InstructionType.StringGreater:
                    res = OprUtils.GreaterThan(b, a);
                    break;


                case InstructionType.Add:
                case InstructionType.Add2:
                case InstructionType.StringConcat:
                    res = OprUtils.Addition(b, a);
                    break;
                case InstructionType.Subtract:
                    res = OprUtils.Subtraction(b, a);
                    break;
                case InstructionType.Multiply:
                    res = OprUtils.Multiplication(b, a);
                    break;
                case InstructionType.Divide:
                    res = OprUtils.Division(b, a);
                    break;
                case InstructionType.Modulo:
                    res = OprUtils.Remainder(b, a);
                    break;

                case InstructionType.LogicalAnd:
                    res = OprUtils.LogicalAnd(b, a);
                    break;
                case InstructionType.LogicalOr:
                    res = OprUtils.LogicalOr(b, a);
                    break;
                case InstructionType.BitwiseAnd:
                    res = OprUtils.BitwiseAnd(b, a);
                    break;
                case InstructionType.BitwiseOr:
                    res = OprUtils.BitwiseOr(b, a);
                    break;
                case InstructionType.BitwiseXOr:
                    res = OprUtils.BitwiseXor(b, a);
                    break;

                case InstructionType.ShiftLeft:
                    res = OprUtils.BitwiseLeftShift(b, a);
                    break;
                case InstructionType.ShiftRight:
                    res = OprUtils.BitwiseRightShift(b, a);
                    break;
                case InstructionType.ShiftRight2:
                    res = OprUtils.BitwiseUnsignedRightShift(b, a);
                    break;

                default:
                    return false;
            }
            np.PushNode(res);
            return true;
        }

        public static bool ExecuteUnInput(SyntaxNodePool np, RawInstruction inst)
        {
            var a = np.PopExpression();
            SNExpression res = null;
            switch (inst.Type)
            {
                case InstructionType.LogicalNot:
                    res = OprUtils.LogicalNot(a);
                    break;

                case InstructionType.Increment:
                    res = OprUtils.PrefixIncrement(a);
                    break;
                case InstructionType.Decrement:
                    res = OprUtils.PrefixDecrement(a);
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
                    res = OprUtils.FunctionCall(
                        new SNNominator("parseInt"),
                        new SNArray(new SNExpression[] {
                            a
                        }));
                    break;
                case InstructionType.ToNumber:
                    res = OprUtils.FunctionCall(
                        new SNNominator("Number"),
                        new SNArray(new SNExpression[] {
                            a
                        }));
                    break;
                case InstructionType.ToString:
                    res = OprUtils.FunctionCall(
                        new SNNominator("String"),
                        new SNArray(new SNExpression[] {
                            a
                        }));
                    break;

                case InstructionType.Random:
                    res = OprUtils.FunctionCall(
                        new SNNominator("random"),
                        new SNArray(new SNExpression[] {
                            a
                        }));
                    break;

                default:
                    return false;
            }
            np.PushNode(res);
            return true;
        }

        public static bool ExecuteStackRegOpr(SyntaxNodePool np, RawInstruction inst)
        {
            switch (inst.Type)
            {
                case InstructionType.SetRegister:
                    var val = np.PopExpression(deleteIfPossible: false);
                    var reg = inst.Parameters[0].Integer;
                    np.SetRegister(reg, val);
                    break;
                case InstructionType.StackSwap:
                    return false;
                case InstructionType.Pop:
                    np.PopExpression();
                    break;
                case InstructionType.PushDuplicate:
                    np.PushNode(np.PopExpression(deleteIfPossible: false));
                    break;
                case InstructionType.PushData:
                    foreach (var constant in inst.Parameters.Skip(1))
                        np.PushNodeConstant(constant.Integer);
                    break;

                case InstructionType.EA_PushUndefined:
                    np.PushNode(new SNLiteralUndefined());
                    break;
                case InstructionType.EA_PushNull:
                    np.PushNode(new SNLiteral(null));
                    break;
                case InstructionType.EA_PushFalse:
                    np.PushNode(new SNLiteral(RawValue.FromBoolean(false)));
                    break;
                case InstructionType.EA_PushTrue:
                    np.PushNode(new SNLiteral(RawValue.FromBoolean(true)));
                    break;
                case InstructionType.EA_PushZero:
                    np.PushNode(new SNLiteral(RawValue.FromInteger(0)));
                    break;
                case InstructionType.EA_PushOne:
                    np.PushNode(new SNLiteral(RawValue.FromInteger(1)));
                    break;

                case InstructionType.EA_PushByte:
                case InstructionType.EA_PushShort:
                case InstructionType.EA_PushLong:
                case InstructionType.EA_PushFloat:
                case InstructionType.EA_PushString:
                    np.PushNode(new SNLiteral(inst.Parameters[0]));
                    break;

                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_PushConstantWord:
                    np.PushNodeConstant(inst.Parameters[0].Integer);
                    break;

                case InstructionType.EA_PushRegister:
                    np.PushNodeRegister(inst.Parameters[0].Integer);
                    break;

                default:
                    return false;
            }
            return true;
        }

        public static bool Parse(SyntaxNodePool np, RawInstruction inst)
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

                    return ExecuteBinInput(np, inst);

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

                    return ExecuteUnInput(np, inst);

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

                    return ExecuteStackRegOpr(np, inst);

                default:

                    return false;
            }
        }
    }
}
