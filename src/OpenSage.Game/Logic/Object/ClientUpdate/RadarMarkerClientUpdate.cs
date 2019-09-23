using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2Rotwk)]
    public sealed class RadarMarkerClientUpdateModuleData : ClientUpdateModuleData
    {
        internal static RadarMarkerClientUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<RadarMarkerClientUpdateModuleData> FieldParseTable = new IniParseTable<RadarMarkerClientUpdateModuleData>
        {
            { "MarkerType", (parser, x) => x.MarkerType = parser.ParseAssetReference() },
        };

        public string MarkerType { get; private set; }
    }
}
