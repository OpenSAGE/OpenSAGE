using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CritterEmitterUpdateModuleData : UpdateModuleData
    {
        internal static CritterEmitterUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CritterEmitterUpdateModuleData> FieldParseTable = new IniParseTable<CritterEmitterUpdateModuleData>
        {
            { "FX", (parser, x) => x.FX = parser.ParseAssetReference() },
            { "SpawnObject", (parser, x) => x.SpawnObject = parser.ParseAssetReference() },
            { "ReloadTime", (parser, x) => x.ReloadTime = parser.ParseInteger() }
        };

        public string FX { get; private set; }
        public string SpawnObject { get; private set; }
        public int ReloadTime { get; private set; }
    }
}
