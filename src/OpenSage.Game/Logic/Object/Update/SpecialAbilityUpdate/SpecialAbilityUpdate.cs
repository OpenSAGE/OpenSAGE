using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public class SpecialAbilityUpdate : UpdateModule
    {
        // TODO

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown3 = reader.ReadBoolean();

            var unknown8 = reader.ReadUInt32();

            reader.SkipUnknownBytes(4);

            var unknown4 = reader.ReadUInt32();

            reader.SkipUnknownBytes(16);

            var unknown1 = reader.ReadBoolean();
            if (!unknown1)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(7);

            var unknown5 = reader.ReadUInt32();

            var unknown7 = reader.ReadBoolean();

            reader.SkipUnknownBytes(1);

            var unknown9 = reader.ReadBoolean();

            var unknown2 = reader.ReadBoolean();
            if (!unknown2)
            {
                throw new InvalidStateException();
            }

            var unknown6 = reader.ReadSingle();
        }
    }

    /// <summary>
    /// Allows the use of PACKING and UNPACKING condition states.
    /// </summary>
    public class SpecialAbilityUpdateModuleData : UpdateModuleData
    {
        internal static SpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<SpecialAbilityUpdateModuleData>
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
            { "PreTriggerUnstealthTime", (parser, x) => x.PreTriggerUnstealthTime = parser.ParseInteger() },
            { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLOS = parser.ParseBoolean() },
            { "NeedToFaceTarget", (parser, x) => x.NeedToFaceTarget = parser.ParseBoolean() },
            { "PersistenceRequiresRecharge", (parser, x) => x.PersistenceRequiresRecharge = parser.ParseBoolean() },
            { "ChargeAttackSpeedBoost", (parser, x) => x.ChargeAttackSpeedBoost = parser.ParseBoolean() },
            { "Instant", (parser, x) => x.Instant = parser.ParseBoolean() },
            { "CustomAnimAndDuration", (parser, x) => x.CustomAnimAndDuration = AnimAndDuration.Parse(parser) },
            { "ContactPointOverride", (parser, x) => x.ContactPointOverride = parser.ParseEnum<ContactPointType>() },
            { "UnpackingVariation", (parser, x) => x.UnpackingVariation = parser.ParseInteger() },
            { "TriggerAttributeModifier", (parser, x) => x.TriggerAttributeModifier = parser.ParseIdentifier() },
            { "AttributeModifierDuration", (parser, x) => x.AttributeModifierDuration = parser.ParseInteger() },
            { "KillAttributeModifierOnExit", (parser, x) => x.KillAttributeModifierOnExit = parser.ParseBoolean() },
            { "IgnoreFacingCheck", (parser, x) => x.IgnoreFacingCheck = parser.ParseBoolean() },
            { "EffectRange", (parser, x) => x.EffectRange = parser.ParseInteger() },
            { "GrabPassengerAnimAndDuration", (parser, x) => x.GrabPassengerAnimAndDuration = AnimAndDuration.Parse(parser) },
            { "GrabPassengerHealGainPercent", (parser, x) => x.GrabPassengerHealGainPercent = parser.ParseFloat() },
            { "PersistentCount", (parser, x) => x.PersistentCount = parser.ParseInteger() },
            { "RejectedConditions", (parser, x) => x.RejectedConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "RequiredConditions", (parser, x) => x.RequiredConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "KillAttributeModifierOnRejected", (parser, x) => x.KillAttributeModifierOnRejected = parser.ParseBoolean() },
            { "TriggerModelCondition", (parser, x) => x.TriggerModelCondition = parser.ParseAttributeEnum<ModelConditionFlag>("ModelConditionState") },
            { "TriggerModelConditionDuration", (parser, x) => x.TriggerModelConditionDuration = parser.ParseInteger() },
            { "FreezeAfterTriggerDuration", (parser, x) => x.FreezeAfterTriggerDuration = parser.ParseInteger() }
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
        public bool ApproachRequiresLOS { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool NeedToFaceTarget { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool PersistenceRequiresRecharge { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool ChargeAttackSpeedBoost { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool Instant { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public AnimAndDuration CustomAnimAndDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ContactPointType ContactPointOverride { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int UnpackingVariation { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string TriggerAttributeModifier { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int AttributeModifierDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool KillAttributeModifierOnExit { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool IgnoreFacingCheck { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int EffectRange { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public AnimAndDuration GrabPassengerAnimAndDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float GrabPassengerHealGainPercent { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int PersistentCount { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> RejectedConditions { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> RequiredConditions { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool KillAttributeModifierOnRejected { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public ModelConditionFlag TriggerModelCondition { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public int TriggerModelConditionDuration { get; private set; }

        public int FreezeAfterTriggerDuration { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new SpecialAbilityUpdate();
        }
    }
}
