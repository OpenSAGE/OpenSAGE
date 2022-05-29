using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenAS2.Base;

namespace OpenAS2.Compilation.Syntax
{
    public static class Control
    {
        public static bool Parse(SyntaxNodePool np, RawInstruction inst)
        {
            switch (inst.Type)
            {
                case InstructionType.End:
                    return true;
                case InstructionType.Padding:
                    return true;

                case InstructionType.Return:
                    np.PushNode(new SNKeyWord("return", np.PopExpression()));
                    return true;
                case InstructionType.Throw:
                    np.PushNode(new SNKeyWord("throw", np.PopExpression()));
                    return true;
                case InstructionType.ConstantPool:
                    np.Constants = CompilationUtils.CreateConstantPool(inst.Parameters, np.GlobalPool);
                    return true;

                case InstructionType.BranchAlways:
                    return true;
                case InstructionType.EA_BranchIfFalse:
                    np.PushNode(np.PopExpression());
                    return true;
                case InstructionType.BranchIfTrue:
                    np.PushNode(OprUtils.LogicalNot(np.PopExpression()));
                    return true;


                case InstructionType.DefineFunction:
                    break;
                case InstructionType.DefineFunction2:
                    break;
                case InstructionType.Try:
                    return false;
                case InstructionType.With:
                    return false;


                // do nothing at all
                case InstructionType.NextFrame:
                case InstructionType.PrevFrame:
                case InstructionType.Play:
                case InstructionType.Stop:
                case InstructionType.ToggleQuality:
                case InstructionType.StopSounds:
                    np.PushNode(new SNPlainCode(inst.Type.ToString() + "()"));
                    return true;


                case InstructionType.SetTarget:
                    break;
                case InstructionType.SetTarget2:
                    break;
                case InstructionType.TargetPath:
                    break;


                case InstructionType.CloneSprite:
                    break;
                case InstructionType.RemoveSprite:
                    break;


                case InstructionType.StartDragMovie:
                    break;
                case InstructionType.StopDragMovie:
                    break;


                case InstructionType.GotoLabel:
                    np.PushNode(new SNToStatement(OprUtils.FunctionCall(
                        new SNNominator("gotoLabel"),
                        new SNArray(new SNExpression[] {
                            new SNLiteral(inst.Parameters[0]),
                        }))));
                    return true;
                case InstructionType.GotoFrame:
                    np.PushNode(new SNToStatement(OprUtils.FunctionCall(
                        new SNNominator("gotoFrame"),
                        new SNArray(new SNExpression[] {
                            new SNLiteral(inst.Parameters[0]),
                        }))));
                    return true;
                case InstructionType.GotoFrame2:
                    np.PushNode(new SNToStatement(OprUtils.FunctionCall(
                        new SNNominator("gotoFrame"),
                        new SNArray(new SNExpression[] {
                            np.PopExpression(),
                            new SNLiteral(inst.Parameters[0]),
                        }))));
                    return true;
                case InstructionType.CallFrame:
                    break;
                case InstructionType.WaitFormFrame:
                    break;
                case InstructionType.WaitForFrameExpr:
                    break;


                // TODO will it push anything back to stack?
                case InstructionType.GetURL:
                    np.PushNode(new SNToStatement(OprUtils.FunctionCall(
                        new SNNominator("getUrl"),
                        new SNArray(new SNExpression[] {
                            new SNLiteral(inst.Parameters[0]),
                            new SNLiteral(inst.Parameters[1])
                        }))));
                    return true;
                case InstructionType.GetURL2:
                    var lv0 = np.PopExpression();
                    var lv1 = np.PopExpression();
                    np.PushNode(OprUtils.FunctionCall(
                        new SNNominator("loadVariables"),
                        new SNArray(new SNExpression[] {
                            lv1,
                            lv0
                        })));
                    return true;


                case InstructionType.GetTime:
                    np.PushNode(OprUtils.FunctionCall(
                        new SNNominator("getTimer"),
                        new SNArray(new SNExpression[] { })));
                    return true;


                case InstructionType.Trace:
                    var tracev0 = np.PopExpression();
                    np.PushNode(new SNToStatement(OprUtils.FunctionCall(
                        new SNNominator("trace"),
                        new SNArray(new SNExpression[] {
                            tracev0
                        }))));
                    return true;
                case InstructionType.TraceStart:
                    break;


                case InstructionType.GetProperty:
                    var gpv0 = np.PopExpression();
                    var gpv1 = np.PopExpression();
                    np.PushNode(new SNMemberAccess(new SNCheckTarget(SNNominator.Check(gpv1)), gpv0));
                    return true;
                case InstructionType.SetProperty:
                    var spv0 = np.PopExpression();
                    var spv1 = np.PopExpression();
                    var spv2 = np.PopExpression();
                    np.PushNode(new SNValAssign(new SNMemberAccess(new SNCheckTarget(SNNominator.Check(spv2)), spv1), spv0));
                    return true;


                case InstructionType.GetVariable:
                    np.PushNode(new SNCheckTarget(SNNominator.Check2(np.PopExpression())));
                    return true;
                case InstructionType.SetVariable: // TODO
                    var svv0 = new SNCheckTarget(SNNominator.Check2(np.PopExpression()));
                    var svv1 = np.PopExpression();
                    np.PushNode(new SNValAssign(svv1, svv0));
                    return true;
            }
            return false;
        }
    }
}
