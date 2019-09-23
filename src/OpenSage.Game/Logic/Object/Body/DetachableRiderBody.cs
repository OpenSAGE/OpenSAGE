using OpenSage.Data.Ini;
using OpenSage.Mathematics;

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

        public Percentage HealthPercentageWhenRiderDies { get; private set; }
        public bool StartsActive { get; private set; }
        public string TriggeredBy { get; private set; }
    }
}
