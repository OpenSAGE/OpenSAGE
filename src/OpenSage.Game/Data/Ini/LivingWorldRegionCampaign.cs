using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class LivingWorldRegionCampaign
    {
        internal static LivingWorldRegionCampaign Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldRegionCampaign> FieldParseTable = new IniParseTable<LivingWorldRegionCampaign>
        {
            { "RegionObject", (parser, x) => x.RegionObject = parser.ParseAssetReference() },

            { "ZOffset", (parser, x) => x.ZOffset = parser.ParseInteger() },

            { "RegionConqueredSound", (parser, x) => x.RegionConqueredSound = parser.ParseAssetReference() },

            { "RegionPopupDefaultColor", (parser, x) => x.RegionPopupDefaultColor = parser.ParseColorRgba() },
            { "RegionPopupOverColor", (parser, x) => x.RegionPopupOverColor = parser.ParseColorRgba() },

            { "RegionBonusArmy", (parser, x) => x.RegionBonusArmy = parser.ParseLocalizedStringKey() },
            { "RegionBonusResource", (parser, x) => x.RegionBonusResource = parser.ParseLocalizedStringKey() },
            { "RegionBonusLegendary", (parser, x) => x.RegionBonusLegendary = parser.ParseLocalizedStringKey() },

            { "SmallArmyCommandPoints", (parser, x) => x.SmallArmyCommandPoints = parser.ParseInteger() },
            { "MediumArmyCommandPoints", (parser, x) => x.MediumArmyCommandPoints = parser.ParseInteger() },

            { "ArmyPlacementPos", (parser, x) => x.ArmyPlacementPositions.Add(Coord2D.Parse(parser)) },

            { "EnemyBordersEffect", (parser, x) => x.EnemyBordersEffect = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "FriendlyBordersEffect", (parser, x) => x.FriendlyBordersEffect = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "HilightBordersEffect", (parser, x) => x.HilightBordersEffect = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "ConqueredEffectEvenglow", (parser, x) => x.ConqueredEffectEvenglow = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "ConqueredEffectFlareup", (parser, x) => x.ConqueredEffectFlareup = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoverEffectFlareupOwned", (parser, x) => x.MouseoverEffectFlareupOwned = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoutEffectFlareupOwned", (parser, x) => x.MouseoutEffectFlareupOwned = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoverEffectFlareupContested", (parser, x) => x.MouseoverEffectFlareupContested = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoutEffectFlareupContested", (parser, x) => x.MouseoutEffectFlareupContested = LivingWorldRegionCampaignEffect.Parse(parser) },

            { "Region", (parser, x) => x.Regions.Add(LivingWorldRegionCampaignRegion.Parse(parser)) }
        };

        public string Name { get; private set; }

        public string RegionObject { get; private set; }

        public int ZOffset { get; private set; }

        public string RegionConqueredSound { get; private set; }

        public ColorRgba RegionPopupDefaultColor { get; private set; }
        public ColorRgba RegionPopupOverColor { get; private set; }

        public string RegionBonusArmy { get; private set; }
        public string RegionBonusResource { get; private set; }
        public string RegionBonusLegendary { get; private set; }

        public int SmallArmyCommandPoints { get; private set; }
        public int MediumArmyCommandPoints { get; private set; }

        public List<Coord2D> ArmyPlacementPositions { get; } = new List<Coord2D>();

        public LivingWorldRegionCampaignEffect EnemyBordersEffect { get; private set; }
        public LivingWorldRegionCampaignEffect FriendlyBordersEffect { get; private set; }
        public LivingWorldRegionCampaignEffect HilightBordersEffect { get; private set; }
        public LivingWorldRegionCampaignEffect ConqueredEffectEvenglow { get; private set; }
        public LivingWorldRegionCampaignEffect ConqueredEffectFlareup { get; private set; }
        public LivingWorldRegionCampaignEffect MouseoverEffectFlareupOwned { get; private set; }
        public LivingWorldRegionCampaignEffect MouseoutEffectFlareupOwned { get; private set; }
        public LivingWorldRegionCampaignEffect MouseoverEffectFlareupContested { get; private set; }
        public LivingWorldRegionCampaignEffect MouseoutEffectFlareupContested { get; private set; }

        public List<LivingWorldRegionCampaignRegion> Regions { get; } = new List<LivingWorldRegionCampaignRegion>();
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class LivingWorldRegionCampaignEffect
    {
        internal static LivingWorldRegionCampaignEffect Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldRegionCampaignEffect> FieldParseTable = new IniParseTable<LivingWorldRegionCampaignEffect>
        {
            { "Geometry", (parser, x) => x.Geometries.Add(parser.ParseAssetReference()) },
            { "ControlPoint", (parser, x) => x.ControlPoints.Add(LivingWorldControlPoint.Parse(parser)) }
        };

        public List<string> Geometries { get; } = new List<string>();
        public List<LivingWorldControlPoint> ControlPoints { get; } = new List<LivingWorldControlPoint>();
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class LivingWorldControlPoint
    {
        internal static LivingWorldControlPoint Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldControlPoint> FieldParseTable = new IniParseTable<LivingWorldControlPoint>
        {
            { "Position", (parser, x) => x.Position = Coord3D.Parse(parser) },
            { "Angle", (parser, x) => x.Angle = parser.ParseInteger() },
            { "EaseIn", (parser, x) => x.EaseIn = parser.ParseFloat() },
            { "EaseOut", (parser, x) => x.EaseOut = parser.ParseFloat() },
            { "Time", (parser, x) => x.Time = parser.ParseFloat() }
        };

        public Coord3D Position { get; private set; }

        /// <summary>
        /// Angle around Z in degrees.
        /// </summary>
        public int Angle { get; private set; }

        public float EaseIn { get; private set; }
        public float EaseOut { get; private set; }

        /// <summary>
        /// Absolute time in seconds from start.
        /// </summary>
        public float Time { get; private set; }
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class LivingWorldRegionCampaignRegion
    {
        internal static LivingWorldRegionCampaignRegion Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldRegionCampaignRegion> FieldParseTable = new IniParseTable<LivingWorldRegionCampaignRegion>
        {
            { "DisplayName", (parser, x) => x.DisplayName = parser.ParseAssetReference() },
            { "ConqueredNotice", (parser, x) => x.ConqueredNotice = parser.ParseLocalizedStringKey() },
            { "MapName", (parser, x) => x.MapName = parser.ParseAssetReference() },
            { "MissionObjectiveTag", (parser, x) => x.MissionObjectiveTags.Add(parser.ParseLocalizedStringKey()) },
            { "BonusMissionObjectiveTag", (parser, x) => x.BonusMissionObjectiveTags.Add(parser.ParseLocalizedStringKey()) },
            { "MovieNameFirstTime", (parser, x) => x.MovieNameFirstTime = parser.ParseAssetReference() },
            { "MovieNameRepeat", (parser, x) => x.MovieNameRepeat = parser.ParseAssetReference() },
            { "SkirmishStillImage", (parser, x) => x.SkirmishStillImage = parser.ParseAssetReference() },
            { "SkirmishVoiceTrack", (parser, x) => x.SkirmishVoiceTrack = parser.ParseAssetReference() },
            { "SkirmishMusicTrack", (parser, x) => x.SkirmishMusicTrack = parser.ParseAssetReference() },
            { "RegionPortrait", (parser, x) => x.RegionPortrait = parser.ParseAssetReference() },
            { "CustomUIPopupPoint", (parser, x) => x.CustomUIPopupPoint = parser.ParseBoolean() },
            { "UIPopupPoint", (parser, x) => x.UIPopupPoint = Coord2D.Parse(parser) },
            { "SubObject", (parser, x) => x.SubObject = parser.ParseAssetReference() },
            { "RegionBonus", (parser, x) => x.RegionBonus = parser.ParseAssetReference() },
            { "LegendaryBonus", (parser, x) => x.LegendaryBonus = parser.ParseInteger() },
            { "ResourceBonus", (parser, x) => x.ResourceBonus = parser.ParseInteger() },
            { "ArmyBonus", (parser, x) => x.ArmyBonus = parser.ParseInteger() },
            { "DisplayActNum", (parser, x) => x.DisplayActNum = parser.ParseInteger() },
            { "UnpackCamps", (parser, x) => x.UnpackCamps = parser.ParseBoolean() },
            { "CustomCenterPoint", (parser, x) => x.CustomCenterPoint = parser.ParseBoolean() },
            { "CenterPoint", (parser, x) => x.CenterPoint = Coord2D.Parse(parser) },
            { "EndOfCampaign", (parser, x) => x.EndOfCampaign = parser.ParseBoolean() },

            { "ConnectsTo", (parser, x) => x.ConnectsTo = parser.ParseAssetReferenceArray() },

            { "SkirmishOpponent", (parser, x) => x.SkirmishOpponents.Add(LivingWorldRegionCampaignRegionSkirmishOpponent.Parse(parser)) },

            { "ArmyPlacementPos", (parser, x) => x.ArmyPlacementPositions.Add(Coord2D.Parse(parser)) },
        };

        public string Name { get; private set; }

        public string DisplayName { get; private set; }
        public string ConqueredNotice { get; private set; }
        public string MapName { get; private set; }
        public List<string> MissionObjectiveTags { get; } = new List<string>();
        public List<string> BonusMissionObjectiveTags { get; } = new List<string>();
        public string MovieNameFirstTime { get; private set; }
        public string MovieNameRepeat { get; private set; }
        public string SkirmishStillImage { get; private set; }
        public string SkirmishVoiceTrack { get; private set; }
        public string SkirmishMusicTrack { get; private set; }
        public string SubObject { get; private set; }
        public string RegionPortrait { get; private set; }
        public bool CustomUIPopupPoint { get; private set; }
        public Coord2D UIPopupPoint { get; private set; }
        public string RegionBonus { get; private set; }
        public int LegendaryBonus { get; private set; }
        public int ResourceBonus { get; private set; }
        public int ArmyBonus { get; private set; }
        public int DisplayActNum { get; private set; }
        public bool UnpackCamps { get; private set; }
        public bool CustomCenterPoint { get; private set; }
        public Coord2D CenterPoint { get; private set; }
        public bool EndOfCampaign { get; private set; }

        public string[] ConnectsTo { get; private set; }

        public List<LivingWorldRegionCampaignRegionSkirmishOpponent> SkirmishOpponents { get; } = new List<LivingWorldRegionCampaignRegionSkirmishOpponent>();

        public List<Coord2D> ArmyPlacementPositions { get; } = new List<Coord2D>();
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class LivingWorldRegionCampaignRegionSkirmishOpponent
    {
        internal static LivingWorldRegionCampaignRegionSkirmishOpponent Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldRegionCampaignRegionSkirmishOpponent> FieldParseTable = new IniParseTable<LivingWorldRegionCampaignRegionSkirmishOpponent>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() },
            { "Faction", (parser, x) => x.Faction = parser.ParseAssetReference() },
            { "StartPosition", (parser, x) => x.StartPosition = parser.ParseInteger() },
            { "TeamID", (parser, x) => x.TeamID = parser.ParseInteger() },
            { "AllowInActs", (parser, x) => x.AllowInActs = parser.ParseAssetReferenceArray() },
            { "IsPlayer", (parser, x) => x.IsPlayer = parser.ParseBoolean() },
        };

        public string Name { get; private set; }
        public string Faction { get; private set; }
        public int StartPosition { get; private set; }
        public int TeamID { get; private set; }
        public string[] AllowInActs { get; private set; }
        public bool IsPlayer { get; private set; }
    }
}
