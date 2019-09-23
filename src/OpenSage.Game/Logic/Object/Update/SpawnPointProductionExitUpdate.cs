using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SpawnPointProductionExitUpdateModuleData : UpdateModuleData
    {
        internal static SpawnPointProductionExitUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpawnPointProductionExitUpdateModuleData> FieldParseTable = new IniParseTable<SpawnPointProductionExitUpdateModuleData>
        {
            { "SpawnPointBoneName", (parser, x) => x.SpawnPointBoneName = parser.ParseBoneName() }
        };

        public string SpawnPointBoneName { get; private set; }
    }
}
