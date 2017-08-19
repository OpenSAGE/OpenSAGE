using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires <see cref="Locomotor.LocomotorWorksWhenDead"/> = <code>true</code> on the object's
    /// Locomotor to function correctly.
    /// </summary>
    public sealed class JetSlowDeathBehavior : ObjectBehavior
    {
        internal static JetSlowDeathBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<JetSlowDeathBehavior> FieldParseTable = new IniParseTable<JetSlowDeathBehavior>
        {
            { "FXOnGroundDeath", (parser, x) => x.FXOnGroundDeath = parser.ParseAssetReference() },
            { "OCLOnGroundDeath", (parser, x) => x.OCLOnGroundDeath = parser.ParseAssetReference() },
            { "DestructionDelay", (parser, x) => x.DestructionDelay = parser.ParseInteger() },
            { "RollRate", (parser, x) => x.RollRate = parser.ParseFloat() },
            { "RollRateDelta", (parser, x) => x.RollRateDelta = parser.ParsePercentage() },
            { "PitchRate", (parser, x) => x.PitchRate = parser.ParseFloat() },
            { "FallHowFast", (parser, x) => x.FallHowFast = parser.ParsePercentage() },
            { "FXInitialDeath", (parser, x) => x.FXInitialDeath = parser.ParseAssetReference() },
            { "OCLInitialDeath", (parser, x) => x.OCLInitialDeath = parser.ParseAssetReference() },
            { "DelaySecondaryFromInitialDeath", (parser, x) => x.DelaySecondaryFromInitialDeath = parser.ParseInteger() },
            { "FXSecondary", (parser, x) => x.FXSecondary = parser.ParseAssetReference() },
            { "OCLSecondary", (parser, x) => x.OCLSecondary = parser.ParseAssetReference() },
            { "FXHitGround", (parser, x) => x.FXHitGround = parser.ParseAssetReference() },
            { "OCLHitGround", (parser, x) => x.OCLHitGround = parser.ParseAssetReference() },
            { "DelayFinalBlowUpFromHitGround", (parser, x) => x.DelayFinalBlowUpFromHitGround = parser.ParseInteger() },
            { "FXFinalBlowUp", (parser, x) => x.FXFinalBlowUp = parser.ParseAssetReference() },
            { "OCLFinalBlowUp", (parser, x) => x.OCLFinalBlowUp = parser.ParseAssetReference() }
        };

        public string FXOnGroundDeath { get; private set; }
        public string OCLOnGroundDeath { get; private set; }
        public int DestructionDelay { get; private set; }
        public float RollRate { get; private set; }
        public float RollRateDelta { get; private set; }
        public float PitchRate { get; private set; }
        public float FallHowFast { get; private set; }
        public string FXInitialDeath { get; private set; }
        public string OCLInitialDeath { get; private set; }
        public int DelaySecondaryFromInitialDeath { get; private set; }
        public string FXSecondary { get; private set; }
        public string OCLSecondary { get; private set; }
        public string FXHitGround { get; private set; }
        public string OCLHitGround { get; private set; }
        public int DelayFinalBlowUpFromHitGround { get; private set; }
        public string FXFinalBlowUp { get; private set; }
        public string OCLFinalBlowUp { get; private set; }
    }
}
