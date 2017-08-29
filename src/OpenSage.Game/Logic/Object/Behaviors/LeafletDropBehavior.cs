using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class LeafletDropBehaviorModuleData : BehaviorModuleData
    {
        internal static LeafletDropBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LeafletDropBehaviorModuleData> FieldParseTable = new IniParseTable<LeafletDropBehaviorModuleData>
        {
            { "DisabledDuration", (parser, x) => x.DisabledDuration = parser.ParseInteger() },
            { "Delay", (parser, x) => x.Delay = parser.ParseInteger() },
            { "AffectRadius", (parser, x) => x.AffectRadius = parser.ParseFloat() },
            { "LeafletFXParticleSystem", (parser, x) => x.LeafletFXParticleSystem = parser.ParseAssetReference() },
        };

        public int DisabledDuration { get; private set; }
        public int Delay { get; private set; }
        public float AffectRadius { get; private set; }
        public string LeafletFXParticleSystem { get; private set; }
    }
}
