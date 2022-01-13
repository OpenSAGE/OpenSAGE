using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenAS2.Base;
using OpenAS2.Runtime;
using OpenAS2.Runtime.Library;

namespace OpenAS2.Runtime.Execution
{

    using OPR1 = Func<Value, Value>;
    using OPR2 = Func<Value, Value, Value>;
    using OPR1A = Action<Value>;
    using OPR2A = Action<Value, Value>;

    public static class ObjectOriented
    {

        static readonly OPR1 TypeOf = (v) => Value.FromString(v.GetStringType());
        static readonly OPR2 CastOp = (objv, cstv) =>
        {
            var obj = objv.ToObject();
            var cst = cstv.ToFunction();
            ESObject val = obj.InstanceOf(cst) ? obj : null;
            return Value.FromObject(val);
        };
        static void DoExtends(ExecutionContext context) // this should work
        {
            var sup = context.Pop().ToFunction();
            var cls = context.Pop().ToFunction();
            var obj = new ESObject(context.Avm);
            obj.__proto__ = sup.prototype;
            obj.constructor = sup;
            cls.prototype = obj;
        }
        static readonly OPR2 InstanceOf = (a, b) =>
        {
            var constr = a.ToFunction();
            var obj = b.ToObject();
            var val = obj.InstanceOf(constr);
            return Value.FromBoolean(val);
        };

        // all sorts of null check...
        public static void DoNewMethod(ExecutionContext context)
        {
            var nameVal = context.Pop();
            var name = nameVal.ToString();
            var obj = context.Pop();
            var args = FunctionUtils.GetArgumentsFromStack(context);

            Value vfunc = (nameVal.Type != ValueType.Undefined && name.Length != 0) ? obj.ToObject().IGet(name) : obj;
            ESFunction func = vfunc.IsCallable() ? vfunc.ToFunction() : null;

            context.ConstructObjectAndPush(func, args);
        }
        public static void DoNewObject(ExecutionContext context)
        {
            var name = context.Pop().ToString();
            var args = FunctionUtils.GetArgumentsFromStack(context);
            context.ConstructObjectAndPush(name, args);
        }
        public static void DoInitObject(ExecutionContext context)
        {
            var nArgs = context.Pop().ToInteger();
            var obj = new ESObject(context.Avm);
            for (int i = 0; i < nArgs; ++i)
            {
                var vi = context.Pop();
                var ni = context.Pop().ToString();
                obj.IPut(ni, vi);
            }

            context.Push(Value.FromObject(obj));
        }
        public static void DoInitArray(ExecutionContext context)
        {
            var args = FunctionUtils.GetArgumentsFromStack(context);
            context.Push(Value.FromArray(args, context.Avm));
        }

        public static void DoDefineLocal(ExecutionContext context)
        {
            var value = context.Pop();
            var varName = context.Pop().ToString();
            context.SetValueOnLocal(varName, value);
        }
        public static void DoDefineLocal2(ExecutionContext context)
        {
            var varName = context.Pop().ToString();
            if (context.HasValueOnLocal(varName))
                return;
            else
                context.SetValueOnLocal(varName, Value.Undefined());
        }
        public static void DoDelete2(ExecutionContext context)
        {
            var property = context.Pop().ToString();
            context.DeleteValueOnChain(property);
        }

        public static void DoEnumerate2(ExecutionContext context)
        {
            var obj = context.Pop().ToObject();
            context.Push(Value.FromObject(null));
            // Not sure if this is correct
            foreach (var slot in obj.GetAllProperties())
            {
                context.Push(Value.FromString(slot));
            }
        }

        public static readonly OPR2 GetMember = (member, vobj) =>
        {
            var obj = vobj.ToObject();
            // arrays stay the same
            return obj.IGet(member.ToString());
        };
        public static void DoSetMember(ExecutionContext context)
        {
            //pop the value
            var valueVal = context.Pop();
            //pop the member name
            var memberName = context.Pop().ToString();
            //pop the object
            var obj = context.Pop().ToObject();
            if (obj is null)
                throw new InvalidOperationException();
            else
                obj.IPut(memberName, valueVal);
        }
        public static void DoDelete(ExecutionContext context)
        {
            throw new NotImplementedException("need to check");
            var property = context.Pop().ToString();
            var target = context.Pop();// TODO wtf? context.GetTarget(context.Pop().ToString());
            target.ToObject().IDeleteValue(property);
        }

        public static void DoCallFunction(ExecutionContext context)
        {
            var funcName = context.Pop().ToString();
            var args = FunctionUtils.GetArgumentsFromStack(context);
            var ret = FunctionUtils.TryExecuteFunction(funcName, args, context);
            context.EnqueueResultCallback(ret);

        }
        public static void DoCallMethod(ExecutionContext context)
        {
            var funcNameVal = context.Pop();
            var funcName = funcNameVal.ToString();
            ESCallable.Result ret;
            // If funcname is defined we need get the function from an object
            if (!funcNameVal.IsUndefined() && funcName.Length > 0)
            {
                var obj = context.Pop().ToObject();
                var args = FunctionUtils.GetArgumentsFromStack(context);
                ret = FunctionUtils.TryExecuteFunction(funcName, args, context, obj);
            }
            // Else the function is on the stack
            else
            {
                var funcVal = context.Pop();
                var args = FunctionUtils.GetArgumentsFromStack(context);
                ret = FunctionUtils.TryExecuteFunction(funcVal, args, context);
            }
            context.EnqueueResultCallback(ret);
        }
        public static void DoGetNamedMember(ExecutionContext context, int cid)
        {
            var member = context.ResolveConstant(cid).ToString();

            //pop the object
            var objectVal = context.Pop();
            var obj = objectVal.ToObject();

            if (obj != null)
                context.Push(obj.IGet(member));
            else
                context.Push(Value.Undefined());
        }
        public static void DoCallNamedFunc(ExecutionContext context, int cid)
        {
            var funcName = context.ResolveConstant(cid).ToString();
            var args = FunctionUtils.GetArgumentsFromStack(context);

            var ret = FunctionUtils.TryExecuteFunction(funcName, args, context);
            context.EnqueueResultCallback(ret);
        }
        public static void DoCallNamedMethod(ExecutionContext context, int cid, bool pop = false)
        {
            var funcName = context.ResolveConstant(cid).ToString();
            var obj = context.Pop().ToObject();
            var args = FunctionUtils.GetArgumentsFromStack(context);

            var ret0 = FunctionUtils.TryExecuteFunction(funcName, args, context, obj);
            context.EnqueueResultCallback(ret0);

            if (!pop)
            {
                throw new NotImplementedException("need check");
                var ret = FunctionUtils.TryExecuteFunction(funcName, args, context, obj);
                ret.AddRecallCode((ret2) =>
                {
                    var result = ret2.Value;
                    var varName = context.Pop();
                    context.SetValueOnLocal(varName.ToString(), result);
                    return null; // push nothing back
                });
                context.EnqueueResultCallback(ret);
            }
        }

        public static void DoGetStringVar(ExecutionContext context, RawInstruction inst)
        {
            var memberName = inst.Parameters[0].String;
            // check if this a special object, like _root, _parent etc.
            // this is automatically done by the built-in variables in the global object.
            var result = context.GetValueOnChain(memberName);
            if (result == null)
                throw new InvalidOperationException();
            context.Push(result);
        }
        public static void DoGetStringMember(ExecutionContext context, RawInstruction inst)
        {
            // pop member name???
            var memberName = inst.Parameters[0].String;
            //pop the object
            var objectVal = context.Pop();
            var valueVal = objectVal.ToObject().IGet(memberName);
            context.Push(valueVal);
        }

        public static bool Execute(ExecutionContext context, RawInstruction inst)
        {
            switch (inst.Type)
            {

                case InstructionType.GetMember:
                    context.ExecBinaryOprOnStack(GetMember);
                    break;
                case InstructionType.SetMember:
                    DoSetMember(context);
                    break;
                case InstructionType.EA_ZeroVar:
                    context.This.IPut(context.Pop().ToString(), Value.FromInteger(0));
                    break;
                case InstructionType.Delete:
                    DoDelete(context);
                    break;


                case InstructionType.CallFunction:
                case InstructionType.EA_CallFuncPop:
                    DoCallFunction(context);
                    break;
                case InstructionType.EA_CallFunc:
                    return false;

                case InstructionType.CallMethod:
                // Since the execution (in original implementation)
                // is precisely the same as CallMethod, omit it
                // TODO Don't know if the word pop means discard the return value
                case InstructionType.EA_CallMethodPop:
                case InstructionType.EA_CallMethod:
                    DoCallMethod(context);
                    break;

                case InstructionType.EA_GetNamedMember:
                    DoGetNamedMember(context, inst.Parameters[0].Integer);
                    break;
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                    DoCallNamedFunc(context, inst.Parameters[0].Integer);
                    break;
                case InstructionType.EA_CallNamedMethodPop:
                    DoCallNamedMethod(context, inst.Parameters[0].Integer, pop: true);
                    break;
                case InstructionType.EA_CallNamedMethod:
                    DoGetNamedMember(context, inst.Parameters[0].Integer); // TODO need check
                    break;


                case InstructionType.EA_GetStringVar:
                    DoGetStringVar(context, inst);
                    break;
                case InstructionType.EA_GetStringMember:
                    DoGetStringMember(context, inst);
                    break;
                case InstructionType.EA_SetStringVar:
                    context.This.IPut(context.Pop().ToString(), Value.FromRaw(inst.Parameters[0]));
                    break;
                case InstructionType.EA_SetStringMember:
                    var memberVal = context.Pop().ToString();
                    var objectVal = context.Pop().ToObject();
                    objectVal.IPut(memberVal, Value.FromRaw(inst.Parameters[0]));
                    break;

                case InstructionType.TypeOf:
                    context.ExecUnaryOprOnStack(TypeOf);
                    break;
                case InstructionType.CastOp:
                    context.ExecBinaryOprOnStack(CastOp);
                    break;
                case InstructionType.ImplementsOp:
                    return false; // TODO
                case InstructionType.Extends:
                    DoExtends(context);
                    break;
                case InstructionType.InstanceOf:
                    context.ExecBinaryOprOnStack(InstanceOf);
                    break;


                case InstructionType.NewMethod:
                    DoNewMethod(context);
                    break;
                case InstructionType.NewObject:
                    DoNewObject(context);
                    break;
                case InstructionType.InitObject:
                    DoInitObject(context);
                    break;
                case InstructionType.InitArray:
                    DoInitArray(context);
                    break;


                case InstructionType.DefineLocal:
                    DoDefineLocal(context);
                    break;
                case InstructionType.Var:
                    DoDefineLocal2(context);
                    break;
                case InstructionType.Delete2:
                    DoDelete2(context);
                    break;


                case InstructionType.Enumerate:
                    return false; // TODO
                case InstructionType.Enumerate2:
                    DoEnumerate2(context);
                    break;

                case InstructionType.EA_PushThis:
                    return false;
                case InstructionType.EA_PushGlobal:
                    return false;
                case InstructionType.EA_PushThisVar:
                    context.Push(Value.FromObject(context.This));
                    break;
                case InstructionType.EA_PushGlobalVar:
                    context.Push(Value.FromObject(context.Global));
                    break;
                case InstructionType.EA_PushValueOfVar:
                    var cid = context.ResolveConstant(inst.Parameters[0].Integer);
                    var cstr = cid.ToString();
                    context.Push(context.HasParameter(cstr) ? context.GetParameter(cstr) : context.GetValueOnChain(cstr));
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}
