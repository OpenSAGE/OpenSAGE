using System;
using System.Linq;
using OpenSage.Content;

namespace OpenSage.Data.StreamFS
{
    public enum AssetType : uint
    {
        Achievement = 0x2B49BF71,
        AIBudgetStateDefinition = 0xAC78CE63,
        AIPersonalityDefinition = 0xD6D4F18E,
        AIStrategicStateDefinition = 0x242FF6D4,
        AITargetingHeuristic = 0x373E10FA,
        AmbientStream = 0xEA2C2798,
        ArmorTemplate = 0x3A6C5E8E,
        ArmyDefinition = 0xD5D580F6,
        AttributeModifier = 0xC5E07887,
        [SkipParsingIn(Game = SageGame.Ra3)] // inherited from BaseSingleSound
        AudioEvent = 0x844D7B9F,
        AudioFile = 0x166B084D,
        AudioLod = 0x11D8BAC7,
        [SkipParsingIn(Game = SageGame.Ra3)] // BaseSingleSound's format is a bit different in RA3
        AudioSettings = 0x5608EE71,
        CampaignTemplate = 0x585E034E,
        ConnectionLineManager = 0x64B2184,
        CrowdResponse = 0x17E53184,
        DamageFX = 0xAD3568F5,
        DefaultHotKeys = 0xDEFCA2F6,
        [SkipParsingIn(Game = SageGame.Ra3)] // inherited from BaseSingleSound
        DialogEvent = 0xD414D1C3,
        DynamicGameLod = 0xACEf31A4,
        Environment = 0x2893E309,
        ExperienceLevelTemplate = 0x6FBC4A9F,
        FXList = 0x86682E78,
        FXParticleSystemTemplate = 0x28E7FD7F,
        GameLodPreset = 0x151D037C,
        GameObject = 0x942FFF2D,
        HotKeySlot = 0xA6E6BBA7,
        ImageSequence = 0x21BA45A7,
        InGameUIFixedElementHotKeySlotMap = 0x928F51E4,
        InGameUIGroupSelectionCommandSlots = 0x6114137E,
        InGameUILookAtCommandSlots = 0x395A0FD6,
        InGameUIPlayerPowerCommandSlots = 0x1E0FC59E,
        InGameUISettings = 0x33A671F8,
        InGameUISideBarCommandSlots = 0x98EE2743,
        InGameUITacticalCommandSlots = 0xA7A65DAC,
        InGameUIUnitAbilityCommandSlots = 0xA78E592E,
        InGameUIVoiceChatCommandSlots = 0x245EB4F9,
        IntelDB = 0x1F9865CE,
        LargeGroupAudioMap = 0x1FD451BF,
        LocalBuildListMonitor = 0x12CFE3EF,
        LocomotorTemplate = 0xECC2A1D3,
        LogicCommand = 0x7D464170,
        LogicCommandSet = 0xEC066D65,
        MappableKey = 0x5080A5D8,
        MiscAudio = 0x7B6AE7D5,
        MissionTemplate = 0xCF4AED23,
        Mouse = 0x9687F57B,
        MpGameRules = 0x2C358B80,
        MultiplayerColor = 0x8E28081D,
        MultiplayerSettings = 0xB63AEF0,
        Multisound = 0xA3A7AF37,
        [SkipParsingIn(Game = SageGame.Ra3)] // inherited from BaseSingleSound
        MusicTrack = 0x7046D9F8,
        ObjectCreationList = 0xE86E4D61,
        OnDemandTexture = 0x9312B9AC,
        OnDemandTextureImage = 0xFE0E84BB,
        OnlineChatColors = 0x502EED32,
        PackedTextureImage = 0x56626220,
        PhaseEffect = 0xD99C40A9,
        PlayerPowerButtonTemplateStore = 0x66219699,
        RadiusCursorLibrary = 0x30D2F544,
        ShadowMap = 0xF7CE0BBD,
        SkirmishOpeningMove = 0xECBE73E8,
        SpecialPowerTemplate = 0x81D85EFA,
        StanceTemplate = 0x9684C743,
        StaticGameLod = 0x1FB298D1,
        TargetingCombatChainCompare = 0x559C032,
        TargetingCompareList = 0xBE06A9E5,
        TargetingDistanceCompare = 0x8CA5A7D7,
        TargetingInTurretArcCompare = 0x77AC3B08,
        Texture = 0x21E727DA,
        TheaterOfWarTemplate = 0x1A2DC767,
        UnitAbilityButtonTemplateStore = 0xB408BD4,
        UnitOverlayIconSettings = 0x9053D603,
        UnitTypeIcon = 0xD76B50C7,
        UpgradeTemplate = 0xE1AFE75B,
        VideoEventList = 0x564A9693,
        W3dAnimation = 0x2448AE30,
        W3dCollisionBox = 0xE3181C04,
        W3dContainer = 0xF0F08712,
        W3dHierarchy = 0x61D7EA40,
        W3dMesh = 0xC2B1A262,
        WeaponTemplate = 0x94D4D96E,
        Weather = 0x90D951AD,
    }

    /// <summary>
    /// Some binary assets (<see cref="Asset"/>)
    /// have their format changed between different SAGE games,
    /// and before we implement game-specific parsing of those assets,
    /// OpenSage may run into issues when trying to parse them.
    /// <br/>
    /// This attribute is indeed used on those kind of assets where
    /// currently we do not (yet) have game-specific parsing code,
    /// so we skip parsing them for now, to avoid errors.
    /// <br/>
    /// This is a temporary workaround and should eventually be removed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SkipParsingInAttribute : Attribute
    {
        public SageGame Game { get; init; }
    }

    /// <summary>
    /// Some assets (<see cref="Asset"/>)
    /// have their name changed between different SAGE games,
    /// but they are still almost identical.
    /// <br/>
    /// This attribute provides a way to manually override
    /// type id, instead of calculating it from the type name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class AssetTypeIdOverrideAttribute : Attribute
    {
        public SageGame Game { get; init; }
        public AssetType Type { get; init; }
    }

    public static class AssetTypeUtility
    {
        public static bool ShouldSkipFor(uint enumVal, SageGame game)
        {
            var type = typeof(AssetType);
            var memInfo = type.GetMember(((AssetType) enumVal).ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(SkipParsingInAttribute), false);
            foreach (var attribute in attributes)
            {
                if (((SkipParsingInAttribute) attribute).Game == game)
                {
                    return true;
                }
            }
            return false;
        }

        public static uint GetAssetTypeId<T>(SageGame game) where T : BaseAsset
        {
            var type = typeof(T);
            var attributes = type.GetCustomAttributes(typeof(AssetTypeIdOverrideAttribute), false);
            foreach (var attribute in attributes.Cast<AssetTypeIdOverrideAttribute>())
            {
                if (attribute.Game == game)
                {
                    return (uint) attribute.Type;
                }
            }
            return AssetHash.GetHashCaseSensitive(type.Name);
        }
    }
}
