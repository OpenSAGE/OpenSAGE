using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class HordeNotifyTargetsOfImminentProbableCrushingUpdateModuleData : UpdateModuleData
    {
        internal static HordeNotifyTargetsOfImminentProbableCrushingUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<HordeNotifyTargetsOfImminentProbableCrushingUpdateModuleData> FieldParseTable =
            new IniParseTable<HordeNotifyTargetsOfImminentProbableCrushingUpdateModuleData>
        {
            { "ScanWidth", (parser, x) => x.ScanWidth = parser.ParseFloat() },
        };

        public float ScanWidth { get; private set; }
    }
}
