using System;
using System.Collections.Generic;
using System.Linq;

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

        public IniParseTable<T2> Concat<T2>(IniParseTable<T2> otherTable)
            where T2 : T
        {
            var result = new IniParseTable<T2>(this.ToDictionary(
                x => x.Key, 
                x => new ParseFieldCallback<T2>((parser, y) => x.Value(parser, y))));

            foreach (var kvp in otherTable)
            {
                result.Add(kvp.Key, (parser, x) => kvp.Value(parser, x));
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

    internal sealed class CompositeFieldParserProvider<T> : IIniFieldParserProvider<T>
    {
        private readonly IIniFieldParserProvider<T>[] _providers;

        public CompositeFieldParserProvider(params IIniFieldParserProvider<T>[] providers)
        {
            _providers = providers;
        }

        bool IIniFieldParserProvider<T>.TryGetFieldParser(string fieldName, out ParseFieldCallback<T> fieldParser)
        {
            foreach (var provider in _providers)
            {
                if (provider.TryGetFieldParser(fieldName, out fieldParser))
                {
                    return true;
                }
            }

            fieldParser = null;
            return false;
        }
    }
}
