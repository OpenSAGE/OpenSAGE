using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the object to sway if enabled in GameData.INI or allowed by LOD/map specific settings.
    /// </summary>
    public sealed class SwayClientUpdate : ClientUpdate
    {
        internal static SwayClientUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SwayClientUpdate> FieldParseTable = new IniParseTable<SwayClientUpdate>();
    }
}
