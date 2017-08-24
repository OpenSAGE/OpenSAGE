using System;
using System.Collections.Generic;

namespace OpenSage.Data.Ini.Parser
{
    internal interface IIniFieldParserProvider<T>
    {
        bool TryGetFieldParser(string fieldName, out ParseFieldCallback<T> fieldParser);
    }

    internal delegate void ParseFieldCallback<T>(IniParser parser, T result);

    internal sealed class IniParseTable<T> : Dictionary<string, ParseFieldCallback<T>>, IIniFieldParserProvider<T>
    {
        public IniParseTable(IDictionary<string, ParseFieldCallback<T>> dictionary)
            : base(dictionary)
        {

        }

        public IniParseTable() { }

        public IniParseTable<T> Concat<T1, T2>(IniParseTable<T2> otherTable)
            where T1 : T, T2
        {
            var result = new IniParseTable<T>(this);

            foreach (var kvp in otherTable)
            {
                result.Add(kvp.Key, (parser, x) => kvp.Value(parser, (T1)x));
            }

            return result;
        }

        bool IIniFieldParserProvider<T>.TryGetFieldParser(string fieldName, out ParseFieldCallback<T> fieldParser)
        {
            return TryGetValue(fieldName, out fieldParser);
        }
    }

    internal sealed class IniArbitraryFieldParserProvider<T> : IIniFieldParserProvider<T>
    {
        private readonly Action<T, string> _parseFieldCallback;

        public IniArbitraryFieldParserProvider(Action<T, string> parseFieldCallback)
        {
            _parseFieldCallback = parseFieldCallback;
        }

        bool IIniFieldParserProvider<T>.TryGetFieldParser(string fieldName, out ParseFieldCallback<T> fieldParser)
        {
            fieldParser = (parser, result) => _parseFieldCallback(result, fieldName);
            return true;
        }
    }
}