using OpenSage.Data.Ini.Parser;
using System.Numerics;
using OpenSage.Mathematics;
using System.Collections.Generic;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldMapInfo
    {
        internal static LivingWorldMapInfo Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LivingWorldMapInfo> FieldParseTable = new IniParseTable<LivingWorldMapInfo>
        {
            { "MapObject", (parser, x) => x.MapObject = parser.ParseAssetReference() },
            { "CloudBorderSubObject", (parser, x) => x.CloudBorderSubObject = parser.ParseAssetReference() },
            { "TextLayerSubObject", (parser, x) => x.TextLayerSubObject = parser.ParseAssetReference() },
            { "Center", (parser, x) => x.Center = parser.ParseVector2() },
            { "Extent", (parser, x) => x.Extent = parser.ParseVector2() },
            { "CameraBoundX", (parser, x) => x.CameraBoundX = parser.ParseFloat() },
            { "CameraBoundY", (parser, x) => x.CameraBoundY = parser.ParseFloat() },
            { "Ambient", (parser, x) => x.Ambient = parser.ParseColorRgb() },
            { "SunDir", (parser, x) => x.SunDir = parser.ParseVector2() },
            { "SunRGB", (parser, x) => x.SunRGB = parser.ParseColorRgb() },
            { "Accent1Dir", (parser, x) => x.Accent1Dir = parser.ParseVector2() },
            { "Accent1RGB", (parser, x) => x.Accent1RGB  = parser.ParseColorRgb() },
            { "Accent2Dir", (parser, x) => x.Accent2Dir = parser.ParseVector2() },
            { "Accent2RGB", (parser, x) => x.Accent2RGB  = parser.ParseColorRgb() },
            { "GondorBanner", (parser, x) => x.GondorBanner = parser.ParseAssetReference() },
            { "RohanBanner", (parser, x) => x.RohanBanner = parser.ParseAssetReference() },
            { "MordorBanner", (parser, x) => x.MordorBanner = parser.ParseAssetReference() },
            { "IsengardBanner", (parser, x) => x.IsengardBanner = parser.ParseAssetReference() },
            { "NeutralBanner", (parser, x) => x.NeutralBanner = parser.ParseAssetReference() },
            { "GondorAnts", (parser, x) => x.GondorAnts = parser.ParseAssetReference() },
            { "RohanAnts", (parser, x) => x.RohanAnts = parser.ParseAssetReference() },
            { "MordorAnts", (parser, x) => x.MordorAnts = parser.ParseAssetReference() },
            { "IsengardAnts", (parser, x) => x.IsengardAnts = parser.ParseAssetReference() },
            { "NeutralAnts", (parser, x) => x.NeutralAnts = parser.ParseAssetReference() },
            { "BannerScaleSpeed", (parser, x) => x.BannerScaleSpeed = parser.ParseFloat() },
            { "BannerMaxScale", (parser, x) => x.BannerMaxScale = parser.ParseFloat() },
            { "BannerTiltAngle", (parser, x) => x.BannerTiltAngle = parser.ParseFloat() },
            { "BannerHeight", (parser, x) => x.BannerHeight = parser.ParseFloat() },
            { "ArmyHeight", (parser, x) => x.ArmyHeight = parser.ParseFloat() },
            { "BeaconHeight", (parser, x) => x.BeaconHeight = parser.ParseFloat() },
            { "BattleMarker", (parser, x) => x.BattleMarker = parser.ParseAssetReference() },
            { "PalantirMarker", (parser, x) => x.PalantirMarker = parser.ParseAssetReference() },
            { "BattleMarkerCreatedSound", (parser, x) => x.BattleMarkerCreatedSound = parser.ParseAssetReference() },
            { "EnterMapSound", (parser, x) => x.EnterMapSound = parser.ParseAssetReference() },
            { "AnimRays", (parser, x) => x.AnimRays = parser.ParseAssetReference() },
            { "AnimRaysColor", (parser, x) => x.AnimRaysColor  = parser.ParseColorRgb() },
            { "AnimRaysPartSys", (parser, x) => x.AnimRaysPartSys = parser.ParseAssetReference() },
            { "AnimRaysPartSysOffset", (parser, x) => x.AnimRaysPartSysOffset = parser.ParseVector3() },
            { "AnimRaysColorScale", (parser, x) => x.AnimRaysColorScale = parser.ParseFloat() },
            { "AnimRaysEffectShells", (parser, x) => x.AnimRaysEffectShells = parser.ParseInteger() },
            { "AnimRaysEffectDiameter", (parser, x) => x.AnimRaysEffectDiameter = parser.ParseInteger() },
            { "AnimRaysEffectLifetime", (parser, x) => x.AnimRaysEffectLifetime = parser.ParseInteger() },
            { "AnimRaysCreateSound", (parser, x) => x.AnimRaysCreateSound = parser.ParseAssetReference() },
            { "AnimCloud", (parser, x) => x.AnimCloud = parser.ParseAssetReference() },
            { "AnimCloudPartSys", (parser, x) => x.AnimCloudPartSys = parser.ParseAssetReference() },
            { "NumAnimClouds", (parser, x) => x.NumAnimClouds = parser.ParseInteger() },
            { "AnimCloudRegionMin", (parser, x) => x.AnimCloudRegionMin = parser.ParseVector3() },
            { "AnimCloudRegionMax", (parser, x) => x.AnimCloudRegionMax = parser.ParseVector3() },
            { "AnimCloudLifetime", (parser, x) => x.AnimCloudLifetime = parser.ParseInteger() },
            { "MordorCloud", (parser, x) => x.MordorCloud = parser.ParseAssetReference() },
            { "EmbersPartSys", (parser, x) => x.EmbersPartSys = parser.ParseAssetReference() },
            { "CloudPos", (parser, x) => x.CloudPos = parser.ParseVector3() },
            { "CloudGrowthPos", (parser, x) => x.CloudGrowthPos = parser.ParseVector3() },
            { "CloudGrowthRate", (parser, x) => x.CloudGrowthRate = parser.ParseInteger() },
            { "CloudInitialSize", (parser, x) => x.CloudInitialSize = parser.ParseFloat() },
            { "CloudGrowthSize", (parser, x) => x.CloudGrowthSize = parser.ParseFloat() },
            { "CloudInitialOpacity", (parser, x) => x.CloudInitialOpacity = parser.ParseFloat() },
            { "ShadowColor", (parser, x) => x.ShadowColor = parser.ParseColorRgb() },
            { "EnableMapShadows", (parser, x) => x.EnableMapShadows = parser.ParseBoolean() },
            { "ArmySelectedIconObject", (parser, x) => x.ArmySelectedIconObject = parser.ParseAssetReference() },
            { "ArmyHilightedIconObject", (parser, x) => x.ArmyHilightedIconObject = parser.ParseAssetReference() },
            { "ArmySelectedFadeInEnd", (parser, x) => x.ArmySelectedFadeInEnd = parser.ParseInteger() },
            { "ArmySelectedFadeOutStart", (parser, x) => x.ArmySelectedFadeOutStart = parser.ParseInteger() },
            { "ArmySelectedFadeInStart", (parser, x) => x.ArmySelectedFadeInStart = parser.ParseInteger() },
            { "ArmySelectedFadeOutEnd", (parser, x) => x.ArmySelectedFadeOutEnd = parser.ParseInteger() },
            { "ArmyHilightedFadeInTime", (parser, x) => x.ArmyHilightedFadeInTime = parser.ParseInteger() },
            { "ArmyHilightedFadeOutTime", (parser, x) => x.ArmyHilightedFadeOutTime = parser.ParseInteger() },
            { "ArmySoldierLarge", (parser, x) => x.ArmySoldierLarge = parser.ParseAssetReference() },
            { "ArmySoldierMedium", (parser, x) => x.ArmySoldierMedium = parser.ParseAssetReference() },
            { "ArmySoldierSmall", (parser, x) => x.ArmySoldierSmall = parser.ParseAssetReference() },

            { "EyeTower", (parser, x) => x.EyeTower = EyeTower.Parse(parser) },
            { "NumWorldTiles", (parser, x) => x.NumWorldTiles = parser.ParseInteger() },
            { "AddShadowSubObject", (parser, x) => x.ShadowSubObjects.Add(parser.ParseAssetReference()) },
            { "AptCenter", (parser, x) => x.AptCenter = parser.ParseVector2() },
            { "AptZoom", (parser, x) => x.AptZoom = parser.ParseFloat() },
            { "AptPitch", (parser, x) => x.AptPitch = parser.ParseFloat() },
            { "ClickScrollThreshold", (parser, x) => x.ClickScrollThreshold = parser.ParseInteger() },
            { "MouseWheelZoomPerTick", (parser, x) => x.MouseWheelZoomPerTick = parser.ParseFloat() },
            { "MouseWheelZoomDampenFactor", (parser, x) => x.MouseWheelZoomDampenFactor = parser.ParseFloat() },
            { "AutoScrollSpeed", (parser, x) => x.AutoScrollSpeed = parser.ParseFloat() },
            { "MaxAutoScrollTime", (parser, x) => x.MaxAutoScrollTime = parser.ParseFloat() },
            { "NumPointsPerArmyLine", (parser, x) => x.NumPointsPerArmyLine = parser.ParseInteger() },
            { "ArmyLineHeightBias", (parser, x) => x.ArmyLineHeightBias = parser.ParseFloat() },
            { "ArmyLineWidth", (parser, x) => x.ArmyLineWidth = parser.ParseFloat() },
            { "ArmyLineColorAttacking", (parser, x) => x.ArmyLineColorAttacking  = parser.ParseColorRgb() },
            { "ArmyLineColorNeutral", (parser, x) => x.ArmyLineColorNeutral  = parser.ParseColorRgb() },
            { "ArmyLineColorAllied", (parser, x) => x.ArmyLineColorAllied  = parser.ParseColorRgb() },
            { "ArmyLineTextureName", (parser, x) => x.ArmyLineTextureName = parser.ParseAssetReference() },
            { "MenBanner", (parser, x) => x.MenBanner = parser.ParseAssetReference() },
            { "ElvesBanner", (parser, x) => x.ElvesBanner = parser.ParseAssetReference() },
            { "DwarvesBanner", (parser, x) => x.DwarvesBanner = parser.ParseAssetReference() },
            { "WildBanner", (parser, x) => x.WildBanner = parser.ParseAssetReference() },
            { "MenAnts", (parser, x) => x.MenAnts = parser.ParseAssetReference() },
            { "ElvesAnts", (parser, x) => x.ElvesAnts = parser.ParseAssetReference() },
            { "DwarvesAnts", (parser, x) => x.DwarvesAnts = parser.ParseAssetReference() },
            { "WildAnts", (parser, x) => x.WildAnts = parser.ParseAssetReference() },
            { "DefaultArmyMoveSpeed", (parser, x) => x.DefaultArmyMoveSpeed = parser.ParseFloat() },
            { "HeroArmyIconDiameter", (parser, x) => x.HeroArmyIconDiameter = parser.ParseFloat() },
            { "RegionAwardDisputeMarker", (parser, x) => x.RegionAwardDisputeMarker = parser.ParseAssetReference() },
            { "AngmarBanner", (parser, x) => x.AngmarBanner = parser.ParseAssetReference() },
            { "AngmarAnts", (parser, x) => x.AngmarAnts = parser.ParseAssetReference() },
        };

        public string MapObject { get; private set; }
        public string CloudBorderSubObject { get; private set; }
        public string TextLayerSubObject { get; private set; }
        public Vector2 Center { get; private set; }
        public Vector2 Extent { get; private set; }
        public float CameraBoundX { get; private set; }
        public float CameraBoundY { get; private set; }
        public ColorRgb Ambient { get; private set; }
        public Vector2 SunDir { get; private set; }
        public ColorRgb SunRGB { get; private set; }

        public Vector2 Accent1Dir { get; private set; }
        public ColorRgb Accent1RGB { get; private set; }
        public Vector2 Accent2Dir { get; private set; }
        public ColorRgb Accent2RGB { get; private set; }
        public string GondorBanner { get; private set; }
        public string RohanBanner { get; private set; }
        public string MordorBanner { get; private set; }
        public string IsengardBanner { get; private set; }
        public string NeutralBanner { get; private set; }

        public string GondorAnts { get; private set; }
        public string RohanAnts { get; private set; }
        public string MordorAnts { get; private set; }
        public string IsengardAnts { get; private set; }
        public string NeutralAnts { get; private set; }
        public float BannerScaleSpeed { get; private set; }
        public float BannerMaxScale { get; private set; }
        public float BannerTiltAngle { get; private set; }
        public float BannerHeight { get; private set; }

        public float ArmyHeight { get; private set; }
        public float BeaconHeight { get; private set; }
        public string BattleMarker { get; private set; }
        public string PalantirMarker { get; private set; }
        public string BattleMarkerCreatedSound { get; private set; }
        public string EnterMapSound { get; private set; }


        public string AnimRays { get; private set; }
        public ColorRgb AnimRaysColor { get; private set; }
        public string AnimRaysPartSys { get; private set; }
        public Vector3 AnimRaysPartSysOffset { get; private set; }
        public float AnimRaysColorScale { get; private set; }
        public int AnimRaysEffectShells { get; private set; }
        public int AnimRaysEffectDiameter { get; private set; }
        public int AnimRaysEffectLifetime { get; private set; }
        public string AnimRaysCreateSound { get; private set; }
  
        public string AnimCloud { get; private set; }
        public string AnimCloudPartSys { get; private set; }
        public int NumAnimClouds { get; private set; }
        public Vector3 AnimCloudRegionMin { get; private set; }
        public Vector3 AnimCloudRegionMax { get; private set; }
        public int AnimCloudLifetime { get; private set; }
  
        public string MordorCloud { get; private set; }
        public string EmbersPartSys { get; private set; }
        public Vector3 CloudPos { get; private set; }
        public Vector3 CloudGrowthPos { get; private set; }
        public int CloudGrowthRate { get; private set; }
        public float CloudInitialSize { get; private set; }
        public float CloudGrowthSize { get; private set; }
        public float CloudInitialOpacity { get; private set; }

        public ColorRgb ShadowColor { get; private set; }
        public bool EnableMapShadows { get; private set; }
        public string ArmySelectedIconObject { get; private set; }
        public string ArmyHilightedIconObject { get; private set; }
        public int ArmySelectedFadeInStart { get; private set; }
        public int ArmySelectedFadeInEnd { get; private set; }
        public int ArmySelectedFadeOutStart { get; private set; }

        public int ArmySelectedFadeOutEnd { get; private set; }
        public int ArmyHilightedFadeInTime { get; private set; }
        public int ArmyHilightedFadeOutTime { get; private set; }
        public string ArmySoldierLarge { get; private set; }
        public string ArmySoldierMedium { get; private set; }
        public string ArmySoldierSmall { get; private set; }
        public EyeTower EyeTower { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int NumWorldTiles { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<string> ShadowSubObjects { get; } = new List<string>();

        [AddedIn(SageGame.Bfme2)]
        public Vector2 AptCenter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float AptZoom { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float AptPitch { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int ClickScrollThreshold { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float MouseWheelZoomPerTick { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float MouseWheelZoomDampenFactor { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float AutoScrollSpeed { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float MaxAutoScrollTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int NumPointsPerArmyLine { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ArmyLineHeightBias { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float ArmyLineWidth { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ColorRgb ArmyLineColorAttacking { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ColorRgb ArmyLineColorNeutral { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ColorRgb ArmyLineColorAllied { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ArmyLineTextureName { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string MenBanner { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ElvesBanner { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string DwarvesBanner { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string WildBanner { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string MenAnts { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string ElvesAnts { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string DwarvesAnts { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string WildAnts { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float DefaultArmyMoveSpeed { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public float HeroArmyIconDiameter { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string RegionAwardDisputeMarker { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string AngmarBanner { get; private set; }

        [AddedIn(SageGame.Bfme2Rotwk)]
        public string AngmarAnts { get; private set; }
    }

    public sealed class EyeTower
    {
        internal static EyeTower Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EyeTower> FieldParseTable = new IniParseTable<EyeTower>
        {
            { "PupilAnimObject", (parser, x) => x.PupilAnimObject = parser.ParseAssetReference() },
            { "PupilBeamAnimObject", (parser, x) => x.PupilBeamAnimObject = parser.ParseAssetReference() },
            { "EyeDecalAnimObject", (parser, x) => x.EyeDecalAnimObject = parser.ParseAssetReference() },
            { "EyeDecalBeamAnimObject", (parser, x) => x.EyeDecalBeamAnimObject = parser.ParseAssetReference() },
        };

        public string PupilAnimObject { get; private set; }
        public string PupilBeamAnimObject { get; private set; }
        public string EyeDecalAnimObject { get; private set; }
        public string EyeDecalBeamAnimObject { get; private set; }
    }
}
