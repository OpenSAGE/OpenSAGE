using System;
using System.Collections.Generic;

namespace OpenSage.Gui.Apt.ActionScript.Opcodes
{
    public enum InstructionType :  byte
    {
        //Playback related instructions
        End = 0x00,
        NextFrame = 0x04,
        PrevFrame = 0x05,
        Play = 0x06,
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
        And = 0x10,
        Or = 0x11,
        Not = 0x12,
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
        GetTimer = 0x34,
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
        New = 0x40,
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
        BranchIfTtrue = 0x9D,
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

        Padding =0xFF
    }

    public class InstructionAlignment
    {
        public static bool IsAligned(InstructionType type)
        {
            switch(type)
            {
                case InstructionType.DefineFunction:
                case InstructionType.DefineFunction2:
                case InstructionType.ConstantPool:
                case InstructionType.BranchIfTtrue:
                case InstructionType.PushData:
                case InstructionType.GetURL:
                case InstructionType.GotoLabel:
                case InstructionType.SetRegister:
                case InstructionType.EA_PushString:
                case InstructionType.EA_GetStringVar:
                case InstructionType.EA_GetStringMember:
                    return true;
            }

            return false;
        }
    }

    public interface IInstruction
    {        
        InstructionType Type { get; }
        //the size in bytes for this instruction (not including the opcode size)
        uint Size { get; }

        List<Value> Parameters { get; set; }
        void Execute();
    }

    public sealed class Padding : IInstruction
    {
        public InstructionType Type => InstructionType.Padding;
        public bool Aligned => false;
        public uint Size { get; private set; }

        public List<Value> Parameters { get => throw new NotImplementedException();
                                        set => throw new NotImplementedException(); }

        public void Execute()
        {
            throw new NotImplementedException();
        }

        public Padding(uint size)
        {
            Size = size;
        }
    }
}
