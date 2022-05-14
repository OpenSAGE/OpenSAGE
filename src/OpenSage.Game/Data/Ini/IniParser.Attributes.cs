using System;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    partial class IniParser
    {
        public T ParseAttributeList<T>(
           IIniFieldParserProvider<T> fieldParserProvider)
           where T : class, new()
        {
            var result = new T();

            var done = false;

            while (!done)
            {
                if (_tokenReader.EndOfFile)
                {
                    throw new InvalidOperationException();
                }

                var nameToken = GetNextTokenOptional(SeparatorsColon);
                if (!nameToken.HasValue)
                {
                    break;
                }

                var fieldName = nameToken.Value.Text;
                if (fieldParserProvider.TryGetFieldParser(fieldName, out var fieldParser))
                {
                    _currentBlockOrFieldStack.Push(fieldName);

                    fieldParser(this, result);
                        
                    _currentBlockOrFieldStack.Pop();
                }
                else
                {
                    throw new IniParseException($"Unexpected field '{fieldName}' in block '{_currentBlockOrFieldStack.Peek()}'.", nameToken.Value.Position);
                }
            }

            return result;
        }

        public T ParseAttribute<T>(string label, Func<IniParser, T> parse)
        {
            var nameToken = GetNextToken(SeparatorsColon);
            if (!string.Equals(nameToken.Text, label, StringComparison.OrdinalIgnoreCase))
            {
                throw new IniParseException($"Expected attribute name '{label}'", nameToken.Position);
            }

            return parse(this);
        }

        public delegate T ParseValueDelegate<T>(in IniToken token);

        public T ParseAttribute<T>(string label, ParseValueDelegate<T> parseValue)
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

            if (!nameToken.HasValue)
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

        public Percentage ParseAttributePercentage(string label)
        {
            return ParseAttribute(label, ParsePercentage);
        }

        public int ParseAttributeInteger(string label)
        {
            return ParseAttribute(label, ScanInteger);
        }

        public LogicFrameSpan ParseAttributeTimeMillisecondsToLogicFrames(string label)
        {
            return ParseAttribute(label, ScanTimeMillisecondsToLogicFrames);
        }

        public string ParseAttributeIdentifier(string label)
        {
            string GetText(in IniToken token) => token.Text;

            return ParseAttribute(label, GetText);
        }

        public LazyAssetReference<ObjectDefinition> ParseAttributeObjectReference(string label)
        {
            LazyAssetReference<ObjectDefinition> GetText(in IniToken token) => ParseObjectReference(token.Text);

            return ParseAttribute(label, GetText);
        }

        public T ScanAttributeEnum<T>(string label, in IniToken token)
            where T : Enum
        {
            return ParseAttribute<T>(label, ScanEnum<T>);
        }

        public T ParseAttributeEnum<T>(string label)
            where T : Enum
        {
            T GetValue(in IniToken token) => ScanEnum<T>(token);

            return ParseAttribute(label, GetValue);
        }

        public BitArray<T> ParseAttributeEnumBitArray<T>(string label)
            where T : Enum
        {
            return ParseAttribute(label, ParseEnumBitArray<T>);
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
