using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    /// <summary>
    /// All known instruction types that are used in Apt
    /// </summary>
    public enum InstructionType : byte
    {
        /// <summary>
        /// End the current instruction stream
        /// </summary>
        End = 0x00,
        /// <summary>
        /// Go to the next frame of the current object (must be sprite)
        /// </summary>
        NextFrame = 0x04,
        /// <summary>
        /// Go to the previous frame of the current object (must be sprite)
        /// </summary>
        PrevFrame = 0x05,
        /// <summary>
        /// Start playback of current object (must be sprite)
        /// </summary>
        Play = 0x06,
        /// <summary>
        /// Stop playback of current object (must be sprite)
        /// </summary>
        Stop = 0x07,
        ToggleQuality = 0x08,
        StopSounds = 0x09,
        //Basic arithmetic operations
        Add = 0x0A,
        Subtract = 0x0B,
        Multiply = 0x0C,
        Divide = 0x0D,
        //Basic logic operations
        Equals = 0x0E,
        LessThan = 0x0F,
        LogicalAnd = 0x10,
        LogicalOr = 0x11,
        LogicalNot = 0x12,
        //Basic string operations
        StringEquals = 0x13,
        StringLength = 0x14,
        SubString = 0x15,
        //Others
        Pop = 0x17,
        ToInteger = 0x18,
        GetVariable = 0x1C,
        SetVariable = 0x1D,
        SetTarget2 = 0x20,
        StringConcat = 0x21,
        GetProperty = 0x22,
        SetProperty = 0x23,
        CloneSprite = 0x24,
        RemoveSprite = 0x25,
        //Debug instruction
        Trace = 0x26,
        StartDragMovie = 0x27,
        StopDragMovie = 0x28,
        StringCompare = 0x29,
        Throw = 0x2A,
        CastOp = 0x2B,
        ImplementsOp = 0x2C,
        Random = 0x30,
        MbLength = 0x31, //MB is multibyte strings
        Ord = 0x32, //CHAR to ASCII
        Chr = 0x33, //ASCII to CHAR
        GetTime = 0x34,
        MbSubString = 0x35, //MB is multibyte strings
        MbOrd = 0x36, //MB is multibyte strings
        MbChr = 0x37, //MB is multibyte strings
        Delete = 0x3A,
        Delete2 = 0x3B,
        //Function instructions
        DefineLocal = 0x3C,
        CallFunction = 0x3D,
        Return = 0x3E,
        Modulo = 0x3F,
        NewObject = 0x40,
        Var = 0x41,
        InitArray = 0x42,
        InitObject = 0x43,
        TypeOf = 0x44,
        TargetPath = 0x45,
        Enumerate = 0x46,
        Add2 = 0x47, //does also handle strings
        LessThan2 = 0x48,
        Equals2 = 0x49,
        ToNumber = 0x4A,
        ToString = 0x4B,
        PushDuplicate = 0x4C,
        StackSwap = 0x4D,
        GetMember = 0x4E,
        SetMember = 0x4F,
        Increment = 0x50,
        Decrement = 0x51,
        CallMethod = 0x52,
        NewMethod = 0x53,
        InstanceOf = 0x54,
        Enumerate2 = 0x55,
        //EA instructions
        EA_PushThis = 0x56,
        EA_PushGlobal = 0x58,
        EA_PushZero = 0x59,
        EA_PushOne = 0x5A,
        EA_CallFuncPop = 0x5B,
        EA_CallFunc = 0x5C,
        EA_CallMethodPop = 0x5D,
        EA_CallMethod = 0x5E,
        //Bitwise instructions
        BitwiseAnd = 0x60,
        BitwiseOr = 0x61,
        BitwiseXOr = 0x62,
        ShiftLeft = 0x63,
        ShiftRight = 0x64,
        ShiftRight2 = 0x65,
        StrictEqual = 0x66,
        Greater = 0x67,
        StringGreater = 0x68,
        Extends = 0x69,
        //EA functions
        EA_PushThisVar = 0x70,
        EA_PushGlobalVar = 0x71,
        EA_ZeroVar = 0x72,
        EA_PushTrue = 0x73,
        EA_PushFalse = 0x74,
        EA_PushNull = 0x75,
        EA_PushUndefined = 0x76,
        TraceStart = 0x77,
        GotoFrame = 0x81,
        GetURL = 0x83,
        SetRegister = 0x87,
        ConstantPool = 0x88,
        WaitFormFrame = 0x8A,
        SetTarget = 0x8B,
        GotoLabel = 0x8C,
        WaitForFrameExpr = 0x8D,
        DefineFunction2 = 0x8E,
        Try = 0x8F,
        With = 0x94,
        PushData = 0x96,
        BranchAlways = 0x99,
        GetURL2 = 0x9A,
        DefineFunction = 0x9B,
        BranchIfTrue = 0x9D,
        CallFrame = 0x9E,
        GotoFrame2 = 0x9F,
        //EA instructions
        EA_PushString = 0xA1,
        EA_PushConstantByte = 0xA2,
        EA_PushConstantWord = 0xA3,
        EA_GetStringVar = 0xA4,
        EA_GetStringMember = 0xA5,
        EA_SetStringVar = 0xA6,
        EA_SetStringMember = 0xA7,
        EA_PushValueOfVar = 0xAE,
        EA_GetNamedMember = 0xAF,
        EA_CallNamedFuncPop = 0xB0,
        EA_CallNamedFunc = 0xB1,
        EA_CallNamedMethodPop = 0xB2,
        EA_CallNamedMethod = 0xB3,
        EA_PushFloat = 0xB4,
        EA_PushByte = 0xB5,
        EA_PushShort = 0xB6,
        EA_PushLong = 0xB7,
        EA_BranchIfFalse = 0xB8,
        EA_PushRegister = 0xB9,

        Padding = 0xFF
    }

    /// <summary>
    /// Helper class that tells wether or not an instruction needs a 4-byte alignment
    /// </summary>
    public class InstructionAlignment
    {
        public static bool IsAligned(InstructionType type)
        {
            switch (type)
            {
                case InstructionType.DefineFunction:
                case InstructionType.DefineFunction2:
                case InstructionType.ConstantPool:
                case InstructionType.BranchIfTrue:
                case InstructionType.BranchAlways:
                case InstructionType.PushData:
                case InstructionType.GetURL:
                case InstructionType.GotoLabel:
                case InstructionType.SetRegister:
                case InstructionType.SetTarget:
                case InstructionType.GotoFrame:
                case InstructionType.GotoFrame2:
                case InstructionType.With:
                case InstructionType.EA_PushString:
                case InstructionType.EA_BranchIfFalse:
                case InstructionType.EA_GetStringVar:
                case InstructionType.EA_SetStringVar:
                case InstructionType.EA_GetStringMember:
                case InstructionType.EA_SetStringMember:
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// The baseclass used for all Instructions. Set default variables accordingly
    /// </summary>
    public abstract class InstructionBase
    {
        public abstract InstructionType Type { get; }
        //the size in bytes for this instruction (not including the opcode size)
        public virtual uint Size { get { return 0; } set { } }
        public virtual List<Value> Parameters { get; set; }
        public virtual bool Breakpoint { get; set; }
        public virtual bool IsStatement => true;

        public abstract void Execute(ActionContext context);
        public override string ToString()
        {
            return ToString((ActionContext) null);
        }
        public virtual string GetParameterDesc(ActionContext context)
        {
            if (Parameters == null) return "";
            string[] pv;// = { };
            var param_val = Parameters.Take(5).ToArray();
            pv = param_val.Select(x => x.ToStringWithType(context)).ToArray();
            return string.Join(", ", pv);
        }
        public virtual string ToString(ActionContext context)
        {
            return $"{Type}({GetParameterDesc(context)})";//: {Size}";
        }

        public virtual string ToString(string[] parameters)
        {
            var t = (Parameters == null || Parameters.Count == 0) ? Type.ToString() : ToString();
            return $"{t}({string.Join(", ", parameters)})";
        }
    }

    public abstract class InstructionFixedStackOpr: InstructionBase
    {
        public virtual uint StackPush => 0;
        public virtual uint StackPop => 0;
        public override bool IsStatement => StackPush == 0;

        public override void Execute(ActionContext context)
        {
            var push = ExecuteWithArgs(context.Pop(StackPop));
            if ((push == null && StackPush > 0) || StackPush != push.Length)
                throw new InvalidOperationException("Argument count is wrong!");
            context.Push(push);
        }

        public abstract Value[] ExecuteWithArgs(Value[] poppedVals);
    }

    public abstract class InstructionMonoPush : InstructionFixedStackOpr
    {
        public override uint StackPush => PushStack ? 1u : 0u;
        public override uint StackPop => 0;
        public virtual bool PushStack => false;

        public override void Execute(ActionContext context)
        {
            var push = ExecuteWithArgs2(context.Pop(StackPop));
            if ((push == null && PushStack) || (push != null && !PushStack))
                throw new InvalidOperationException("Argument count is wrong!");
            context.Push(push);
        }

        public override Value[] ExecuteWithArgs(Value[] poppedVals)
        {
            var ret = ExecuteWithArgs2(poppedVals);
            if (!PushStack) return null;
            var ans = new Value[1];
            ans[0] = ret;
            return ans;
        }

        public virtual Value ExecuteWithArgs2(Value[] poppedVals)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class InstructionMonoPushPop : InstructionMonoPush
    {
        public override uint StackPop => PopStack ? 1u : 0u;
        public virtual bool PopStack => false;

        public override void Execute(ActionContext context)
        {
            var push = ExecuteWithArgs2(PopStack ? context.Pop() : null);
            if ((push == null && PushStack) || (push != null && !PushStack))
                throw new InvalidOperationException("Argument count is wrong!");
            context.Push(push);
        }

        public override Value[] ExecuteWithArgs(Value[] poppedVals)
        {
            var ret = ExecuteWithArgs2(poppedVals);
            if (!PushStack) return null;
            var ans = new Value[1];
            ans[0] = ret;
            return ans;
        }

        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            return ExecuteWithArgs2(PopStack ? poppedVals[0] : null);
        }

        public virtual Value ExecuteWithArgs2(Value poppedVal)
        {
            throw new NotImplementedException();
        }
    }
    public class InstructionPushValue : InstructionMonoPushPop
    {
        public override InstructionType Type => throw new InvalidOperationException("Should not be called since this is not a standard instruction");
        public override bool PushStack => true;
        public InstructionPushValue() : base() { }
        public InstructionPushValue(Value v) : base() { Parameters = new List<Value> { v }; }
        public virtual Value ValueToPush => Parameters[0];

        public override void Execute(ActionContext context)
        {
            context.Push(ValueToPush);
        }
        public override Value ExecuteWithArgs2(Value poppedVal)
        {
            return ValueToPush;
        }
    }

    public abstract class InstructionMonoOperator: InstructionMonoPushPop
    {
        public override bool PushStack => true;
        public override bool PopStack => true;
        public virtual Func<Value, Value> Operator => throw new NotImplementedException();

        public override void Execute(ActionContext context)
        {
            context.Push(Operator(context.Pop()));
        }
        public override Value ExecuteWithArgs2(Value poppedVal)
        {
            return Operator(poppedVal);
        }
    }

    public abstract class InstructionDiOperator : InstructionMonoPush
    {
        public override bool PushStack => true;
        public override uint StackPop => 2;
        public virtual Func<Value, Value, Value> Operator => throw new NotImplementedException();

        public override void Execute(ActionContext context)
        {
            var a = context.Pop();
            var b = context.Pop();
            context.Push(Operator(a, b));
        }
        public override Value ExecuteWithArgs2(Value[] poppedVals)
        {
            return Operator(poppedVals[0], poppedVals[1]);
        }
    }

    /// <summary>
    /// This padding does exist in the InstructionStream because of alignment requirements of some instructions
    /// </summary>
    public sealed class Padding : InstructionBase
    {
        public override InstructionType Type => InstructionType.Padding;
        public override uint Size { get; set; }

        public override List<Value> Parameters
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public override void Execute(ActionContext context)
        {
            throw new NotImplementedException();
        }

        public Padding(uint size)
        {
            Size = size;
        }
    }
}
