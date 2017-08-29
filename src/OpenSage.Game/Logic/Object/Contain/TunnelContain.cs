using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Tunnel contain limit is special case global logic defined by 
    /// <see cref="GameData.MaxTunnelCapacity"/> in GameData.INI and allows the use of 
    /// <see cref="ObjectDefinition.SoundEnter"/> and <see cref="ObjectDefinition.SoundExit"/>.
    /// </summary>
    public sealed class TunnelContainModuleData : GarrisonContainModuleData
    {
        internal static new TunnelContainModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<TunnelContainModuleData> FieldParseTable = GarrisonContainModuleData.FieldParseTable
            .Concat(new IniParseTable<TunnelContainModuleData>
            {
                { "TimeForFullHeal", (parser, x) => x.TimeForFullHeal = parser.ParseInteger() }
            });

        public int TimeForFullHeal { get; private set; }
    }
}
