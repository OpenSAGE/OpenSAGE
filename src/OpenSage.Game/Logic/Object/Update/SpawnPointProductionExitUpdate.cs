using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpawnPointProductionExitUpdate : ObjectBehavior
    {
        internal static SpawnPointProductionExitUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpawnPointProductionExitUpdate> FieldParseTable = new IniParseTable<SpawnPointProductionExitUpdate>
        {
            { "SpawnPointBoneName", (parser, x) => x.SpawnPointBoneName = parser.ParseBoneName() }
        };

        public string SpawnPointBoneName { get; private set; }
    }
}
