using System.Collections.Generic;

namespace OpenSage.Data.Ini
{
    public sealed class IniDataContext
    {
        internal Dictionary<string, IniToken> Defines { get; } = new Dictionary<string, IniToken>();
    }
}
