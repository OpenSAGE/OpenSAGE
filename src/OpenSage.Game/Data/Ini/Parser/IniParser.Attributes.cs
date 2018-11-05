using System;
using System.Numerics;
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

        public bool ParseAttributeOptional<T>(string label, Func<T> parseValue, out T parsed)
        {
            var nameToken = GetNextTokenOptional(SeparatorsColon);

            if (nameToken == null)
            {
                parsed = default;
                return false;
            }

            if (!string.Equals(nameToken.Value.Text, label, StringComparison.OrdinalIgnoreCase))
            {
                throw new IniParseException($"Expected attribute name '{label}'", nameToken.Value.Position);
            }

            parsed = parseValue();
            return true;
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
            where T : struct, IConvertible
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

        public Vector2 ParseAttributeVector2(string label)
        {
            return ParseAttribute(label, ParseVector2);
        }

        public Vector3 ParseAttributeVector3(string label)
        {
            return ParseAttribute(label, ParseVector3);
        }
    }
}
