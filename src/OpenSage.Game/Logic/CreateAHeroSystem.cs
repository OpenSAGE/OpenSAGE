using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class CreateAHeroSystem
    {
        internal static CreateAHeroSystem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateAHeroSystem> FieldParseTable = new IniParseTable<CreateAHeroSystem>
        {
            { "CreateAHeroMapModeUpgradeName", (parser, x) => x.CreateAHeroMapModeUpgradeName = parser.ParseString() },
            { "CreateAHeroGameModeUpgradeName", (parser, x) => x.CreateAHeroGameModeUpgradeName = parser.ParseString() },
            { "CanBuildCreateAHeroUpgradeName", (parser, x) => x.CanBuildCreateAHeroUpgradeName = parser.ParseString() },
            { "CommandSetTemplate", (parser, x) => x.CommandSetTemplate = parser.ParseAssetReference() },
            { "StratigicDefeatStatName", (parser, x) => x.StratigicDefeatStatName = parser.ParseString() },
            { "StratigicVictoryStatName", (parser, x) => x.StratigicVictoryStatName = parser.ParseString() },
            { "StratigicMPDefeatStatName", (parser, x) => x.StratigicMPDefeatStatName = parser.ParseString() },
            { "StratigicMPVictoryStatName", (parser, x) => x.StratigicMPVictoryStatName = parser.ParseString() },
            { "SkirmishDefeatStatName", (parser, x) => x.SkirmishDefeatStatName = parser.ParseString() },
            { "SkirmishVictoryStatName", (parser, x) => x.SkirmishVictoryStatName = parser.ParseString() },
            { "OpenPlayDefeatStatName", (parser, x) => x.OpenPlayDefeatStatName = parser.ParseString() },
            { "OpenPlayVictoryStatName", (parser, x) => x.OpenPlayVictoryStatName = parser.ParseString() },
            { "StratigicCampainVictoryStatName", (parser, x) => x.StratigicCampainVictoryStatName = parser.ParseString() },
            { "WeaponGroupName", (parser, x) => x.WeaponGroupName = parser.ParseString() },
            { "SelectedCheerAninName", (parser, x) => x.SelectedCheerAninName = parser.ParseString() },
            { "ExamineWeaponAninName", (parser, x) => x.ExamineWeaponAninName = parser.ParseString() },
            { "ExamineSelfAninName", (parser, x) => x.ExamineSelfAninName = parser.ParseString() },
            { "ExamineAnimTweakValue", (parser, x) => x.ExamineAnimTweakValue = parser.ParseInteger() },
            { "SpecialAnimPercentChance", (parser, x) => x.SpecialAnimPercentChance = parser.ParseFloat() },
            { "HeroRevivalDiscount", (parser, x) => x.HeroRevivalDiscount = parser.ParsePercentage() },
            { "SpecialPowerDiscountPerLevel", (parser, x) => x.SpecialPowerDiscountPerLevel = parser.ParsePercentage() },
            { "CreateAHeroBling", (parser, x) => x.CreateAHeroBlings.Add(CreateAHeroBling.Parse(parser)) },
            { "CreateAHeroBlingBinder", (parser, x) => x.CreateAHeroBlingBinders.Add(CreateAHeroBlingBinder.Parse(parser)) },
            { "CreateAHeroClass", (parser, x) => x.CreateAHeroClasses.Add(CreateAHeroClass.Parse(parser)) }
        };

        public string CreateAHeroMapModeUpgradeName { get; private set; }
        public string CreateAHeroGameModeUpgradeName { get; private set; }
        public string CanBuildCreateAHeroUpgradeName { get; private set; }
        public string CommandSetTemplate { get; private set; }
        public string StratigicDefeatStatName { get; private set; }
        public string StratigicVictoryStatName { get; private set; }
        public string StratigicMPDefeatStatName { get; private set; }
        public string StratigicMPVictoryStatName { get; private set; }
        public string SkirmishDefeatStatName { get; private set; }
        public string SkirmishVictoryStatName { get; private set; }
        public string OpenPlayDefeatStatName { get; private set; }
        public string OpenPlayVictoryStatName { get; private set; }
        public string StratigicCampainVictoryStatName { get; private set; }
        public string WeaponGroupName { get; private set; }
        public string SelectedCheerAninName { get; private set; }
        public string ExamineWeaponAninName { get; private set; }
        public string ExamineSelfAninName { get; private set; }
        public int ExamineAnimTweakValue { get; private set; }
        public float SpecialAnimPercentChance { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public Percentage HeroRevivalDiscount { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public Percentage SpecialPowerDiscountPerLevel { get; private set; }

        public List<CreateAHeroBling> CreateAHeroBlings { get; } = new List<CreateAHeroBling>();
        public List<CreateAHeroBlingBinder> CreateAHeroBlingBinders { get; } = new List<CreateAHeroBlingBinder>();
        public List<CreateAHeroClass> CreateAHeroClasses { get; } = new List<CreateAHeroClass>();
    }

    public class CreateAHeroBling
    {
        internal static CreateAHeroBling Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateAHeroBling> FieldParseTable = new IniParseTable<CreateAHeroBling>
        {
            { "NameTag", (parser, x) => x.NameTag = parser.ParseLocalizedStringKey() },
            { "DescriptionTag", (parser, x) => x.DescriptionTag = parser.ParseLocalizedStringKey() },
            { "GroupName", (parser, x) => x.GroupName = parser.ParseString() },
            { "BlingUpgradeName", (parser, x) => x.BlingUpgradeName = parser.ParseAssetReference() },
        };

        public string NameTag { get; private set; }
        public string DescriptionTag { get; private set; }
        public string GroupName { get; private set; }
        public string BlingUpgradeName { get; private set; }
    }

    public class CreateAHeroBlingBinder
    {
        internal static CreateAHeroBlingBinder Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateAHeroBlingBinder> FieldParseTable = new IniParseTable<CreateAHeroBlingBinder>
        {
            { "GroupName", (parser, x) => x.GroupName = parser.ParseString() },
            { "DescriptionTag", (parser, x) => x.DescriptionTag = parser.ParseLocalizedStringKey() },
            { "LabelTag", (parser, x) => x.LabelTag = parser.ParseLocalizedStringKey() },
            { "UISlot", (parser, x) => x.UISlot = parser.ParseInteger() },
            { "BlingType", (parser, x) => x.BlingType = parser.ParseEnum<BlingType>() }
        };

        public string GroupName { get; private set; }
        public string DescriptionTag { get; private set; }
        public string LabelTag { get; private set; }
        public int UISlot { get; private set; }
        public BlingType BlingType { get; private set; }
    }

    public class CreateAHeroClass
    {
        internal static CreateAHeroClass Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CreateAHeroClass> FieldParseTable = new IniParseTable<CreateAHeroClass>
        {
            { "NameTag", (parser, x) => x.NameTag = parser.ParseLocalizedStringKey() },
            { "DescriptionTag", (parser, x) => x.DescriptionTag = parser.ParseLocalizedStringKey() },
            { "PowersDescTag", (parser, x) => x.PowersDescTag = parser.ParseLocalizedStringKey() },
            { "UpgradeName", (parser, x) => x.UpgradeName = parser.ParseString() },
            { "IconImage", (parser, x) => x.IconImage = parser.ParseAssetReference() },
            { "SubClass", (parser, x) => x.SubClasses.Add(SubClass.Parse(parser)) }
        };

        public string NameTag { get; private set; }
        public string DescriptionTag { get; private set; }
        public string PowersDescTag { get; private set; }
        public string UpgradeName { get; private set; }
        public string IconImage { get; private set; }

        public List<SubClass> SubClasses { get; } = new List<SubClass>();
    }

    public class SubClass
    {
        internal static SubClass Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SubClass> FieldParseTable = new IniParseTable<SubClass>
        {
            { "NameTag", (parser, x) => x.NameTag = parser.ParseLocalizedStringKey() },
            { "DescriptionTag", (parser, x) => x.DescriptionTag = parser.ParseLocalizedStringKey() },
            { "PowersDescTag", (parser, x) => x.PowersDescTag = parser.ParseLocalizedStringKey() },
            { "UpgradeName", (parser, x) => x.UpgradeName = parser.ParseString() },
            { "IconImage", (parser, x) => x.IconImage = parser.ParseAssetReference() },
            { "ButtonImage", (parser, x) => x.ButtonImage = parser.ParseAssetReference() },
            { "DefaultFaction", (parser, x) => x.DefaultFaction = parser.ParseString() },
            { "UsableFactions", (parser, x) => x.UsableFactions = parser.ParseAssetReferenceArray() },
            { "SpendableAttributePoints", (parser, x) => x.SpendableAttributePoints = parser.ParseInteger() },
            { "Awards", (parser, x) => x.Awards = parser.ParseAssetReferenceArray() },
            { "Stats", (parser, x) => x.Stats = parser.ParseAssetReferenceArray() },
            { "BlingUpgrades", (parser, x) => x.BlingUpgrades = parser.ParseAssetReferenceArray() },
            { "DefaultPrimaryColor", (parser, x) => x.DefaultPrimaryColor = parser.ParseColorRgb() },
            { "DefaultSecondaryColor", (parser, x) => x.DefaultSecondaryColor = parser.ParseColorRgb() },
            { "DefaultTertiaryColor", (parser, x) => x.DefaultTertiaryColor = parser.ParseColorRgb() },
            { "Attribute", (parser, x) => x.Attributes.Add(SubClassAttribute.Parse(parser)) },
            { "ViewInfo", (parser, x) => x.ViewInfo = ViewInfo.Parse(parser) },
        };

        public string NameTag { get; private set; }
        public string DescriptionTag { get; private set; }
        public string PowersDescTag { get; private set; }
        public string UpgradeName { get; private set; }
        public string IconImage { get; private set; }
        public string ButtonImage { get; private set; }
        public string DefaultFaction { get; private set; }
        public string[] UsableFactions { get; private set; }
        public int SpendableAttributePoints { get; private set; }

        public string[] Awards { get; private set; }
        public string[] Stats { get; private set; }
        public string[] BlingUpgrades { get; private set; }

        public ColorRgb DefaultPrimaryColor { get; private set; }
        public ColorRgb DefaultSecondaryColor { get; private set; }
        public ColorRgb DefaultTertiaryColor { get; private set; }
        public List<SubClassAttribute> Attributes { get; } = new List<SubClassAttribute>();
        public ViewInfo ViewInfo { get; private set; }
    }

    public class SubClassAttribute
    {
        internal static SubClassAttribute Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SubClassAttribute> FieldParseTable = new IniParseTable<SubClassAttribute>
        {
            { "GroupName", (parser, x) => x.GroupName = parser.ParseString() },
            { "MinValueUpgrade", (parser, x) => x.MinValueUpgrade = parser.ParseAssetReference() },
            { "MaxValueUpgrade", (parser, x) => x.MaxValueUpgrade = parser.ParseAssetReference() },
            { "DefaultValueUpgrade", (parser, x) => x.DefaultValueUpgrade = parser.ParseAssetReference() },
        };

        public string GroupName { get; private set; }
        public string MinValueUpgrade { get; private set; }
        public string MaxValueUpgrade { get; private set; }
        public string DefaultValueUpgrade { get; private set; }
    }

    public class ViewInfo
    {
        internal static ViewInfo Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ViewInfo> FieldParseTable = new IniParseTable<ViewInfo>
        {
            { "FarPitch", (parser, x) => x.FarPitch = parser.ParseFloat() },
            { "FarZoom", (parser, x) => x.FarZoom = parser.ParseFloat() },
            { "FarFloor", (parser, x) => x.FarFloor = parser.ParseFloat() },
            { "FarDist", (parser, x) => x.FarDist = parser.ParseFloat() },
            { "FarShift", (parser, x) => x.FarShift = parser.ParseFloat() },
            { "NearPitch", (parser, x) => x.NearPitch = parser.ParseFloat() },
            { "NearZoom", (parser, x) => x.NearZoom = parser.ParseFloat() },
            { "NearFloor", (parser, x) => x.NearFloor = parser.ParseFloat() },
            { "NearDist", (parser, x) => x.NearDist = parser.ParseFloat() },
            { "NearShift", (parser, x) => x.NearShift = parser.ParseFloat() },
            { "CloseUpPitch", (parser, x) => x.CloseUpPitch = parser.ParseFloat() },
            { "CloseUpZoom", (parser, x) => x.CloseUpZoom = parser.ParseFloat() },
            { "CloseUpFloor", (parser, x) => x.CloseUpFloor = parser.ParseFloat() },
            { "CloseUpDist", (parser, x) => x.CloseUpDist = parser.ParseFloat() },
            { "CloseUpShift", (parser, x) => x.CloseUpShift = parser.ParseFloat() },
            { "PortraitPitch", (parser, x) => x.PortraitPitch = parser.ParseFloat() },
            { "PortraitZoom", (parser, x) => x.PortraitZoom = parser.ParseFloat() },
            { "PortraitFloor", (parser, x) => x.PortraitFloor = parser.ParseFloat() },
            { "PortraitDist", (parser, x) => x.PortraitDist = parser.ParseFloat() },
            { "PortraitShift", (parser, x) => x.PortraitShift = parser.ParseFloat() },
            { "NormalCam", (parser, x) => x.NormalCam = parser.ParseFloat() },
            { "MapLocation", (parser, x) => x.MapLocation = parser.ParseInteger() },
            { "CameraAngle", (parser, x) => x.CameraAngle = parser.ParseFloat() }
        };

        public float FarPitch { get; private set; }
        public float FarZoom { get; private set; }
        public float FarFloor { get; private set; }
        public float FarDist { get; private set; }
        public float FarShift { get; private set; }
        public float NearPitch { get; private set; }
        public float NearZoom { get; private set; }
        public float NearFloor { get; private set; }
        public float NearDist { get; private set; }
        public float NearShift { get; private set; }
        public float CloseUpPitch { get; private set; }
        public float CloseUpZoom { get; private set; }
        public float CloseUpFloor { get; private set; }
        public float CloseUpDist { get; private set; }

        public float CloseUpShift { get; private set; }
        public float PortraitPitch { get; private set; }
        public float PortraitZoom { get; private set; }
        public float PortraitFloor { get; private set; }
        public float PortraitDist { get; private set; }
        public float PortraitShift { get; private set; }
        public float NormalCam { get; private set; }
        public float CameraAngle { get; private set; }

        public float MapLocation { get; private set; }
    }

    public enum BlingType
    {
        [IniEnum("ATTRIBUTE")]
        Attribute,

        [IniEnum("APPEARANCE")]
        Appearance,
    }
}
