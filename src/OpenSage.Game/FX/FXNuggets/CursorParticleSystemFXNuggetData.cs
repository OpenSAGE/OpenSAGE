using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CursorParticleSystemFXNuggetData : FXNuggetData
    {
        internal static CursorParticleSystemFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CursorParticleSystemFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<CursorParticleSystemFXNuggetData>
        {
            { "Anim2DTemplateName", (parser, x) => x.Anim2DTemplateName = parser.ParseAssetReference() },
            { "BurstCount", (parser, x) => x.BurstCount = parser.ParseInteger() },
            { "ParticleLife", (parser, x) => x.ParticleLife = parser.ParseRandomVariable() },
            { "SystemLife", (parser, x) => x.SystemLife = parser.ParseRandomVariable() },
            { "DriftVelX", (parser, x) => x.DriftVelX = parser.ParseRandomVariable() },
            { "DriftVelY", (parser, x) => x.DriftVelY = parser.ParseRandomVariable() },
        });

        public string Anim2DTemplateName { get; private set; }
        public int BurstCount { get; private set; }
        public RandomVariable ParticleLife { get; private set; }
        public RandomVariable SystemLife { get; private set; }
        public RandomVariable DriftVelX { get; private set; }
        public RandomVariable DriftVelY { get; private set; }
    }
}
