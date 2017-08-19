using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Tunnel contain limit is special case global logic defined by 
    /// <see cref="GameData.MaxTunnelCapacity"/> in GameData.INI and allows the use of 
    /// <see cref="ObjectDefinition.SoundEnter"/> and <see cref="ObjectDefinition.SoundExit"/>.
    /// </summary>
    public sealed class TunnelContain : ObjectBehavior
    {
        internal static TunnelContain Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TunnelContain> FieldParseTable = new IniParseTable<TunnelContain>
        {
            { "TimeForFullHeal", (parser, x) => x.TimeForFullHeal = parser.ParseInteger() }
        };

        public int TimeForFullHeal { get; private set; }
    }
}
