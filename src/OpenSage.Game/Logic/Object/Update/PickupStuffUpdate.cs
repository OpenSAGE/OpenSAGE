using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class PickupStuffUpdateModuleData : UpdateModuleData
    {
        internal static PickupStuffUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PickupStuffUpdateModuleData> FieldParseTable = new IniParseTable<PickupStuffUpdateModuleData>
        {
            { "SkirmishAIOnly", (parser, x) => x.SkirmishAIOnly = parser.ParseBoolean() },
            { "StuffToPickUp", (parser, x) => x.StuffToPickUp = ObjectFilter.Parse(parser) },
            { "ScanRange", (parser, x) => x.ScanRange = parser.ParseFloat() },
            { "ScanIntervalSeconds", (parser, x) => x.ScanIntervalSeconds = parser.ParseFloat() }
        };

        public bool SkirmishAIOnly { get; private set; }
        public ObjectFilter StuffToPickUp { get; private set; }
        public float ScanRange { get; private set; }
        public float ScanIntervalSeconds { get; private set; }
    }
}
