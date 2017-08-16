using System;

namespace OpenSage.Data.Ini.Parser
{
    partial class IniParser
    {
        public T ParseAttribute<T>(string label, Func<T> parseValue)
        {
            var nameToken = NextToken(IniTokenType.Identifier);
            if (!string.Equals(nameToken.StringValue, label, StringComparison.OrdinalIgnoreCase))
            {
                throw new IniParseException($"Expected attribute name '{label}'", nameToken.Position);
            }

            NextToken(IniTokenType.Colon);

            var result = parseValue();

            // Generals FXList.ini line 2296 has an extra colon...
            NextTokenIf(IniTokenType.Colon);

            return result;
        }

        public int ParseAttributeInteger(string label)
        {
            return ParseAttribute(label, ParseInteger);
        }

        public string ParseAttributeIdentifier(string label)
        {
            return ParseAttribute(label, ParseIdentifier);
        }

        public T ParseAttributeEnum<T>(string label)
            where T : struct
        {
            return ParseAttribute(label, ParseEnum<T>);
        }

        public byte ParseAttributeByte(string label)
        {
            return ParseAttribute(label, ParseByte);
        }

        public bool ParseAttributeBoolean(string label)
        {
            return ParseAttribute(label, ParseBoolean);
        }

        public float ParseAttributeFloat(string label)
        {
            return ParseAttribute(label, ParseFloat);
        }
    }
}
