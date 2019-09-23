using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;
using System.Numerics;

namespace OpenSage.LivingWorld
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldRegionCampaign
    {
        internal static LivingWorldRegionCampaign Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
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

            { "ArmyPlacementPos", (parser, x) => x.ArmyPlacementPositions.Add(parser.ParseVector2()) },

            { "EnemyBordersEffect", (parser, x) => x.EnemyBordersEffect = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "FriendlyBordersEffect", (parser, x) => x.FriendlyBordersEffect = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "HilightBordersEffect", (parser, x) => x.HilightBordersEffect = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "ConqueredEffectEvenglow", (parser, x) => x.ConqueredEffectEvenglow = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "ConqueredEffectFlareup", (parser, x) => x.ConqueredEffectFlareup = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoverEffectFlareupOwned", (parser, x) => x.MouseoverEffectFlareupOwned = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoutEffectFlareupOwned", (parser, x) => x.MouseoutEffectFlareupOwned = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoverEffectFlareupContested", (parser, x) => x.MouseoverEffectFlareupContested = LivingWorldRegionCampaignEffect.Parse(parser) },
            { "MouseoutEffectFlareupContested", (parser, x) => x.MouseoutEffectFlareupContested = LivingWorldRegionCampaignEffect.Parse(parser) },

            { "Region", (parser, x) => x.Regions.Add(LivingWorldRegionCampaignRegion.Parse(parser)) },
            { "RegionEffectsManagerName", (parser, x) => x.RegionEffectsManagerName = parser.ParseString() },
            { "HeroOnlyArmyCommandPoints", (parser, x) => x.HeroOnlyArmyCommandPoints = parser.ParseInteger() },
            { "ConcurrentRegionBonus", (parser, x) => x.ConcurrentRegionBonuses.Add(ConcurrentRegionBonus.Parse(parser)) },
            { "ArmyRetreatRounds", (parser, x) => x.ArmyRetreatRounds = parser.ParseInteger() }
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

        public List<Vector2> ArmyPlacementPositions { get; } = new List<Vector2>();

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

        [AddedIn(SageGame.Bfme2)]
        public string RegionEffectsManagerName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int HeroOnlyArmyCommandPoints { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<ConcurrentRegionBonus> ConcurrentRegionBonuses { get; } = new List<ConcurrentRegionBonus>();

        [AddedIn(SageGame.Bfme2)]
        public int ArmyRetreatRounds { get; private set; }
    }

    [AddedIn(SageGame.Bfme)]
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

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldControlPoint
    {
        internal static LivingWorldControlPoint Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldControlPoint> FieldParseTable = new IniParseTable<LivingWorldControlPoint>
        {
            { "Position", (parser, x) => x.Position = parser.ParseVector3() },
            { "Angle", (parser, x) => x.Angle = parser.ParseInteger() },
            { "EaseIn", (parser, x) => x.EaseIn = parser.ParseFloat() },
            { "EaseOut", (parser, x) => x.EaseOut = parser.ParseFloat() },
            { "Time", (parser, x) => x.Time = parser.ParseFloat() }
        };

        public Vector3 Position { get; private set; }

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

    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldRegionCampaignRegion
    {
        internal static LivingWorldRegionCampaignRegion Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
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
            { "UIPopupPoint", (parser, x) => x.UIPopupPoint = parser.ParseVector2() },
            { "SubObject", (parser, x) => x.SubObject = parser.ParseAssetReference() },
            { "RegionBonus", (parser, x) => x.RegionBonus = parser.ParseAssetReference() },
            { "LegendaryBonus", (parser, x) => x.LegendaryBonus = parser.ParseInteger() },
            { "ResourceBonus", (parser, x) => x.ResourceBonus = parser.ParseInteger() },
            { "ArmyBonus", (parser, x) => x.ArmyBonus = parser.ParseInteger() },
            { "DisplayActNum", (parser, x) => x.DisplayActNum = parser.ParseInteger() },
            { "UnpackCamps", (parser, x) => x.UnpackCamps = parser.ParseBoolean() },
            { "CustomCenterPoint", (parser, x) => x.CustomCenterPoint = parser.ParseBoolean() },
            { "CenterPoint", (parser, x) => x.CenterPoint = parser.ParseVector2() },
            { "EndOfCampaign", (parser, x) => x.EndOfCampaign = parser.ParseBoolean() },
            { "SkirmishOpponent", (parser, x) => x.SkirmishOpponents.Add(LivingWorldRegionCampaignRegionSkirmishOpponent.Parse(parser)) },
            { "ArmyPlacementPos", (parser, x) => x.ArmyPlacementPositions.Add(parser.ParseVector2()) },
            { "ExperienceBonus", (parser, x) => x.ExperienceBonus = parser.ParseInteger() },
            { "ConnectsTo", (parser, x) => x.RegionConnections = ParseRegionConnections(parser) },
            { "HeroArmySpot", (parser, x) => x.HeroArmySpots.Add(parser.ParseVector2()) },
            { "GarrisonArmySpot", (parser, x) => x.GarrisonArmySpots.Add(parser.ParseVector2()) },
            { "BuildingSpot", (parser, x) => x.BuildingSpots.Add(parser.ParseVector2()) },
            { "CPLimit", (parser, x) => x.CPLimit = parser.ParseInteger() },
            { "AllyCPLimit", (parser, x) => x.AllyCPLimit = parser.ParseInteger() },
            { "RestrictBuildings", (parser, x) => x.RestrictBuildingsList.Add(RestrictBuildings.Parse(parser)) },
            { "AttackBonus", (parser, x) => x.AttackBonus = parser.ParseInteger() },
            { "CreateAutoFort", (parser, x) => x.CreateAutoFort = parser.ParseBoolean() },
            { "FortressPortrait", (parser, x) => x.FortressPortrait = parser.ParseAssetReference() },
            { "FortressDisplayName", (parser, x) => x.FortressDisplayName = parser.ParseLocalizedStringKey() },
            { "FortressDisplayDescription", (parser, x) => x.FortressDisplayDescription = parser.ParseLocalizedStringKey() },
            { "DefenseBonus", (parser, x) => x.DefenseBonus = parser.ParseInteger() },
            { "FertileTerritoryBonus", (parser, x) => x.FertileTerritoryBonus = parser.ParseInteger() }
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
        public Vector2 UIPopupPoint { get; private set; }
        public string RegionBonus { get; private set; }
        public int LegendaryBonus { get; private set; }
        public int ResourceBonus { get; private set; }
        public int ArmyBonus { get; private set; }
        public int DisplayActNum { get; private set; }
        public bool UnpackCamps { get; private set; }
        public bool CustomCenterPoint { get; private set; }
        public Vector2 CenterPoint { get; private set; }
        public bool EndOfCampaign { get; private set; }
        public List<LivingWorldRegionCampaignRegionSkirmishOpponent> SkirmishOpponents { get; } = new List<LivingWorldRegionCampaignRegionSkirmishOpponent>();
        public List<Vector2> ArmyPlacementPositions { get; } = new List<Vector2>();

        [AddedIn(SageGame.Bfme2)]
        public int ExperienceBonus { get; private set; }

        public List<RegionConnection> RegionConnections { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<Vector2> HeroArmySpots { get; } = new List<Vector2>();

        [AddedIn(SageGame.Bfme2)]
        public List<Vector2> GarrisonArmySpots { get; } = new List<Vector2>();

        [AddedIn(SageGame.Bfme2)]
        public List<Vector2> BuildingSpots { get; } = new List<Vector2>();

        [AddedIn(SageGame.Bfme2)]
        public int CPLimit { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int AllyCPLimit { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<RestrictBuildings> RestrictBuildingsList { get; } = new List<RestrictBuildings>();

        [AddedIn(SageGame.Bfme2)]
        public int AttackBonus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool CreateAutoFort { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string FortressPortrait { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string FortressDisplayName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string FortressDisplayDescription { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int DefenseBonus { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int FertileTerritoryBonus { get; private set; }

        private static List<RegionConnection> ParseRegionConnections(IniParser parser)
        {
            var result = new List<RegionConnection>();
            if (parser.SageGame == SageGame.Bfme)
            {
                var regions = parser.ParseAssetReferenceArray();
                foreach (var region in regions)
                {
                    result.Add(new RegionConnection(region));
                }
                return result;
            }

            parser.GoToNextLine(); //skip '=' and go to next line
            var token = parser.PeekNextTokenOptional();
            while (token.HasValue && token.Value.Text == "Connection")
            {
                result.Add(RegionConnection.Parse(parser));
                parser.GoToNextLine();
                token = parser.PeekNextTokenOptional();
            }
            return result;
        }
    }

    [AddedIn(SageGame.Bfme)]
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

    [AddedIn(SageGame.Bfme2)]
    public class RegionConnection
    {
        public RegionConnection() { }

        public RegionConnection(string region)
        {
            Region = region;
        }

        internal static RegionConnection Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RegionConnection> FieldParseTable = new IniParseTable<RegionConnection>
        {
            { "Region", (parser, x) => x.Region = parser.ParseString() },
            { "DetourPoint", (parser, x) => x.DetourPoints.Add(parser.ParseVector2()) }
        };

        public string Region { get; private set; }
        public List<Vector2> DetourPoints { get; } = new List<Vector2>();
    }

    [AddedIn(SageGame.Bfme2)]
    public class RestrictBuildings
    {
        internal static RestrictBuildings Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RestrictBuildings> FieldParseTable = new IniParseTable<RestrictBuildings>
        {
            { "Buildings", (parser, x) => x.Buildings = parser.ParseAssetReferenceArray() },
            { "NumberAllowed", (parser, x) => x.NumberAllowed = parser.ParseInteger() }
        };

        public string[] Buildings { get; private set; }
        public int NumberAllowed { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class ConcurrentRegionBonus
    {
        internal static ConcurrentRegionBonus Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ConcurrentRegionBonus> FieldParseTable = new IniParseTable<ConcurrentRegionBonus>
        {
            { "Territory", (parser, x) => x.Territory = parser.ParseLocalizedStringKey() },
            { "EffectName", (parser, x) => x.EffectName = parser.ParseString() },
            { "Regions", (parser, x) => x.Regions = parser.ParseAssetReferenceArray() },
            { "ArmyBonus", (parser, x) => x.ArmyBonus = parser.ParseInteger() },
            { "ResourceBonus", (parser, x) => x.ResourceBonus = parser.ParseInteger() },
            { "LegendaryBonus", (parser, x) => x.LegendaryBonus = parser.ParseInteger() },
            { "AttackBonus", (parser, x) => x.AttackBonus = parser.ParseInteger() },
            { "DefenseBonus", (parser, x) => x.DefenseBonus = parser.ParseInteger() },
            { "ExperienceBonus", (parser, x) => x.ExperienceBonus = parser.ParseInteger() },
            { "UnifiedEvaEvent", (parser, x) => x.UnifiedEvaEvent = parser.ParseAssetReference() },
            { "LostEvaEvent", (parser, x) => x.LostEvaEvent = parser.ParseAssetReference() },
            { "LookAtCenter", (parser, x) => x.LookAtCenter = parser.ParseVector2() },
            { "LookAtHeading", (parser, x) => x.LookAtHeading = parser.ParseInteger() },
            { "LookAtZoom", (parser, x) => x.LookAtZoom = parser.ParseFloat() },
            { "ExtraStartResourcesBonus", (parser, x) => x.ExtraStartResourcesBonus = parser.ParseInteger() },
            { "DiscountedHeroUnitsBonus", (parser, x) => x.DiscountedHeroUnitsBonus = parser.ParseInteger() },
            { "DiscountedBarracksUnitsBonus", (parser, x) => x.DiscountedBarracksUnitsBonus = parser.ParseInteger() },
            { "DiscountedSeigeUnitsBonus", (parser, x) => x.DiscountedSeigeUnitsBonus = parser.ParseInteger() },
            { "BuildingDiscountBonus", (parser, x) => x.BuildingDiscountBonus = parser.ParseInteger() },
            { "FreeBuilderBonus", (parser, x) => x.FreeBuilderBonus = parser.ParseInteger() },
            { "FreeInnUnitsBonus", (parser, x) => x.FreeInnUnitsBonus = parser.ParseInteger() }
        };

        public string Territory { get; private set; }
        public string EffectName { get; private set; }
        public string[] Regions { get; private set; }
        public int ArmyBonus { get; private set; }
        public int ResourceBonus { get; private set; }
        public int LegendaryBonus { get; private set; }
        public int AttackBonus { get; private set; }
        public int DefenseBonus { get; private set; }
        public int ExperienceBonus { get; private set; }
        public string UnifiedEvaEvent { get; private set; }
        public string LostEvaEvent { get; private set; }
        public Vector2 LookAtCenter { get; private set; }
        public int LookAtHeading { get; private set; }
        public float LookAtZoom { get; private set; }
        public int ExtraStartResourcesBonus { get; private set; }
        public int DiscountedHeroUnitsBonus { get; private set; }
        public int DiscountedBarracksUnitsBonus { get; private set; }
        public int DiscountedSeigeUnitsBonus { get; private set; }
        public int BuildingDiscountBonus { get; private set; }
        public int FreeBuilderBonus { get; private set; }
        public int FreeInnUnitsBonus { get; private set; }
    }
}
