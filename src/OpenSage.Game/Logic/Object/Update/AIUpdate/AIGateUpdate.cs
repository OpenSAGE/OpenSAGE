using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AIGateUpdateModuleData : AIUpdateModuleData
    {
        internal static new AIGateUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AIGateUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<AIGateUpdateModuleData>
            {
                { "TriggerWidthX", (parser, x) => x.TriggerWidthX = parser.ParseFloat() },
                { "TriggerWidthY", (parser, x) => x.TriggerWidthY = parser.ParseFloat() },
            });

        public float TriggerWidthX { get; private set; }
        public float TriggerWidthY { get; private set; }
    }
}
