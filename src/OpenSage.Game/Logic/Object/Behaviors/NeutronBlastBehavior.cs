using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class NeutronBlastBehaviorModuleData : BehaviorModuleData
    {
        internal static NeutronBlastBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<NeutronBlastBehaviorModuleData> FieldParseTable = new IniParseTable<NeutronBlastBehaviorModuleData>
        {
            { "BlastRadius", (parser, x) => x.BlastRadius = parser.ParseInteger() },
            { "AffectAirborne", (parser, x) => x.AffectAirborne = parser.ParseBoolean() },
            { "AffectAllies", (parser, x) => x.AffectAllies = parser.ParseBoolean() },
        };

        public int BlastRadius { get; private set; }
        public bool AffectAirborne { get; private set; }
        public bool AffectAllies { get; private set; }
    }
}
