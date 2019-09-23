using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class ThreatFinderUpdateModuleData : UpdateModuleData
    {
        internal static ThreatFinderUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ThreatFinderUpdateModuleData> FieldParseTable = new IniParseTable<ThreatFinderUpdateModuleData>
        {
            { "DefaultRadius", (parser, x) => x.DefaultRadius = parser.ParseFloat() },
        };

        public float DefaultRadius { get; private set; }
    }
}
