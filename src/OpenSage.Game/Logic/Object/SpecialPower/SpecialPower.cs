using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using System.Numerics;
using OpenSage.Content;

namespace OpenSage.Logic.Object
{
    public class SpecialPowerModule : BehaviorModule
    {
        internal virtual void Activate(Vector3 position)
        {
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown = reader.ReadBytes(16);
        }
    }

    public class SpecialPowerModuleData : BehaviorModuleData
    {
        public override ModuleKind ModuleKind => ModuleKind.SpecialPower;

        internal static SpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static readonly IniParseTable<SpecialPowerModuleData> FieldParseTable = new IniParseTable<SpecialPowerModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPower = parser.ParseSpecialPowerReference() },
            { "StartsPaused", (parser, x) => x.StartsPaused = parser.ParseBoolean() },
            { "UpdateModuleStartsAttack", (parser, x) => x.UpdateModuleStartsAttack = parser.ParseBoolean() },
            { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() },
            { "InitiateSound2", (parser, x) => x.InitiateSound2 = parser.ParseAssetReference() },
            { "AttributeModifier", (parser, x) => x.AttributeModifier = parser.ParseAssetReference() },
            { "AttributeModifierAffectsSelf", (parser, x) => x.AttributeModifierAffectsSelf = parser.ParseBoolean() },
            { "InitiateFX", (parser, x) => x.InitiateFX = parser.ParseAssetReference() },
            { "AntiCategory", (parser, x) => x.AntiCategory = parser.ParseEnum<ModifierCategory>() },
            { "AttributeModifierRange", (parser, x) => x.AttributeModifierRange = parser.ParseFloat() },
            { "AttributeModifierFX", (parser, x) => x.AttributeModifierFX = parser.ParseAssetReference() },
            { "TriggerFX", (parser, x) => x.TriggerFX = parser.ParseAssetReference() },
            { "SetModelCondition", (parser, x) => x.SetModelCondition = parser.ParseAttributeEnum<ModelConditionFlag>("ModelConditionState") },
            { "SetModelConditionTime", (parser, x) => x.SetModelConditionTime = parser.ParseFloat() },
            { "AttributeModifierAffects", (parser, x) => x.AttributeModifierAffects = ObjectFilter.Parse(parser) },
            { "AvailableAtStart", (parser, x) => x.AvailableAtStart = parser.ParseBoolean() },
            { "TargetAllSides", (parser, x) => x.TargetAllSides = parser.ParseBoolean() },
            { "AffectAllies", (parser, x) => x.AffectAllies = parser.ParseBoolean() },
            { "AttributeModifierWeatherBased", (parser, x) => x.AttributeModifierWeatherBased = parser.ParseBoolean() },
            { "TargetEnemy", (parser, x) => x.TargetEnemy = parser.ParseBoolean() },
            { "OnTriggerRechargeSpecialPower", (parser, x) => x.OnTriggerRechargeSpecialPower = parser.ParseAssetReference() },
            { "DisableDuringAnimDuration", (parser, x) => x.DisableDuringAnimDuration = parser.ParseBoolean() },
            { "RequirementsFilterMPSkirmish", (parser, x) => x.RequirementsFilterMPSkirmish = ObjectFilter.Parse(parser) },
            { "RequirementsFilterStrategic", (parser, x) => x.RequirementsFilterStrategic = ObjectFilter.Parse(parser) }
        };

        public LazyAssetReference<SpecialPower> SpecialPower { get; private set; }
        public bool StartsPaused { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool UpdateModuleStartsAttack { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string InitiateSound { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string AttributeModifier { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AttributeModifierAffectsSelf { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string InitiateFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ModifierCategory AntiCategory { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float AttributeModifierRange { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string AttributeModifierFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string InitiateSound2 { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string TriggerFX { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ModelConditionFlag SetModelCondition { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float SetModelConditionTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public ObjectFilter AttributeModifierAffects { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool TargetAllSides { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AvailableAtStart { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AffectAllies { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool AttributeModifierWeatherBased { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool TargetEnemy { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string OnTriggerRechargeSpecialPower { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool DisableDuringAnimDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter RequirementsFilterMPSkirmish { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter RequirementsFilterStrategic { get; private set; }
    }
}
