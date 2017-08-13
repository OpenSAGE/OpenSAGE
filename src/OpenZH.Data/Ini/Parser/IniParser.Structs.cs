using System;

namespace OpenZH.Data.Ini.Parser
{
    partial class IniParser
    {
        private delegate bool ParseAttributeValue<T>(string value, out T parsedValue);

        private T ParseAttribute<T>(string label, ParseAttributeValue<T> parse)
        {
            var attributeToken = NextToken(IniTokenType.Identifier);

            void throwException()
            {
                throw new IniParseException($"Invalid attribute '{attributeToken.StringValue}'", attributeToken.Position);
            }

            if (!attributeToken.StringValue.Contains(":"))
            {
                throwException();
            }

            var attributeParts = attributeToken.StringValue.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (attributeParts.Length < 2)
            {
                // A bit hacky, but it works... Some attributes have a space between the colon and value.

                attributeParts = new[] { attributeParts[0], ParseFloat().ToString() };
            }
            if (attributeParts[0].ToUpper() != label.ToUpper())
            {
                throwException();
            }

            if (!parse(attributeParts[1], out var parsedValue))
            {
                throwException();
            }

            return parsedValue;
        }

        public int ParseAttributeInteger(string label)
        {
            return ParseAttribute<int>(label, int.TryParse);
        }

        public string ParseAttributeString(string label)
        {
            bool tryParseString(string value, out string parsedValue)
            {
                parsedValue = value;
                return true;
            }

            return ParseAttribute<string>(label, tryParseString);
        }

        public T ParseAttributeEnum<T>(string label)
            where T : struct
        {
            bool tryParseEnum(string value, out T parsedValue)
            {
                parsedValue = ParseEnum<T>(new IniToken(IniTokenType.Identifier, CurrentPosition) { StringValue = value });
                return true;
            }

            return ParseAttribute<T>(label, tryParseEnum);
        }

        public byte ParseAttributeByte(string label)
        {
            return ParseAttribute<byte>(label, byte.TryParse);
        }

        public float ParseAttributeFloat(string label)
        {
            return ParseAttribute<float>(label, float.TryParse);
        }

        public string ParseLocalizedStringKey() => ParseIdentifier();
    }
}
