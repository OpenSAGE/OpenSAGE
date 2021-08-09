using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.FileFormats.Apt.ActionScript
{
    public enum RawParamType
    {
        UI8,

        UI16,
        UI24,
        UI32,

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
        ArraySizeNoPutInParam, 

        BranchOffset, 

        Constant,
        Register, 
    }
    public enum RawValueType
    {
        String,
        Boolean,
        Integer,
        Float,

        Constant,
        Register,
    }

    public class ValueStorage
    {
        public RawValueType Type { get; private set; }

        public string String { get; private set; }
        public bool Boolean { get; private set; }
        public int Number { get; private set; }
        public double Decimal { get; private set; }
        public static ValueStorage FromRegister(uint num)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Register;
            v.Number = (int) num;
            return v;
        }

        public static ValueStorage FromConstant(uint id)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Constant;
            v.Number = (int) id;
            return v;
        }

        public ValueStorage ToRegister()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromRegister((uint) Number);
        }

        public ValueStorage ToConstant()
        {
            if (Type != RawValueType.Integer && Type != RawValueType.Constant && Type != RawValueType.Register)
                throw new InvalidOperationException();
            else
                return FromConstant((uint) Number);
        }

        public static ValueStorage FromBoolean(bool cond)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Boolean;
            v.Boolean = cond;
            return v;
        }

        public static ValueStorage FromString(string str)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.String;
            v.String = str;
            return v;
        }

        public static ValueStorage FromInteger(int num)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Integer;
            v.Number = num;
            return v;
        }

        // TODO is it okay?
        public static ValueStorage FromUInteger(uint num)
        {
            var v = new ValueStorage();
            if (num > 0x0FFFFFFF)
            {
                v.Type = RawValueType.Float;
                v.Decimal = (double) num;
            }
            else
            {
                v.Type = RawValueType.Integer;
                v.Number = (int) num;
            }
            return v;
        }

        public static ValueStorage FromFloat(double num)
        {
            var v = new ValueStorage();
            v.Type = RawValueType.Float;
            v.Decimal = num;
            return v;
        }

    }
}


