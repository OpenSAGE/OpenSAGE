using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WeaponFireSpecialAbilityUpdateModuleData : BehaviorModuleData
    {
        internal static WeaponFireSpecialAbilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WeaponFireSpecialAbilityUpdateModuleData> FieldParseTable = new IniParseTable<WeaponFireSpecialAbilityUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "WhichSpecialWeapon", (parser, x) => x.WhichSpecialWeapon = parser.ParseInteger() },
            { "SkipContinue", (parser, x) => x.SkipContinue = parser.ParseBoolean() },
            { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
            { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
            { "PersistentPrepTime", (parser, x) => x.PersistentPrepTime = parser.ParseInteger() },
            { "PackTime", (parser, x) => x.PackTime = parser.ParseInteger() },
            { "AwardXPForTriggering", (parser, x) => x.AwardXPForTriggering = parser.ParseInteger() },
            { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
            { "MustFinishAbility", (parser, x) => x.MustFinishAbility = parser.ParseBoolean() },
            { "SpecialWeapon", (parser, x) => x.SpecialWeapon = parser.ParseAssetReference() },
            { "RejectedConditions", (parser, x) => x.RejectedConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLos = parser.ParseBoolean() },
            { "BusyForDuration", (parser, x) => x.BusyForDuration = parser.ParseInteger() },
            { "FreezeAfterTriggerDuration", (parser, x) => x.FreezeAfterTriggerDuration = parser.ParseInteger() },
            { "TriggerSound", (parser, x) => x.TriggerSound = parser.ParseAssetReference() },
            { "PlayWeaponPreFireFX", (parser, x) => x.PlayWeaponPreFireFX = parser.ParseBoolean() },
            { "Instant", (parser, x) => x.Instant = parser.ParseBoolean() },
            { "LoseStealthOnTrigger", (parser, x) => x.LoseStealthOnTrigger = parser.ParseBoolean() },
            { "PreTriggerUnstealthTime", (parser, x) => x.PreTriggerUnstealthTime = parser.ParseInteger() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public int WhichSpecialWeapon { get; private set; }
        public bool SkipContinue { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int PersistentPrepTime { get; private set; }
        public int PackTime { get; private set; }
        public int AwardXPForTriggering { get; private set; }
        public float StartAbilityRange { get; private set; }
        public bool MustFinishAbility { get; private set; }
        public string SpecialWeapon { get; private set; }
        public BitArray<ModelConditionFlag> RejectedConditions { get; private set; }
        public bool ApproachRequiresLos { get; private set; }
        public int BusyForDuration { get; private set; }
        public int FreezeAfterTriggerDuration { get; private set; }
        public string TriggerSound { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool PlayWeaponPreFireFX { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool Instant { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool LoseStealthOnTrigger { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int PreTriggerUnstealthTime { get; private set; }
    }
}
