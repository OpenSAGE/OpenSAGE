using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenAS2.Base;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Compilation.Syntax
{

    using OPR1 = Func<SNExpression, SNExpression>;
    using OPR2 = Func<SNExpression, SNExpression, SNExpression>;
    using OPR1A = Action<SNExpression>;
    using OPR2A = Action<SNExpression, SNExpression>;

    public static class ObjectOriented
    {

        static void ExecUnaryOprOnStack(this SyntaxNodePool np, OPR1 opr)
        {
            np.PushNode(opr(np.PopExpression()));
        }

        static void ExecBinaryOprOnStack(this SyntaxNodePool np, OPR2 opr)
        {
            var a = np.PopExpression();
            var b = np.PopExpression();
            np.PushNode(opr(a, b));
        }

        static readonly OPR1 TypeOf = (v) => OprUtils.TypeOf(v);
        static readonly OPR2 CastOp = (objv, cstv) => OprUtils.Cast(objv, cstv);
        static readonly OPR2 InstanceOf = (a, b) => OprUtils.InstanceOf(b, a);

        static void DoExtends(SyntaxNodePool np) // this should work
        {
            var sup = np.PopExpression();
            var cls = np.PopExpression();
            np.PushNode(new SNToStatement(new SNBinary(24, SNOperator.Order.NotAcceptable, "{0} extends {1}", cls, sup)));
        }

        static void DoImplements(SyntaxNodePool np) // this should work
        {
            var cst = np.PopExpression();
            var cls = np.PopArray();
            np.PushNode(new SNToStatement(new SNBinary(24, SNOperator.Order.NotAcceptable, "{0} implements {1}", cst, cls)));
        }

        // all sorts of null check...
        public static void DoNewMethod(SyntaxNodePool np)
        {
            var nameVal = np.PopExpression();
            var obj = np.PopExpression();
            var args = np.PopArray();
            var ret = OprUtils.New(new SNMemberAccess(SNNominator.Check(nameVal), obj), args);
            // np.PushNode(new SNMarkNomination(ret));
            np.PushNode(ret);
        }
        public static void DoNewObject(SyntaxNodePool np)
        {
            var name = np.PopExpression();
            var args = np.PopArray();
            var ret = OprUtils.New(SNNominator.Check(name), args);
            // np.PushNode(new SNMarkNomination(ret));
            np.PushNode(ret);
        }
        public static void DoInitObject(SyntaxNodePool np)
        {
            var args = np.PopArray(readPair: true);
            var ret = OprUtils.New(new SNNominator("Object"), args);
            // np.PushNode(new SNMarkNomination(ret));
            np.PushNode(ret);
        }
        public static void DoInitArray(SyntaxNodePool np)
        {
            var ret = np.PopArray();
            // np.PushNode(new SNMarkNomination(ret));
            np.PushNode(ret);
        }

        public static void DoDefineLocal(SyntaxNodePool np)
        {
            var value = np.PopExpression();
            var varName = np.PopExpression();
            np.PushNode(new SNValAssign(varName, value, true));
        }
        public static void DoDefineLocal2(SyntaxNodePool np)
        {
            var varName = np.PopExpression();
            np.PushNode(new SNValAssign(varName, OprUtils.LogicalOr(varName, new SNLiteralUndefined()), true));
        }
        public static void DoDelete2(SyntaxNodePool np)
        {
            np.PushNode(new SNKeyWord("delete", SNNominator.Check(np.PopExpression())));
        }
        public static readonly OPR2 GetMember = (member, vobj) => new SNMemberAccess(new SNCheckTarget(SNNominator.Check(vobj)), member);

        public static void DoSetMember(SyntaxNodePool np)
        {
            //pop the value
            var valueVal = np.PopExpression();
            //pop the member name
            var memberName = np.PopExpression();
            //pop the object
            var obj = np.PopExpression();

            np.PushNode(new SNValAssign(new SNMemberAccess(SNNominator.Check(obj), memberName), valueVal));
        }
        public static void DoDelete(SyntaxNodePool np)
        {
            var property = np.PopExpression();
            var target = np.PopExpression();// TODO wtf? np.GetTarget(np.PopExpression().ToString());
            np.PushNode(new SNKeyWord("delete", new SNMemberAccess(new SNCheckTarget(SNNominator.Check(target)), property)));
        }



        public static void DealWithEACall(SyntaxNodePool np, SNExpression exp, int state)
        {
            if (state == 1) // EA_****
            {
                var varName = np.PopExpression();
                np.PushNode(new SNValAssign(SNNominator.Check(varName), exp));
            }
            else if (state == 2) // EA_****Pop
            {
                np.PushNode(new SNToStatement(exp));
            }
            else
            {
                np.PushNode(exp);
            }
        }

        public static void DoCallFunction(SyntaxNodePool np, int state)
        {
            var funcName = SNNominator.Check(np.PopExpression());
            var args = np.PopArray();
            var fc = OprUtils.FunctionCall(funcName, args);
            DealWithEACall(np, fc, state);
        }

        public static void DoCallMethod(SyntaxNodePool np, int state)
        {
            var funcName = np.PopExpression();
            var flag = funcName is SNLiteralUndefined;
            var funcObj = np.PopExpression();
            var args = np.PopArray();
            var fbody = flag ? funcObj : new SNMemberAccess(SNNominator.Check(funcObj), funcName);
            DealWithEACall(np, OprUtils.FunctionCall(fbody, args), state);
        }
        public static void DoCallNamedFunc(SyntaxNodePool np, int cid, int state)
        {
            np.PushNodeConstant(cid, fname =>
            {
                var fc = OprUtils.FunctionCall(SNNominator.Check(fname), np.PopArray());
                DealWithEACall(np, fc, state);
                return null;
            });
        }
        public static void DoCallNamedMethod(SyntaxNodePool np, int cid, int state)
        {
            np.PushNodeConstant(cid, fname =>
            {
                var obj = SNNominator.Check(np.PopExpression());
                var args = np.PopArray();
                var f = new SNMemberAccess(obj, fname);
                SNExpression fc = OprUtils.FunctionCall(f, args);
                DealWithEACall(np, fc, state);
                return null;
            });

        }

        public static void DoGetNamedMember(SyntaxNodePool np, int cid)
        {
            np.PushNodeConstant(cid, member => new SNMemberAccess(SNNominator.Check(np.PopExpression()), member));
        }
        public static void DoGetStringVar(SyntaxNodePool np, RawInstruction inst)
        {
            np.PushNode(new SNNominator(inst.Parameters[0].String));
        }
        public static void DoGetStringMember(SyntaxNodePool np, RawInstruction inst)
        {
            // pop member name???
            var memberName = new SNNominator(inst.Parameters[0].String);
            //pop the object
            var objectVal = np.PopExpression();
            var valueVal = new SNMemberAccess(memberName, objectVal);
            np.PushNode(valueVal);
        }

        public static bool Parse(SyntaxNodePool np, RawInstruction inst)
        {
            switch (inst.Type)
            {

                case InstructionType.GetMember:
                    np.ExecBinaryOprOnStack(GetMember);
                    break;
                case InstructionType.SetMember:
                    DoSetMember(np);
                    break;
                case InstructionType.EA_ZeroVar:
                    np.PushNode(new SNValAssign(new SNMemberAccess(new SNNominator("this"), np.PopExpression()), new SNLiteral(RawValue.FromInteger(0))));
                    break;
                case InstructionType.Delete:
                    DoDelete(np);
                    break;


                case InstructionType.CallFunction:
                    DoCallFunction(np, 0);
                    break;
                case InstructionType.EA_CallFuncPop:
                    DoCallFunction(np, 2);
                    break;
                case InstructionType.EA_CallFunc:
                    DoCallFunction(np, 1);
                    break;


                // Since the execution (in original implementation)
                // is precisely the same as CallMethod, omit it
                // TODO Don't know if the word pop means discard the return value
                case InstructionType.EA_CallMethodPop:
                    DoCallMethod(np, 2);
                    break;
                case InstructionType.CallMethod:
                    DoCallMethod(np, 0);
                    break;
                case InstructionType.EA_CallMethod:
                    DoCallMethod(np, 1);
                    break;

                case InstructionType.EA_GetNamedMember:
                    DoGetNamedMember(np, inst.Parameters[0].Integer);
                    break;

                case InstructionType.EA_CallNamedFuncPop:
                    DoCallNamedFunc(np, inst.Parameters[0].Integer, 2);
                    break;
                case InstructionType.EA_CallNamedFunc:
                    DoCallNamedFunc(np, inst.Parameters[0].Integer, 1);
                    break;
                case InstructionType.EA_CallNamedMethodPop:
                    DoCallNamedMethod(np, inst.Parameters[0].Integer, 2);
                    break;
                case InstructionType.EA_CallNamedMethod:
                    DoCallNamedMethod(np, inst.Parameters[0].Integer, 1); // TODO need check
                    break;


                case InstructionType.EA_GetStringVar:
                    DoGetStringVar(np, inst);
                    break;
                case InstructionType.EA_GetStringMember:
                    DoGetStringMember(np, inst);
                    break;
                case InstructionType.EA_SetStringVar:
                    np.PushNode(new SNValAssign(np.PopExpression(), new SNLiteral(inst.Parameters[0])));
                    break;
                case InstructionType.EA_SetStringMember:
                    var memberVal = np.PopExpression();
                    var objectVal = np.PopExpression();
                    np.PushNode(new SNValAssign(new SNMemberAccess(objectVal, memberVal), new SNLiteral(inst.Parameters[0])));
                    break;

                case InstructionType.TypeOf:
                    np.ExecUnaryOprOnStack(TypeOf);
                    break;
                case InstructionType.CastOp:
                    np.ExecBinaryOprOnStack(CastOp);
                    break;
                case InstructionType.ImplementsOp:
                    DoImplements(np);
                    break;
                case InstructionType.Extends:
                    DoExtends(np);
                    break;
                case InstructionType.InstanceOf:
                    np.ExecBinaryOprOnStack(InstanceOf);
                    break;


                case InstructionType.NewMethod:
                    DoNewMethod(np);
                    break;
                case InstructionType.NewObject:
                    DoNewObject(np);
                    break;
                case InstructionType.InitObject:
                    DoInitObject(np);
                    break;
                case InstructionType.InitArray:
                    DoInitArray(np);
                    break;


                case InstructionType.DefineLocal:
                    DoDefineLocal(np);
                    break;
                case InstructionType.Var:
                    DoDefineLocal2(np);
                    break;
                case InstructionType.Delete2:
                    DoDelete2(np);
                    break;


                case InstructionType.Enumerate:
                case InstructionType.Enumerate2:
                    var obj = np.PopExpression();
                    // a null object and the enumerated objects are pushed
                    // but due to the mechanism of this class, only the latter
                    // is needed
                    var n = new SNEnumerate(obj);
                    np.PushNode(n);
                    return false;

                case InstructionType.EA_PushThis:
                    return false;
                case InstructionType.EA_PushGlobal:
                    return false;
                case InstructionType.EA_PushThisVar:
                    np.PushNode(new SNNominator("this"));
                    break;
                case InstructionType.EA_PushGlobalVar:
                    np.PushNode(new SNNominator("_global"));
                    break;
                case InstructionType.EA_PushValueOfVar:
                    np.PushNodeConstant(inst.Parameters[0].Integer, x => SNNominator.Check(x));
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}
