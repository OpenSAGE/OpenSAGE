using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class GrabNugget : WeaponEffectNugget
    {
        internal static GrabNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GrabNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<GrabNugget>
            {
                { "ContainTargetOnEffect", (parser, x) => x.ContainTargetOnEffect = parser.ParseBoolean() },
                { "ImpactTargetOnEffect", (parser, x) => x.ImpactTargetOnEffect = parser.ParseBoolean() },
                { "ShockWaveAmount", (parser, x) => x.ShockWaveAmount = parser.ParseFloat() },
                { "ShockWaveRadius", (parser, x) => x.ShockWaveRadius = parser.ParseFloat() },
                { "ShockWaveTaperOff", (parser, x) => x.ShockWaveTaperOff = parser.ParseFloat() },
                { "ShockWaveZMult", (parser, x) => x.ShockWaveZMult = parser.ParseFloat() },
                { "RemoveTargetFromOtherContain", (parser, x) => x.RemoveTargetFromOtherContain = parser.ParseBoolean() }
            });

        public bool ContainTargetOnEffect { get; private set; }
        public bool ImpactTargetOnEffect { get; private set; }
        public float ShockWaveAmount { get; private set; }
        public float ShockWaveRadius { get; private set; }
        public float ShockWaveTaperOff { get; private set; }
        public float ShockWaveZMult { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool RemoveTargetFromOtherContain { get; private set; }
    }
}
