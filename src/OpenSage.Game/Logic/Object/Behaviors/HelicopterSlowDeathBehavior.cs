using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class HelicopterSlowDeathBehaviorModuleData : SlowDeathBehaviorModuleData
    {
        internal static new HelicopterSlowDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<HelicopterSlowDeathBehaviorModuleData> FieldParseTable = SlowDeathBehaviorModuleData.FieldParseTable
            .Concat(new IniParseTable<HelicopterSlowDeathBehaviorModuleData>
            {
                { "SpiralOrbitTurnRate", (parser, x) => x.SpiralOrbitTurnRate = parser.ParseFloat() },
                { "SpiralOrbitForwardSpeed", (parser, x) => x.SpiralOrbitForwardSpeed = parser.ParseFloat() },
                { "SpiralOrbitForwardSpeedDamping", (parser, x) => x.SpiralOrbitForwardSpeedDamping = parser.ParseFloat() },
                { "MaxBraking", (parser, x) => x.MaxBraking = parser.ParseInteger() },
                { "SoundDeathLoop", (parser, x) => x.SoundDeathLoop = parser.ParseAssetReference() },
                { "MinSelfSpin", (parser, x) => x.MinSelfSpin = parser.ParseInteger() },
                { "MaxSelfSpin", (parser, x) => x.MaxSelfSpin = parser.ParseInteger() },
                { "SelfSpinUpdateDelay", (parser, x) => x.SelfSpinUpdateDelay = parser.ParseInteger() },
                { "SelfSpinUpdateAmount", (parser, x) => x.SelfSpinUpdateAmount = parser.ParseInteger() },
                { "FallHowFast", (parser, x) => x.FallHowFast = parser.ParsePercentage() },
                { "MinBladeFlyOffDelay", (parser, x) => x.MinBladeFlyOffDelay = parser.ParseInteger() },
                { "MaxBladeFlyOffDelay", (parser, x) => x.MaxBladeFlyOffDelay = parser.ParseInteger() },
                { "AttachParticle", (parser, x) => x.AttachParticle = parser.ParseAssetReference() },
                { "AttachParticleBone", (parser, x) => x.AttachParticle = parser.ParseBoneName() },
                { "BladeObjectName", (parser, x) => x.BladeObjectName = parser.ParseAssetReference() },
                { "BladeBoneName", (parser, x) => x.BladeBoneName = parser.ParseBoneName() },
                { "OCLEjectPilot", (parser, x) => x.OCLEjectPilot = parser.ParseAssetReference() },
                { "FXBlade", (parser, x) => x.FXBlade = parser.ParseAssetReference() },
                { "OCLBlade", (parser, x) => x.OCLBlade = parser.ParseAssetReference() },
                { "FXHitGround", (parser, x) => x.FXHitGround = parser.ParseAssetReference() },
                { "OCLHitGround", (parser, x) => x.OCLHitGround = parser.ParseAssetReference() },
                { "FXFinalBlowUp", (parser, x) => x.FXFinalBlowUp = parser.ParseAssetReference() },
                { "OCLFinalBlowUp", (parser, x) => x.OCLFinalBlowUp = parser.ParseAssetReference() },
                { "DelayFromGroundToFinalDeath", (parser, x) => x.DelayFromGroundToFinalDeath = parser.ParseInteger() },
                { "FinalRubbleObject", (parser, x) => x.FinalRubbleObject = parser.ParseAssetReference() },
            });

        public float SpiralOrbitTurnRate { get; private set; }
        public float SpiralOrbitForwardSpeed { get; private set; }
        public float SpiralOrbitForwardSpeedDamping { get; private set; }
        public int MaxBraking { get; private set; }
        public string SoundDeathLoop { get; private set; }
        public int MinSelfSpin { get; private set; }
        public int MaxSelfSpin { get; private set; }
        public int SelfSpinUpdateDelay { get; private set; }
        public int SelfSpinUpdateAmount { get; private set; }
        public Percentage FallHowFast { get; private set; }
        public int MinBladeFlyOffDelay { get; private set; }
        public int MaxBladeFlyOffDelay { get; private set; }
        public string AttachParticle { get; private set; }
        public string AttachParticleBone { get; private set; }
        public string BladeObjectName { get; private set; }
        public string BladeBoneName { get; private set; }
        public string OCLEjectPilot { get; private set; }
        public string FXBlade { get; private set; }
        public string OCLBlade { get; private set; }
        public string FXHitGround { get; private set; }
        public string OCLHitGround { get; private set; }
        public string FXFinalBlowUp { get; private set; }
        public string OCLFinalBlowUp { get; private set; }
        public int DelayFromGroundToFinalDeath { get; private set; }
        public string FinalRubbleObject { get; private set; }
    }
}
