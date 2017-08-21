using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of PACKING and UNPACKING condition states.
    /// </summary>
    public sealed class SpecialAbilityUpdate : ObjectBehavior
    {
        internal static SpecialAbilityUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpecialAbilityUpdate> FieldParseTable = new IniParseTable<SpecialAbilityUpdate>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
            { "AbilityAbortRange", (parser, x) => x.AbilityAbortRange = parser.ParseFloat() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "EffectDuration", (parser, x) => x.EffectDuration = parser.ParseInteger() },
            { "EffectValue", (parser, x) => x.EffectValue = parser.ParseInteger() },
            { "DisableFXParticleSystem", (parser, x) => x.DisableFXParticleSystem = parser.ParseAssetReference() },
            { "SpecialObject", (parser, x) => x.SpecialObject = parser.ParseAssetReference() },
            { "SpecialObjectAttachToBone", (parser, x) => x.SpecialObject = parser.ParseBoneName() },
            { "MaxSpecialObjects", (parser, x) => x.MaxSpecialObjects = parser.ParseInteger() },
            { "SpecialObjectsPersistWhenOwnerDies", (parser, x) => x.SpecialObjectsPersistWhenOwnerDies = parser.ParseBoolean() },
            { "AlwaysValidateSpecialObjects", (parser, x) => x.AlwaysValidateSpecialObjects = parser.ParseBoolean() },
            { "SpecialObjectsPersistent", (parser, x) => x.SpecialObjectsPersistent = parser.ParseBoolean() },
            { "UniqueSpecialObjectTargets", (parser, x) => x.UniqueSpecialObjectTargets = parser.ParseBoolean() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "DoCaptureFX", (parser, x) => x.DoCaptureFX = parser.ParseBoolean() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "SkipPackingWithNoTarget", (parser, x) => x.SkipPackingWithNoTarget = parser.ParseBoolean() },
            { "FlipOwnerAfterUnpacking", (parser, x) => x.FlipOwnerAfterUnpacking = parser.ParseBoolean() },
            { "FleeRangeAfterCompletion", (parser, x) => x.FleeRangeAfterCompletion = parser.ParseFloat() },
            { "PackSound", (parser, x) => x.PackSound = parser.ParseAssetReference() },
            { "UnpackSound", (parser, x) => x.UnpackSound = parser.ParseAssetReference() },
            { "TriggerSound", (parser, x) => x.TriggerSound = parser.ParseAssetReference() },
            { "PrepSoundLoop", (parser, x) => x.PrepSoundLoop = parser.ParseAssetReference() },
            { "LoseStealthOnTrigger", (parser, x) => x.LoseStealthOnTrigger = parser.ParseBoolean() },
            { "PreTriggerUnstealthTime", (parser, x) => x.PreTriggerUnstealthTime = parser.ParseInteger() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public float StartAbilityRange { get; private set; }
        public float AbilityAbortRange { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int EffectDuration { get; private set; }
        public int EffectValue { get; private set; }
        public string DisableFXParticleSystem { get; private set; }
        public string SpecialObject { get; private set; }
        public string SpecialObjectAttachToBone { get; private set; }
        public int MaxSpecialObjects { get; private set; }
        public bool SpecialObjectsPersistWhenOwnerDies { get; private set; }
        public bool AlwaysValidateSpecialObjects { get; private set; }
        public bool SpecialObjectsPersistent { get; private set; }
        public bool UniqueSpecialObjectTargets { get; private set; }
        public int UnpackTime { get; private set; }
        public int PackTime { get; private set; }
        public bool DoCaptureFX { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public bool SkipPackingWithNoTarget { get; private set; }
        public bool FlipOwnerAfterUnpacking { get; private set; }
        public float FleeRangeAfterCompletion { get; private set; }
        public string PackSound { get; private set; }
        public string UnpackSound { get; private set; }
        public string TriggerSound { get; private set; }
        public string PrepSoundLoop { get; private set; }
        public bool LoseStealthOnTrigger { get; private set; }
        public int PreTriggerUnstealthTime { get; private set; }
    }
}
