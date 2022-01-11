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

        static readonly OPR1 TypeOf = (v) => OprUtils.TypeOf(v);
        static readonly OPR2 CastOp = (objv, cstv) =>
        {
            var obj = objv.ToObject();
            var cst = cstv.ToFunction();
            ESObject val = obj.InstanceOf(cst) ? obj : null;
            return Value.FromObject(val);
        };
        static void DoExtends(NodePool np) // this should work
        {
            var sup = np.PopExpression();
            var cls = np.PopExpression();
            throw new NotImplementedException();
        }
        static readonly OPR2 InstanceOf = (a, b) => OprUtils.InstanceOf(b, a);

        // all sorts of null check...
        public static void DoNewMethod(NodePool np)
        {
            var nameVal = np.PopExpression();
            var obj = np.PopExpression();
            var args = np.PopArray();

            Value vfunc = (nameVal.Type != ValueType.Undefined && name.Length != 0) ? obj.ToObject().IGet(name) : obj;
            ESFunction func = vfunc.IsCallable() ? vfunc.ToFunction() : null;

            np.ConstructObjectAndPush(func, args);
        }
        public static void DoNewObject(NodePool np)
        {
            var name = np.PopExpression();
            var args = np.PopArray();
            np.PushNode(OprUtils.New(name, args));
        }
        public static void DoInitObject(NodePool np)
        {
            var args = np.PopArray(readPair: true);
            np.PushNode(OprUtils.New(new SNNominator("Object"), args));
        }
        public static void DoInitArray(NodePool np)
        {
            np.PushNode(np.PopArray());
        }

        public static void DoDefineLocal(NodePool np)
        {
            var value = np.PopExpression();
            var varName = np.PopExpression().ToString();
            np.SetValueOnLocal(varName, value);
        }
        public static void DoDefineLocal2(NodePool np)
        {
            var varName = np.PopExpression().ToString();
            if (np.HasValueOnLocal(varName))
                return;
            else
                np.SetValueOnLocal(varName, Value.Undefined());
        }
        public static void DoDelete2(NodePool np)
        {
            np.PushNode(new SNControl("delete", np.PopExpression()));
        }

        public static readonly OPR2 GetMember = (member, vobj) => new SNMemberAccess(vobj, member);

        public static void DoSetMember(NodePool np)
        {
            //pop the value
            var valueVal = np.PopExpression();
            //pop the member name
            var memberName = np.PopExpression();
            //pop the object
            var obj = np.PopExpression();

            np.PushNode(new SNValAssign(new SNMemberAccess(obj, memberName), valueVal));
        }
        public static void DoDelete(NodePool np)
        {
            var property = np.PopExpression();
            var target = np.PopExpression();// TODO wtf? np.GetTarget(np.PopExpression().ToString());
            np.PushNode(new SNControl("delete", new SNMemberAccess(target, property)));
        }

        public static void DoCallFunction(NodePool np)
        {
            var funcName = np.PopExpression().ToString();
            var args = FunctionUtils.GetArgumentsFromStack(np);
            var ret = FunctionUtils.TryExecuteFunction(funcName, args, np);
            np.PushRecallCode(ret);

        }
        public static void DoCallMethod(NodePool np)
        {
            var funcNameVal = np.PopExpression();
            var funcName = funcNameVal.ToString();
            ESCallable.Result ret;
            // If funcname is defined we need get the function from an object
            if (!funcNameVal.IsUndefined() && funcName.Length > 0)
            {
                var obj = np.PopExpression().ToObject();
                var args = FunctionUtils.GetArgumentsFromStack(np);
                ret = FunctionUtils.TryExecuteFunction(funcName, args, np, obj);
            }
            // Else the function is on the stack
            else
            {
                var funcVal = np.PopExpression();
                var args = FunctionUtils.GetArgumentsFromStack(np);
                ret = FunctionUtils.TryExecuteFunction(funcVal, args, np);
            }
            np.PushRecallCode(ret);
        }
        public static void DoGetNamedMember(NodePool np, int cid)
        {
            var member = np.ResolveConstant(cid).ToString();

            //pop the object
            var objectVal = np.PopExpression();
            var obj = objectVal.ToObject();

            if (obj != null)
                np.Push(obj.IGet(member));
            else
                np.Push(Value.Undefined());
        }
        public static void DoCallNamedFunc(NodePool np, int cid)
        {
            var funcName = np.ResolveConstant(cid).ToString();
            var args = FunctionUtils.GetArgumentsFromStack(np);

            var ret = FunctionUtils.TryExecuteFunction(funcName, args, np);
            np.PushRecallCode(ret);
        }
        public static void DoCallNamedMethod(NodePool np, int cid, bool pop = false)
        {
            var funcName = np.ResolveConstant(cid).ToString();
            var obj = np.PopExpression().ToObject();
            var args = FunctionUtils.GetArgumentsFromStack(np);

            var ret0 = FunctionUtils.TryExecuteFunction(funcName, args, np, obj);
            np.PushRecallCode(ret0);

            if (!pop)
            {
                throw new NotImplementedException("need check");
                var ret = FunctionUtils.TryExecuteFunction(funcName, args, np, obj);
                ret.SetRecallCode((ret2) =>
                {
                    var result = ret2.Value;
                    var varName = np.PopExpression();
                    np.SetValueOnLocal(varName.ToString(), result);
                    return null; // push nothing back
                });
                np.PushRecallCode(ret);
            }
        }

        public static void DoGetStringVar(NodePool np, RawInstruction inst)
        {
            var memberName = inst.Parameters[0].String;
            // check if this a special object, like _root, _parent etc.
            // this is automatically done by the built-in variables in the global object.
            var result = np.GetValueOnChain(memberName);
            if (result == null)
                throw new InvalidOperationException();
            np.Push(result);
        }
        public static void DoGetStringMember(NodePool np, RawInstruction inst)
        {
            // pop member name???
            var memberName = inst.Parameters[0].String;
            //pop the object
            var objectVal = np.PopExpression();
            var valueVal = objectVal.ToObject().IGet(memberName);
            np.Push(valueVal);
        }

        public static bool Parse(NodePool np, RawInstruction inst)
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
                    np.This.IPut(np.PopExpression().ToString(), Value.FromInteger(0));
                    break;
                case InstructionType.Delete:
                    DoDelete(np);
                    break;


                case InstructionType.CallFunction:
                case InstructionType.EA_CallFuncPop:
                    DoCallFunction(np);
                    break;
                case InstructionType.EA_CallFunc:
                    return false;

                case InstructionType.CallMethod:
                // Since the execution (in original implementation)
                // is precisely the same as CallMethod, omit it
                // TODO Don't know if the word pop means discard the return value
                case InstructionType.EA_CallMethodPop:
                case InstructionType.EA_CallMethod:
                    DoCallMethod(np);
                    break;

                case InstructionType.EA_GetNamedMember:
                    DoGetNamedMember(np, inst.Parameters[0].Integer);
                    break;
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                    DoCallNamedFunc(np, inst.Parameters[0].Integer);
                    break;
                case InstructionType.EA_CallNamedMethodPop:
                    DoCallNamedMethod(np, inst.Parameters[0].Integer, pop: true);
                    break;
                case InstructionType.EA_CallNamedMethod:
                    DoGetNamedMember(np, inst.Parameters[0].Integer); // TODO need check
                    break;


                case InstructionType.EA_GetStringVar:
                    DoGetStringVar(np, inst);
                    break;
                case InstructionType.EA_GetStringMember:
                    DoGetStringMember(np, inst);
                    break;
                case InstructionType.EA_SetStringVar:
                    np.This.IPut(np.PopExpression().ToString(), Value.FromRaw(inst.Parameters[0]));
                    break;
                case InstructionType.EA_SetStringMember:
                    var memberVal = np.PopExpression().ToString();
                    var objectVal = np.PopExpression().ToObject();
                    objectVal.IPut(memberVal, Value.FromRaw(inst.Parameters[0]));
                    break;

                case InstructionType.TypeOf:
                    np.ExecUnaryOprOnStack(TypeOf);
                    break;
                case InstructionType.CastOp:
                    np.ExecBinaryOprOnStack(CastOp);
                    break;
                case InstructionType.ImplementsOp:
                    return false; // TODO
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
                    return false; // TODO
                case InstructionType.Enumerate2:
                    DoEnumerate2(np);
                    break;

                case InstructionType.EA_PushThis:
                    return false;
                case InstructionType.EA_PushGlobal:
                    return false;
                case InstructionType.EA_PushThisVar:
                    np.Push(Value.FromObject(np.This));
                    break;
                case InstructionType.EA_PushGlobalVar:
                    np.Push(Value.FromObject(np.Global));
                    break;
                case InstructionType.EA_PushValueOfVar:
                    var cid = np.ResolveConstant(inst.Parameters[0].Integer);
                    var cstr = cid.ToString();
                    np.Push(np.HasParameter(cstr) ? np.GetParameter(cstr) : np.GetValueOnChain(cstr));
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}
