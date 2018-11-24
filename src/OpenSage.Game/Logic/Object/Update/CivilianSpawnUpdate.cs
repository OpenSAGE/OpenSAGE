using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CivilianSpawnUpdateModuleData : UpdateModuleData
    {
        internal static CivilianSpawnUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CivilianSpawnUpdateModuleData> FieldParseTable = new IniParseTable<CivilianSpawnUpdateModuleData>
        {
            { "SpawnDelayTime", (parser, x) => x.SpawnDelayTime = parser.ParseInteger() },
            { "MaximumDistance", (parser, x) => x.MaximumDistance = parser.ParseInteger() },
            { "RunToFilter", (parser, x) => x.RunToFilter = ObjectFilter.Parse(parser) },
            { "Civilian", (parser, x) => x.Civilian = parser.ParseEnumBitArray<ObjectKinds>() },
        };

        public int SpawnDelayTime { get; internal set; }
        public int MaximumDistance { get; internal set; }
        public ObjectFilter RunToFilter { get; internal set; }
        public BitArray<ObjectKinds> Civilian { get; internal set; }
    }
}
