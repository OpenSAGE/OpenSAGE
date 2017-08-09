using System;
using System.Collections.Generic;

namespace OpenZH.Data.Ini.Parser
{
    internal sealed class IniParseTable<T> : Dictionary<string, Action<IniParser, T>>
    {
    }
}
