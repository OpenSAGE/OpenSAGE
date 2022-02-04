using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAS2.Base
{
    public enum InstructionType : byte
    {

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
        StrictEquals = 0x66,
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

        Padding = 0xFF,
    }

    [Flags]
    public enum FunctionPreloadFlags
    {
        PreloadExtern = 0x010000,   //this seems to be added by EA
        PreloadParent = 0x008000,
        PreloadRoot = 0x004000,

        SupressSuper = 0x002000,
        PreloadSuper = 0x001000,
        SupressArguments = 0x000800,
        PreloadArguments = 0x000400,
        SupressThis = 0x000200,
        PreloadThis = 0x000100,
        PreloadGlobal = 0x000001
    }

    public enum RawParamType
    {
        UI8,
        UI16,
        UI24,
        UI32,

        I8,
        I16,
        I32,

        Jump8,
        Jump16,
        Jump32,
        Jump64,

        Float,
        Double,
        Boolean,
        String,

        ArrayBegin,
        ArrayEnd,
        ArraySize,

        BranchOffset,

        Constant,
        Register,
    }

    public enum RawValueType
    {
        String = 0,
        Boolean = 1,
        Integer = 2,
        Float = 3,

        Constant = 4,
        Register = 5,
    }

    public enum ConstantType
    {
        //TODO: validate that all those types are correct
        Undef = 0,
        String = 1,
        Property = 2,
        None = 3,
        Register = 4,
        Boolean = 5,
        Float = 6,
        Integer = 7,
        Lookup = 8
    }

    public static class Definition
    {
        public const uint IntPtrSize = 4;
        private static Dictionary<InstructionType, List<RawParamType>> _typeParams;
        private static Dictionary<InstructionType, int> _typeLength;

        static Definition()
        {
            _typeParams = new();
            _typeLength = new();
            foreach (var itype in Enum.GetValues(typeof(InstructionType)).Cast<InstructionType>())
            {
                try
                {
                    _typeParams[itype] = GetParamSequenceInner(itype);
                    _typeLength[itype] = CalcLengthInner(_typeParams[itype]);
                }
                catch (NotImplementedException nie)
                {

                }
            }
        }

        public static bool IsAlignmentRequired(InstructionType type)
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

        public static List<RawParamType> GetParamSequence(InstructionType type)
        {
            return _typeParams.TryGetValue(type, out var x) ? x : throw new NotImplementedException();
        }

        public static int GetParamLength(InstructionType type)
        {
            return _typeLength.TryGetValue(type, out var x) ? x : throw new NotImplementedException();
        }

        private static int GetTypeLengthInner(RawParamType t)
        {
            switch (t)
            {
                case RawParamType.UI8:
                case RawParamType.Jump8:
                case RawParamType.Boolean:
                    return 1;

                case RawParamType.UI16:
                case RawParamType.I16:
                case RawParamType.Jump16:
                    return 2;

                case RawParamType.UI24:
                    return 3;

                case RawParamType.UI32:
                case RawParamType.I32:
                case RawParamType.Jump32:
                case RawParamType.Float:
                    return 4;

                case RawParamType.Jump64:
                case RawParamType.Double:
                    return 8;

                // pointer size
                case RawParamType.ArrayBegin:
                case RawParamType.String:
                    return 4;

                // marker, no size
                case RawParamType.ArrayEnd:
                case RawParamType.ArraySize:
                case RawParamType.BranchOffset:
                case RawParamType.Constant:
                case RawParamType.Register:

                default:
                    return 0;
            }
        }

        private static int CalcLengthInner(List<RawParamType> types)
        {
            int ans = 0;
            bool mark = false;
            foreach (var t in types)
            {
                if (t == RawParamType.ArrayBegin)
                {
                    if (mark)
                        throw new InvalidOperationException();
                    else
                        mark = true;
                }
                else if (t == RawParamType.ArrayEnd)
                    mark = false;
                if (!mark)
                    ans += GetTypeLengthInner(t);
            }
            return ans;
        }

        private static List<RawParamType> GetParamSequenceInner(InstructionType type)
        {
            var paramSequence = new List<RawParamType>();

            switch (type)
            {
                // no parameters
                case InstructionType.ToNumber:
                case InstructionType.NextFrame:
                case InstructionType.Play:
                case InstructionType.Stop:
                case InstructionType.Add:
                case InstructionType.Subtract:
                case InstructionType.Multiply:
                case InstructionType.Divide:
                case InstructionType.BitwiseAnd:
                case InstructionType.BitwiseOr:
                case InstructionType.BitwiseXOr:
                case InstructionType.Greater:
                case InstructionType.LessThan:
                case InstructionType.LogicalAnd:
                case InstructionType.LogicalOr:
                case InstructionType.LogicalNot:
                case InstructionType.StringEquals:
                case InstructionType.Pop:
                case InstructionType.ToInteger:
                case InstructionType.GetVariable:
                case InstructionType.SetVariable:
                case InstructionType.StringConcat:
                case InstructionType.GetProperty:
                case InstructionType.SetProperty:
                case InstructionType.CloneSprite:
                case InstructionType.RemoveSprite:
                case InstructionType.Trace:
                case InstructionType.Random:
                case InstructionType.Return:
                case InstructionType.Modulo:
                case InstructionType.TypeOf:
                case InstructionType.Add2:
                case InstructionType.LessThan2:
                case InstructionType.ToString:
                case InstructionType.PushDuplicate:
                case InstructionType.Increment:
                case InstructionType.Decrement:
                case InstructionType.EA_PushThis:
                case InstructionType.EA_PushZero:
                case InstructionType.EA_PushOne:
                case InstructionType.EA_PushThisVar:
                case InstructionType.EA_PushGlobalVar:
                case InstructionType.EA_ZeroVar:
                case InstructionType.EA_PushTrue:
                case InstructionType.EA_PushFalse:
                case InstructionType.EA_PushNull:
                case InstructionType.EA_PushUndefined:
                case InstructionType.CallFunction:
                case InstructionType.EA_CallFunc:
                case InstructionType.EA_CallFuncPop:
                case InstructionType.CallMethod:
                case InstructionType.EA_CallMethod:
                case InstructionType.EA_CallMethodPop:
                case InstructionType.DefineLocal:
                case InstructionType.Var:
                case InstructionType.Delete:
                case InstructionType.Delete2:
                case InstructionType.Enumerate2:
                case InstructionType.Equals2:
                case InstructionType.GetMember:
                case InstructionType.SetMember:
                case InstructionType.InitArray:
                case InstructionType.InitObject:
                case InstructionType.NewMethod:
                case InstructionType.NewObject:
                case InstructionType.StrictEquals:
                case InstructionType.Extends:
                case InstructionType.InstanceOf:
                case InstructionType.ImplementsOp:
                case InstructionType.CastOp:
                case InstructionType.GetTime:
                case InstructionType.End:
                case InstructionType.GetURL2:
                    break;

                // fixed type parameters

                case InstructionType.GotoFrame:
                case InstructionType.GotoFrame2:
                    paramSequence.Add(RawParamType.I32);
                    break;

                case InstructionType.EA_PushByte:
                    paramSequence.Add(RawParamType.I8);
                    break;
                case InstructionType.EA_PushRegister:
                    paramSequence.Add(RawParamType.UI8);
                    paramSequence.Add(RawParamType.Register);
                    break;
                case InstructionType.EA_PushValueOfVar:
                case InstructionType.EA_PushConstantByte:
                case InstructionType.EA_GetNamedMember:
                case InstructionType.EA_CallNamedFuncPop:
                case InstructionType.EA_CallNamedFunc:
                case InstructionType.EA_CallNamedMethodPop:
                case InstructionType.EA_CallNamedMethod:
                    paramSequence.Add(RawParamType.UI8);
                    paramSequence.Add(RawParamType.Constant);
                    break;

                case InstructionType.EA_PushShort:
                    paramSequence.Add(RawParamType.I16);
                    break;
                case InstructionType.EA_PushConstantWord:
                    paramSequence.Add(RawParamType.UI16);
                    paramSequence.Add(RawParamType.Constant);
                    break;

                case InstructionType.SetRegister:
                    paramSequence.Add(RawParamType.UI32);
                    // do not cancel this commet
                    // otherwise the parameter will be parsed as a reference but not a numerical index
                    // paramSequence.Add(RawParamType.Register);
                    break;
                case InstructionType.EA_PushLong:
                    paramSequence.Add(RawParamType.UI32);
                    break;

                case InstructionType.GotoLabel:
                case InstructionType.EA_PushString:
                case InstructionType.EA_GetStringVar:
                case InstructionType.EA_SetStringVar:
                case InstructionType.EA_GetStringMember:
                case InstructionType.EA_SetStringMember:
                    paramSequence.Add(RawParamType.String);
                    break;

                case InstructionType.GetURL:
                    paramSequence.Add(RawParamType.String);
                    paramSequence.Add(RawParamType.String);
                    break;

                case InstructionType.EA_PushFloat:
                    paramSequence.Add(RawParamType.Float);
                    break;

                // REALLY WEIRD stuffs

                case InstructionType.PushData:
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.ArraySize);
                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.Constant);
                    paramSequence.Add(RawParamType.ArrayEnd);
                    break;
                case InstructionType.ConstantPool:
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.ArraySize);
                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.UI32);
                    paramSequence.Add(RawParamType.Constant);
                    paramSequence.Add(RawParamType.ArrayEnd);
                    break;
                case InstructionType.DefineFunction2:
                    paramSequence.Add(RawParamType.String); // name
                    paramSequence.Add(RawParamType.UI32); // nParams
                    paramSequence.Add(RawParamType.ArraySize);
                    paramSequence.Add(RawParamType.UI8); // nRegs
                    paramSequence.Add(RawParamType.UI24); // flags

                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.I32);
                    paramSequence.Add(RawParamType.String);
                    paramSequence.Add(RawParamType.ArrayEnd);

                    paramSequence.Add(RawParamType.I32); // codeLength
                    paramSequence.Add(RawParamType.Jump64);
                    break;
                case InstructionType.DefineFunction:
                    paramSequence.Add(RawParamType.String); // name
                    paramSequence.Add(RawParamType.UI32); // nParams
                    paramSequence.Add(RawParamType.ArraySize);

                    paramSequence.Add(RawParamType.ArrayBegin);
                    paramSequence.Add(RawParamType.String);
                    paramSequence.Add(RawParamType.ArrayEnd);

                    paramSequence.Add(RawParamType.I32); // codeLength
                    paramSequence.Add(RawParamType.Jump64);
                    break;

                case InstructionType.BranchAlways:
                case InstructionType.BranchIfTrue:
                    paramSequence.Add(RawParamType.I32);
                    paramSequence.Add(RawParamType.BranchOffset);
                    break;

                default:
                    throw new InvalidDataException("Unimplemented bytecode instruction parsing:" + type.ToString());
            }
            return paramSequence;
        }

    }
}
