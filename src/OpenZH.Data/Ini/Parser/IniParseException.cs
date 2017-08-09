using System;

namespace OpenZH.Data.Ini.Parser
{
    internal sealed class IniParseException : Exception
    {
        public IniParseException(string message, IniTokenPosition position)
            : base($"({position}): {message}")
        {
        }
    }
}
