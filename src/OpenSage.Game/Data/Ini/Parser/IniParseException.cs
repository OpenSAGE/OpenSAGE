using System;

namespace OpenSage.Data.Ini.Parser
{
    internal sealed class IniParseException : Exception
    {
        public IniParseException(string message, in IniTokenPosition position)
            : base($"({position}): {message}")
        {
        }
    }
}
