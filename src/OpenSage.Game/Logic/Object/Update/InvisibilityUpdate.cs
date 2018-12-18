using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class InvisibilityUpdateModuleData : UpdateModuleData
    {
        internal static InvisibilityUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InvisibilityUpdateModuleData> FieldParseTable = new IniParseTable<InvisibilityUpdateModuleData>
        {
            { "UpdatePeriod", (parser, x) => x.UpdatePeriod = parser.ParseInteger() },
            { "StartsActive", (parser, x) => x.StartsActive  = parser.ParseBoolean() },
            { "InvisibilityNugget", (parser, x) => x.InvisibilityNugget = InvisibilityNugget.Parse(parser) },
            { "RequiredUpgrades", (parser, x) => x.RequiredUpgrades = parser.ParseAssetReferenceArray() },
            { "ForbiddenUpgrades", (parser, x) => x.ForbiddenUpgrades = parser.ParseAssetReferenceArray() },
            { "UnitSpecificSoundNameToUseAsVoiceMoveToStealthyArea", (parser, x) =>
                x.UnitSpecificSoundNameToUseAsVoiceMoveToStealthyArea = parser.ParseAssetReference() },
            { "UnitSpecificSoundNameToUseAsVoiceEnterStateMoveToStealthyArea", (parser, x) =>
                x.UnitSpecificSoundNameToUseAsVoiceEnterStateMoveToStealthyArea = parser.ParseAssetReference() },
        };

        public int UpdatePeriod { get; private set; }
        public bool StartsActive { get; private set; }
        public InvisibilityNugget InvisibilityNugget { get; private set; }
        public string[] RequiredUpgrades { get; private set; }
        public string[] ForbiddenUpgrades { get; private set; }
        public string UnitSpecificSoundNameToUseAsVoiceMoveToStealthyArea { get; private set; }
        public string UnitSpecificSoundNameToUseAsVoiceEnterStateMoveToStealthyArea { get; private set; }
    }

    public sealed class InvisibilityNugget
    {
        internal static InvisibilityNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InvisibilityNugget> FieldParseTable = new IniParseTable<InvisibilityNugget>
        {
            { "InvisibilityType", (parser, x) => x.Type = parser.ParseEnum<InvisibilityType>() },
            { "DetectionRange", (parser, x) => x.DetectionRange  = parser.ParseFloat() },
            { "Options", (parser, x) => x.Options = parser.ParseEnum<InvisibilityOptions>() },
            { "ForbiddenConditions", (parser, x) => x.ForbiddenConditions = parser.ParseEnumBitArray<ModelConditionFlag>() },
            { "BecomeStealthedFX", (parser, x) => x.BecomeStealthedFX = parser.ParseAssetReference() },
            { "ExitStealthFX", (parser, x) => x.ExitStealthFX = parser.ParseAssetReference() },
            { "ForbiddenWeaponConditions", (parser, x) => x.ForbiddenWeaponConditions = parser.ParseEnumBitArray<WeaponSetConditions>() }
        };

        public InvisibilityType Type { get; private set; }
        public float DetectionRange { get; private set; }
        public InvisibilityOptions Options { get; private set; }
        public BitArray<ModelConditionFlag> ForbiddenConditions { get; private set; }
        public string BecomeStealthedFX { get; private set; }
        public string ExitStealthFX { get; private set; }
        public BitArray<WeaponSetConditions> ForbiddenWeaponConditions { get; private set; }
    }

    public enum InvisibilityType
    {
        [IniEnum("CAMOUFLAGE")]
        Camouflage,

        [IniEnum("STEALTH")]
        Stealth,
    }

    public enum InvisibilityOptions
    {
        [IniEnum("DETECTED_BY_FRIENDLIES")]
        DetectedByFriendlies,

        [IniEnum("UNTOGGLE_HIDDEN_WHEN_LEAVING_STEALTH")]
        UntoggleHiddenWhenLeavingStealth,

        [IniEnum("ALLOW_NEAR_TREES")]
        AllowNearTrees,
    }
}
