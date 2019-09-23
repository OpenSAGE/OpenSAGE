using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RailedTransportDockUpdateModuleData : DockUpdateModuleData
    {
        internal static RailedTransportDockUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RailedTransportDockUpdateModuleData> FieldParseTable = DockUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<RailedTransportDockUpdateModuleData>
            {
                { "PullInsideDuration", (parser, x) => x.PullInsideDuration = parser.ParseInteger() },
                { "PushOutsideDuration", (parser, x) => x.PushOutsideDuration = parser.ParseInteger() },
                { "ToleranceDistance", (parser, x) => x.ToleranceDistance = parser.ParseFloat() }
            });

        public int PullInsideDuration { get; private set; }
        public int PushOutsideDuration { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public float ToleranceDistance { get; private set; }
    }
}
