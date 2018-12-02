using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ArrowStormUpdateModuleData : UpdateModuleData
    {
        internal static ArrowStormUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ArrowStormUpdateModuleData> FieldParseTable = new IniParseTable<ArrowStormUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseIdentifier() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
            { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLos = parser.ParseBoolean() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "ActiveLoopSound", (parser, x) => x.ActiveLoopSound = parser.ParseAssetReference() },
            { "WeaponTemplate", (parser, x) => x.WeaponTemplate = parser.ParseIdentifier() },
            { "TargetRadius", (parser, x) => x.TargetRadius = parser.ParseInteger() },
            { "ShotsPerTarget", (parser, x) => x.ShotsPerTarget = parser.ParseInteger() },
            { "ShotsPerBurst", (parser, x) => x.ShotsPerBurst = parser.ParseInteger() },
            { "MaxShots", (parser, x) => x.MaxShots = parser.ParseInteger() },
            { "ParalyzeDurationWhenAborted", (parser, x) => x.ParalyzeDurationWhenAborted = parser.ParseInteger() },
            { "ParalyzeDurationWhenCompleted", (parser, x) => x.ParalyzeDurationWhenCompleted = parser.ParseInteger() },
            { "CanShootEmptyGround", (parser, x) => x.CanShootEmptyGround = parser.ParseBoolean() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public float StartAbilityRange { get; private set; }
        public int UnpackingVariation { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public bool ApproachRequiresLos { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public string ActiveLoopSound { get; private set; }

        //Specific to ArrowStorm
        public string WeaponTemplate { get; private set; }
        public int TargetRadius { get; private set; }
        public int ShotsPerTarget { get; private set; }
        public int ShotsPerBurst { get; private set; }
        public int MaxShots { get; private set; }
        public int ParalyzeDurationWhenAborted { get; private set; }
        public int ParalyzeDurationWhenCompleted { get; private set; }
        public bool CanShootEmptyGround { get; private set; }
    }
}
