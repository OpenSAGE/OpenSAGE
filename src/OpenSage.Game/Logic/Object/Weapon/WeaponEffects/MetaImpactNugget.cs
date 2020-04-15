using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Throws things back with force.
    /// </summary>
    [AddedIn(SageGame.Bfme2)]
    public sealed class MetaImpactNugget : WeaponEffectNugget
    {
        internal static MetaImpactNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<MetaImpactNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<MetaImpactNugget>
            {
                { "HeroResist", (parser, x) => x.HeroResist = parser.ParseFloat() },
                { "ShockWaveAmount", (parser, x) => x.ShockWaveAmount = parser.ParseFloat() },
                { "ShockWaveRadius", (parser, x) => x.ShockWaveRadius = parser.ParseFloat() },
                { "ShockWaveTaperOff", (parser, x) => x.ShockWaveTaperOff = parser.ParseFloat() },
                { "ShockWaveArc", (parser, x) => x.ShockWaveArc = parser.ParseFloat() },
                { "ShockWaveZMult", (parser, x) => x.ShockWaveZMult = parser.ParseFloat() },
                { "ShockWaveSpeed", (parser, x) => x.ShockWaveSpeed = parser.ParseFloat() },
                { "InvertShockWave", (parser, x) => x.InvertShockWave = parser.ParseBoolean() },
                { "DelayTime", (parser, x) => x.DelayTime = parser.ParseInteger() },
                { "FlipDirection", (parser, x) => x.FlipDirection = parser.ParseBoolean() },
                { "OnlyWhenJustDied", (parser, x) => x.OnlyWhenJustDied = parser.ParseBoolean() },
                { "KillObjectFilter", (parser, x) => x.KillObjectFilter = ObjectFilter.Parse(parser) },
                { "ShockWaveClearRadius", (parser, x) => x.ShockWaveClearRadius = parser.ParseBoolean() },
                { "ShockWaveClearMult", (parser, x) => x.ShockWaveClearMult = parser.ParseFloat() },
                { "ShockWaveClearFlingHeight", (parser, x) => x.ShockWaveClearFlingHeight = parser.ParseInteger() },
                { "ShockWaveArcInverted", (parser, x) => x.ShockWaveArcInverted = parser.ParseBoolean() },
                { "CyclonicFactor", (parser, x) => x.CyclonicFactor = parser.ParseFloat() },
                { "AffectHordes", (parser, x) => x.AffectHordes = parser.ParseBoolean() }
            });

        public float HeroResist { get; private set; }
        public float ShockWaveAmount { get; internal set; }
        public float ShockWaveRadius { get; internal set; }
        public float ShockWaveTaperOff { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveArc { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveZMult { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float ShockWaveSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool InvertShockWave { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int DelayTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool FlipDirection { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool OnlyWhenJustDied { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter KillObjectFilter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShockWaveClearRadius { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ShockWaveClearMult { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int ShockWaveClearFlingHeight { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool ShockWaveArcInverted { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float CyclonicFactor { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public bool AffectHordes { get; private set; }
    }
}
