using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    public static class FunctionCommon
    {

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static Value[] GetArgumentsFromStack(ActionContext context)
        {
            var argCount = context.Pop().ToInteger();

            var args = new Value[argCount];
            for (int i = 0; i < argCount; ++i)
            {
                args[i] = context.Pop();
            }

            return args;
        }

        // TODO this function is no longer safe. try to use the new model.
        // TODO try to fix compatibility
        public static Value ExecuteFunction(Value funcVal, Value[] args, ObjectContext scope, VM vm)
        {
            if (funcVal.Type != ValueType.Undefined)
            {
                var func = funcVal.ToFunction();
                var ret = vm.Execute(func, args, scope);
                return ret;
            }
            else
            {
                logger.Warn($"Function val is wrong is wrong type: {funcVal}");
            }
            return Value.Undefined();
        }

        public static Value StartExecutingFunction(string funcName, Value[] args, ActionContext context, ObjectContext thisVar = null)
        {
            return StartExecutingFunction(context.GetValueOnChain(funcName), args, context, thisVar);
        }
        public static Value StartExecutingFunction(Value funcVal, Value[] args, ActionContext context, ObjectContext thisVar = null)
        {
            if (funcVal.Type != ValueType.Undefined)
            {
                var func = funcVal.ToFunction();
                if (thisVar == null) thisVar = context.Global;
                return func.Invoke(context, thisVar, args);
            }
            else
            {
                logger.Warn($"Function val is wrong is wrong type: {funcVal}");
            }
            return null;
        }
    
    }

    /// <summary>
    /// Declare a new named or anonymous function (depending on function name) that will either be
    /// pushed to stack or set as a variable. 
    /// </summary>
    public sealed class DefineFunction : InstructionBase
    {
        public override InstructionType Type => InstructionType.DefineFunction;
        public override uint Size => 24;

        public override string GetParameterDesc(ActionContext context)
        {
            var name = Parameters[0].ToString();
            var nParams = Parameters[1].ToInteger();
            var size = Parameters[2 + nParams].ToInteger();
            return $"\"{name}\", {nParams} Params, {size} Bytes of Code";
        }

        public override void Execute(ActionContext context)
        {
            var name = Parameters[0].ToString();
            var nParams = Parameters[1].ToInteger();
            var size = Parameters[2 + nParams].ToInteger();

            //create a list of parameters
            var paramList = Parameters
                .Skip(2)
                .Take(nParams)
                .ToList();

            //get all the instructions
            var code = context.Stream.GetInstructions(size);

            var func = new DefinedFunction(context.Apt.Avm)
            {
                Parameters = paramList,
                Instructions = code,
                NumberRegisters = 4,
                Constants = context.Constants, // do not need copy, see df2
                DefinedContext = context, 
                IsNewVersion = false
            };

            var funcVal = Value.FromFunction(func);

            if (name.Length > 0)
                context.This.SetMember(name, funcVal);
            //anonymous function/lambda function
            else
                context.Push(funcVal);
        }
    }

    /// <summary>
    /// Declare a new named or anonymous function (depending on function name) that will either be
    /// pushed to stack or set as a variable. 
    /// </summary>
    public sealed class DefineFunction2 : InstructionBase
    {
        public override InstructionType Type => InstructionType.DefineFunction2;
        public override uint Size => 28;

        public override string GetParameterDesc(ActionContext context)
        {
            var name = Parameters[0].ToString();
            var nParams = Parameters[1].ToInteger();
            var nRegisters = Parameters[2].ToInteger();
            FunctionPreloadFlags flags = (FunctionPreloadFlags) Parameters[3].ToInteger();
            var size = Parameters[4 + nParams * 2].ToInteger();
            return $"\"{name}\", {nParams} Params, {nRegisters} Regs, {size} Bytes of Code, Flags: {flags}";
        }

        public override void Execute(ActionContext context)
        {
            var name = Parameters[0].ToString();
            var nParams = Parameters[1].ToInteger();
            var nRegisters = Parameters[2].ToInteger();
            FunctionPreloadFlags flags = (FunctionPreloadFlags) Parameters[3].ToInteger();
            var size = Parameters[4 + nParams * 2].ToInteger();

            //create a list of parameters
            var paramList = Parameters
                .Skip(4)
                .Take(nParams * 2)
                .ToList();

            //get all the instructions
            var code = context.Stream.GetInstructions(size);

            var func = new DefinedFunction(context.Apt.Avm)
            {
                Parameters = paramList,
                Instructions = code,
                NumberRegisters = nRegisters,
                Constants = context.Constants, // do not need shallow copy anymore since won't override
                DefinedContext = context,
                Flags = flags,
                IsNewVersion = true
            };

            var funcVal = Value.FromFunction(func);

            if (name.Length > 0)
                context.This.SetMember(name, funcVal);
            //anonymous function/lambda function
            else
                context.Push(funcVal);
        }
    }


    /// <summary>
    /// Return out of the current function back to the calling point
    /// If the function does not have a return statement, the value should be null
    /// Else, the value is set to anything other than null (including undefined)
    /// </summary>
    public sealed class Return : InstructionBase
    {
        public override InstructionType Type => InstructionType.Return;

        public override void Execute(ActionContext context)
        {
            context.Return = true;
            context.Halt = true;
            var ret = context.Pop();
            context.ReturnValue = ret == null ? Value.FromObject(null) : ret;
        }
    }

    public sealed class DealWithReturnValue : InstructionBase
    {
        public override InstructionType Type => throw new InvalidOperationException("Should not be called since this is not a standard instruction");

        public DealWithReturnValue(Value v) : base() { Parameters = new List<Value> { v }; }

        public override void Execute(ActionContext context)
        {
            // TODO what on earth does the document mean?
            // original implementation
            /*
            var ret = Parameters[0] == null ? null : Parameters[0].GetReturnValue();
            if (ret != null) context.Push(ret);
            */
            // implementation from observation
            context.Push(Parameters[0] == null ? Value.Undefined() : Parameters[0].ResolveReturn());

        }
    }

    public sealed class ExecNativeCode : InstructionBase
    {
        public override InstructionType Type => throw new InvalidOperationException("Should not be called since this is not a standard instruction");

        private Action<ActionContext> _action;

        public ExecNativeCode(Action<ActionContext> a) : base() { _action = a; }

        public override void Execute(ActionContext context) { _action(context); }
    }

    /// <summary>
    /// Call an anonymous method that is on the stack. Function arguments are also popped from the stack
    /// </summary>
    public class CallMethod : InstructionBase
    {
        public override InstructionType Type => InstructionType.CallMethod;

        public override void Execute(ActionContext context)
        {
            var funcName = context.Pop().ToString();
            var ret = (Value) null;
            // If funcname is defined we need get the function from an object
            if (funcName.Length > 0)
            {
                var obj = context.Pop().ToObject();
                var args = FunctionCommon.GetArgumentsFromStack(context);
                ret = FunctionCommon.StartExecutingFunction(funcName, args, context, obj);
            }
            // Else the function is on the stack
            else
            {
                var funcVal = context.Pop();
                var args = FunctionCommon.GetArgumentsFromStack(context);
                ret = FunctionCommon.StartExecutingFunction(funcVal, args, context);
            }
            context.PushRecallCode(new DealWithReturnValue(ret));
        }
    }

    public sealed class EACallMethod: CallMethod
    {
        public override InstructionType Type => InstructionType.EA_CallMethod;
    }

    /// <summary>
    /// Call an anonymous method that is on the stack. Function arguments are also popped from the stack
    /// </summary>
    public sealed class CallMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallMethodPop;

        public override void Execute(ActionContext context)
        {
            var funcName = context.Pop().ToString();
            var ret = (Value) null;
            // If funcname is defined we need get the function from an object
            if (funcName.Length > 0)
            {
                var obj = context.Pop().ToObject();
                var args = FunctionCommon.GetArgumentsFromStack(context);
                ret = FunctionCommon.StartExecutingFunction(funcName, args, context, obj);
            }
            // Else the function is on the stack
            else
            {
                var funcVal = context.Pop();
                var args = FunctionCommon.GetArgumentsFromStack(context);
                ret = FunctionCommon.StartExecutingFunction(funcVal, args, context);
            }
            context.PushRecallCode(new DealWithReturnValue(ret));
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Function arguments are popped from the stack
    /// </summary>
    public sealed class CallNamedMethodPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedMethodPop;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var funcName = Parameters[0].ResolveConstant(context).ToString();
            var obj = context.Pop().ToObject();
            var args = FunctionCommon.GetArgumentsFromStack(context);

            var ret = FunctionCommon.StartExecutingFunction(funcName, args, context, obj);
            context.PushRecallCode(new DealWithReturnValue(ret));
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Has no arguments
    /// </summary>
    public sealed class CallNamedFunc : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedFunc;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var funcName = Parameters[0].ResolveConstant(context).ToString();
            var args = FunctionCommon.GetArgumentsFromStack(context);

            var ret = FunctionCommon.StartExecutingFunction(funcName, args, context);
            context.PushRecallCode(new DealWithReturnValue(ret));
        }
    }

    /// <summary>
    /// Call a named method that is in the current scope. Function arguments are popped from the stack
    /// </summary>
    public sealed class CallNamedFuncPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedFuncPop;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var funcName = Parameters[0].ResolveConstant(context).ToString();
            var args = FunctionCommon.GetArgumentsFromStack(context);

            var ret = FunctionCommon.StartExecutingFunction(funcName, args, context);
            context.PushRecallCode(new DealWithReturnValue(ret));
        }
    }

    /// <summary>
    /// Call a function that is defined in the current scope
    /// </summary>
    public sealed class CallFunction : InstructionBase
    {
        public override InstructionType Type => InstructionType.CallFunction;
        public override uint Size => 0;

        public override void Execute(ActionContext context)
        {
            var funcName = context.Pop().ToString();
            var args = FunctionCommon.GetArgumentsFromStack(context);
            var ret = FunctionCommon.StartExecutingFunction(funcName, args, context);
            context.PushRecallCode(new DealWithReturnValue(ret));
        }
    }

    /// <summary>
    /// Call a function that is defined in the current scope
    /// </summary>
    public sealed class CallFunc : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallFunc;
        public override uint Size => 0;

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Call a function that is defined in the current scope
    /// Since there is no reference, assume the popping order is correct
    /// </summary>
    public sealed class CallNamedMethod : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallNamedMethod;
        public override uint Size => 1;

        public override void Execute(ActionContext context)
        {
            var id = Parameters[0].ToInteger();
            var funcName = context.Constants[id].ToString();
            var obj = context.Pop().ToObject();
            var args = FunctionCommon.GetArgumentsFromStack(context);

            FunctionCommon.StartExecutingFunction(funcName, args, context, obj);
            context.Apt.Avm.ExecuteUntilHalt();

            var ret = FunctionCommon.StartExecutingFunction(funcName, args, context, obj);
            context.PushRecallCode(new ExecNativeCode((_) =>
            {
                var result = ret.ResolveReturn();
                var varName = context.Pop();
                context.SetValueOnLocal(varName.ToString(), result);
            }));

            
        }
    }

    /// <summary>
    /// Call an function which its name is on the stack. Function arguments are also popped from the stack
    /// </summary>
    public sealed class CallFunctionPop : InstructionBase
    {
        public override InstructionType Type => InstructionType.EA_CallFuncPop;

        public override void Execute(ActionContext context)
        {
            var funcName = context.Pop().ToString();
            var args = FunctionCommon.GetArgumentsFromStack(context);
            var ret = FunctionCommon.StartExecutingFunction(funcName, args, context);
            context.PushRecallCode(new DealWithReturnValue(ret));
        }
    }
}
