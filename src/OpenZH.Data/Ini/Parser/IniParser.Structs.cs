using OpenZH.Data.Wnd;

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

            var attributeParts = attributeToken.StringValue.Split(':');
            if (attributeParts.Length < 2)
            {
                throwException();
            }
            if (attributeParts[0] != label)
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
