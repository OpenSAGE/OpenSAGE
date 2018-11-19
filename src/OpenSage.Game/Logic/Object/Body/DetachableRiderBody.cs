using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DetachableRiderBodyModuleData : ActiveBodyModuleData
    {
        internal static new DetachableRiderBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DetachableRiderBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
            .Concat(new IniParseTable<DetachableRiderBodyModuleData>
            {
                { "HealthPercentageWhenRiderDies", (parser, x) => x.HealthPercentageWhenRiderDies = parser.ParsePercentage() },
                { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
                { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseString() },
            });

        public float HealthPercentageWhenRiderDies { get; internal set; }
        public bool StartsActive { get; internal set; }
        public string TriggeredBy { get; internal set; }
    }
}
