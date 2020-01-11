using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class HijackerUpdateModuleData : UpdateModuleData
    {
        internal static HijackerUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HijackerUpdateModuleData> FieldParseTable = new IniParseTable<HijackerUpdateModuleData>
        {
            { "ParachuteName", (parser, x) => x.ParachuteName = parser.ParseAssetReference() }
        };

        public string ParachuteName { get; private set; }
    }
}
