using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenAS2.Base;
using OpenAS2.Runtime;

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
        static readonly OPR2 InstanceOf = (a, b) =>
        {
            var constr = a.ToFunction();
            var obj = b.ToObject();
            var val = obj.InstanceOf(constr);
            return Value.FromBoolean(val);
        };

        public static bool Execute(ExecutionContext context, Instruction inst)
        {
            switch (inst.Type)
            {
                case InstructionType.TypeOf:
                    context.ExecUnaryOprOnStack(TypeOf);
                    break;
                case InstructionType.CastOp:
                    context.ExecBinaryOprOnStack(CastOp);
                    break;
                case InstructionType.ImplementsOp:
                    return false; // TODO
                case InstructionType.Extends:
                    var sup = context.Pop().ToFunction();
                    var cls = context.Pop().ToFunction();
                    var obj = new ESObject(context.Avm);
                    obj.__proto__ = sup.prototype;
                    obj.constructor = sup;
                    cls.prototype = obj;
                    break;
                case InstructionType.InstanceOf:
                    context.ExecBinaryOprOnStack(InstanceOf);
                    break;


                case InstructionType.NewMethod:
                    break;
                case InstructionType.NewObject:
                    break;
                case InstructionType.InitObject:
                    break;
                case InstructionType.InitArray:
                    break;


                case InstructionType.DefineLocal:
                    break;
                case InstructionType.Var:
                    break;
                case InstructionType.Delete2:
                    break;


                case InstructionType.Enumerate:
                    break;
                case InstructionType.Enumerate2:
                    break;


                case InstructionType.GetMember:
                    break;
                case InstructionType.SetMember:
                    break;
                case InstructionType.EA_ZeroVar:
                    break;
                case InstructionType.Delete:
                    break;


                case InstructionType.CallFunction:
                    break;
                case InstructionType.CallMethod:
                    break;
                case InstructionType.EA_CallFuncPop:
                    break;
                case InstructionType.EA_CallFunc:
                    break;
                case InstructionType.EA_CallMethodPop:
                    break;
                case InstructionType.EA_CallMethod:
                    break;
                case InstructionType.EA_GetNamedMember:
                    break;
                case InstructionType.EA_CallNamedFuncPop:
                    break;
                case InstructionType.EA_CallNamedFunc:
                    break;
                case InstructionType.EA_CallNamedMethodPop:
                    break;
                case InstructionType.EA_CallNamedMethod:
                    break;


                case InstructionType.EA_GetStringVar:
                    break;
                case InstructionType.EA_GetStringMember:
                    break;
                case InstructionType.EA_SetStringVar:
                    break;
                case InstructionType.EA_SetStringMember:
                    break;


                case InstructionType.EA_PushThis:
                    break;
                case InstructionType.EA_PushGlobal:
                    break;
                case InstructionType.EA_PushThisVar:
                    break;
                case InstructionType.EA_PushGlobalVar:
                    break;
                case InstructionType.EA_PushValueOfVar:
                    break;

                default:
                    return false;
            }
            return true;
        }
    }
}
