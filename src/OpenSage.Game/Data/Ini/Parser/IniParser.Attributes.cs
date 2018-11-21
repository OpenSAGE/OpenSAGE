using System;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini.Parser
{
    partial class IniParser
    {
        public T ParseAttribute<T>(string label, Func<IniToken, T> parseValue)
        {
            var nameToken = GetNextToken(SeparatorsColon);
            if (!string.Equals(nameToken.Text, label, StringComparison.OrdinalIgnoreCase))
            {
                throw new IniParseException($"Expected attribute name '{label}'", nameToken.Position);
            }

            return parseValue(GetNextToken(SeparatorsColon));
        }

        public T ParseAttribute<T>(string label, Func<T> parseValue)
        {
            var nameToken = GetNextToken(SeparatorsColon);
            if (!string.Equals(nameToken.Text, label, StringComparison.OrdinalIgnoreCase))
            {
                throw new IniParseException($"Expected attribute name '{label}'", nameToken.Position);
            }

            return parseValue();
        }

        public int ParseAttributeInteger(string label)
        {
            return ParseAttribute(label, ScanInteger);
        }

        public string ParseAttributeIdentifier(string label)
        {
            return ParseAttribute(label, x => x.Text);
        }

        public T ParseAttributeEnum<T>(string label)
            where T : struct
        {
            return ParseAttribute(label, x => ParseEnum<T>(x));
        }

        public byte ParseAttributeByte(string label)
        {
            return ParseAttribute(label, ScanByte);
        }

        public bool ParseAttributeBoolean(string label)
        {
            return ParseAttribute(label, ScanBoolean);
        }

        public float ParseAttributeFloat(string label)
        {
            return ParseAttribute(label, ScanFloat);
        }

        public Point2D ParseAttributePoint2D(string label)
        {
            return ParseAttribute(label, ParsePoint);
        }

        public Point2Df ParseAttributePoint2Df(string label)
        {
            return ParseAttribute(label, ParsePoint2Df);
        }
    }
}
