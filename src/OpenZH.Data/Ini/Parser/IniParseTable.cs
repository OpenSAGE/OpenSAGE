using System;
using System.Collections.Generic;

namespace OpenZH.Data.Ini.Parser
{
    internal sealed class IniParseTable<T> : Dictionary<string, Action<IniParser, T>>
    {
        public IniParseTable(IDictionary<string, Action<IniParser, T>> dictionary)
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
                result.Add(kvp.Key, (parser, x) => kvp.Value(parser, (T1) x));
            }

            return result;
        }
    }
}
