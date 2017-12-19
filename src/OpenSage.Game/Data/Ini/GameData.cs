using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Map;

namespace OpenSage.Data.Ini
{
    public sealed class GameData
    {
        internal static GameData Parse(IniParser parser)
        {
            return parser.ParseTopLevelBlock(FieldParseTable);
        }

        private static readonly IniParseTable<GameData> FieldParseTable = new IniParseTable<GameData>
        {
            { "ShellMapName", (parser, x) => x.ShellMapName = parser.ParseString() },
            { "MapName", (parser, x) => x.MapName = parser.ParseString() },
            { "MoveHintName", (parser, x) => x.MoveHintName = parser.ParseString() },
            { "ShowProps", (parser, x) => x.ShowProps = parser.ParseBoolean() },
            { "UseTrees", (parser, x) => x.UseTrees = parser.ParseBoolean() },
            { "UseFPSLimit", (parser, x) => x.UseFpsLimit = parser.ParseBoolean() },
            { "FramesPerSecondLimit", (parser, x) => x.FramesPerSecondLimit = parser.ParseInteger() },
            { "UseHighQualityVideo", (parser, x) => x.UseHighQualityVideo = parser.ParseBoolean() },
            { "DisablePixelShader", (parser, x) => x.DisablePixelShader = parser.ParseBoolean() },
            { "ChipsetType", (parser, x) => x.ChipsetType = parser.ParseInteger() },
            { "MaxShellScreens", (parser, x) => x.MaxShellScreens = parser.ParseInteger() },
            { "Wireframe", (parser, x) => x.Wireframe = parser.ParseBoolean() },
            { "UseCloudMap", (parser, x) => x.UseCloudMap = parser.ParseBoolean() },
            { "AllowTreeFading", (parser, x) => x.AllowTreeFading = parser.ParseBoolean() },
            { "UseLightMap", (parser, x) => x.UseLightMap = parser.ParseBoolean() },
            { "BilinearTerrainTex", (parser, x) => x.BilinearTerrainTex = parser.ParseBoolean() },
            { "TrilinearTerrainTex", (parser, x) => x.TrilinearTerrainTex = parser.ParseBoolean() },
            { "AnisotropicTerrainTex", (parser, x) => x.AnisotropicTerrainTex = parser.ParseBoolean() },
            { "MultiPassTerrain", (parser, x) => x.MultiPassTerrain = parser.ParseBoolean() },
            { "AdjustCliffTextures", (parser, x) => x.AdjustCliffTextures = parser.ParseBoolean() },
            { "StretchTerrain", (parser, x) => x.StretchTerrain = parser.ParseBoolean() },
            { "UseHalfHeightMap", (parser, x) => x.UseHalfHeightMap = parser.ParseBoolean() },
            { "ShowObjectHealth", (parser, x) => x.ShowObjectHealth = parser.ParseBoolean() },
            { "HideGarrisonFlags", (parser, x) => x.HideGarrisonFlags = parser.ParseBoolean() },
            { "Use3WayTerrainBlends", (parser, x) => x.Use3WayTerrainBlends = parser.ParseInteger() == 1 },
            { "DrawEntireTerrain", (parser, x) => x.DrawEntireTerrain = parser.ParseBoolean() },
            { "TerrainLOD", (parser, x) => x.TerrainLod = parser.ParseEnum<TerrainLod>() },
            { "TerrainLODTargetTimeMS", (parser, x) => x.TerrainLodTargetTimeMS = parser.ParseInteger() },
            { "TextureReductionFactor", (parser, x) => x.TextureReductionFactor = parser.ParseInteger() },
            { "RightMouseAlwaysScrolls", (parser, x) => x.RightMouseAlwaysScrolls = parser.ParseBoolean() },
            { "UseWaterPlane", (parser, x) => x.UseWaterPlane = parser.ParseBoolean() },
            { "UseCloudPlane", (parser, x) => x.UseCloudPlane = parser.ParseBoolean() },
            { "UseShadowVolumes", (parser, x) => x.UseShadowVolumes = parser.ParseBoolean() },
            { "UseShadowDecals", (parser, x) => x.UseShadowDecals = parser.ParseBoolean() },
            { "UseShadowMapping", (parser, x) => x.UseShadowMapping = parser.ParseBoolean() },
            { "ShowSelectedUnitMarker", (parser, x) => x.ShowSelectedUnitMarker = parser.ParseBoolean() },
            { "UseSimpleHordeDecals", (parser, x) => x.UseSimpleHordeDecals = parser.ParseBoolean() },
            { "UseSimpleMergeDecals", (parser, x) => x.UseSimpleMergeDecals = parser.ParseBoolean() },
            { "OpacityOfSimpleMergeDecals", (parser, x) => x.OpacityOfSimpleMergeDecals = parser.ParsePercentage() },
            { "UseBehindBuildingMarker", (parser, x) => x.UseBehindBuildingMarker = parser.ParseBoolean() },
            { "DefaultOcclusionDelay", (parser, x) => x.DefaultOcclusionDelay = parser.ParseInteger() },
            { "OccludedColorLuminanceScale", (parser, x) => x.OccludedColorLuminanceScale = parser.ParseFloat() },
            { "WaterPositionX", (parser, x) => x.WaterPositionX = parser.ParseFloat() },
            { "WaterPositionY", (parser, x) => x.WaterPositionY = parser.ParseFloat() },
            { "WaterPositionZ", (parser, x) => x.WaterPositionZ = parser.ParseFloat() },
            { "WaterExtentX", (parser, x) => x.WaterExtentX = parser.ParseFloat() },
            { "WaterExtentY", (parser, x) => x.WaterExtentY = parser.ParseFloat() },
            { "WaterType", (parser, x) => x.WaterType = parser.ParseInteger() },

            { "DefaultUnitHealingBuffFxList", (parser, x) => x.DefaultUnitHealingBuffFxList = parser.ParseAssetReference() },
            { "DefaultStructureRepairBuffFxList", (parser, x) => x.DefaultStructureRepairBuffFxList = parser.ParseAssetReference() },

            { "DefaultStructureRubbleHeight", (parser, x) => x.DefaultStructureRubbleHeight = parser.ParseFloat() },

            { "VertexWaterAvailableMaps1", (parser, x) => x.VertexWaterAvailableMaps1 = parser.ParseString() },
            { "VertexWaterHeightClampLow1", (parser, x) => x.VertexWaterHeightClampLow1 = parser.ParseFloat() },
            { "VertexWaterHeightClampHi1", (parser, x) => x.VertexWaterHeightClampHi1 = parser.ParseFloat() },
            { "VertexWaterAngle1", (parser, x) => x.VertexWaterAngle1 = parser.ParseInteger() },
            { "VertexWaterXPosition1", (parser, x) => x.VertexWaterXPosition1 = parser.ParseFloat() },
            { "VertexWaterYPosition1", (parser, x) => x.VertexWaterYPosition1 = parser.ParseFloat() },
            { "VertexWaterZPosition1", (parser, x) => x.VertexWaterZPosition1 = parser.ParseFloat() },
            { "VertexWaterXGridCells1", (parser, x) => x.VertexWaterXGridCells1 = parser.ParseInteger() },
            { "VertexWaterYGridCells1", (parser, x) => x.VertexWaterYGridCells1 = parser.ParseInteger() },
            { "VertexWaterGridSize1", (parser, x) => x.VertexWaterGridSize1 = parser.ParseFloat() },
            { "VertexWaterAttenuationA1", (parser, x) => x.VertexWaterAttenuationA1 = parser.ParseFloat() },
            { "VertexWaterAttenuationB1", (parser, x) => x.VertexWaterAttenuationB1 = parser.ParseFloat() },
            { "VertexWaterAttenuationC1", (parser, x) => x.VertexWaterAttenuationC1 = parser.ParseFloat() },
            { "VertexWaterAttenuationRange1", (parser, x) => x.VertexWaterAttenuationRange1 = parser.ParseFloat() },

            { "VertexWaterAvailableMaps2", (parser, x) => x.VertexWaterAvailableMaps2 = parser.ParseString() },
            { "VertexWaterHeightClampLow2", (parser, x) => x.VertexWaterHeightClampLow2 = parser.ParseFloat() },
            { "VertexWaterHeightClampHi2", (parser, x) => x.VertexWaterHeightClampHi2 = parser.ParseFloat() },
            { "VertexWaterAngle2", (parser, x) => x.VertexWaterAngle2 = parser.ParseInteger() },
            { "VertexWaterXPosition2", (parser, x) => x.VertexWaterXPosition2 = parser.ParseFloat() },
            { "VertexWaterYPosition2", (parser, x) => x.VertexWaterYPosition2 = parser.ParseFloat() },
            { "VertexWaterZPosition2", (parser, x) => x.VertexWaterZPosition2 = parser.ParseFloat() },
            { "VertexWaterXGridCells2", (parser, x) => x.VertexWaterXGridCells2 = parser.ParseInteger() },
            { "VertexWaterYGridCells2", (parser, x) => x.VertexWaterYGridCells2 = parser.ParseInteger() },
            { "VertexWaterGridSize2", (parser, x) => x.VertexWaterGridSize2 = parser.ParseFloat() },
            { "VertexWaterAttenuationA2", (parser, x) => x.VertexWaterAttenuationA2 = parser.ParseFloat() },
            { "VertexWaterAttenuationB2", (parser, x) => x.VertexWaterAttenuationB2 = parser.ParseFloat() },
            { "VertexWaterAttenuationC2", (parser, x) => x.VertexWaterAttenuationC2 = parser.ParseFloat() },
            { "VertexWaterAttenuationRange2", (parser, x) => x.VertexWaterAttenuationRange2 = parser.ParseFloat() },

            { "VertexWaterAvailableMaps3", (parser, x) => x.VertexWaterAvailableMaps3 = parser.ParseString() },
            { "VertexWaterHeightClampLow3", (parser, x) => x.VertexWaterHeightClampLow3 = parser.ParseFloat() },
            { "VertexWaterHeightClampHi3", (parser, x) => x.VertexWaterHeightClampHi3 = parser.ParseFloat() },
            { "VertexWaterAngle3", (parser, x) => x.VertexWaterAngle3 = parser.ParseInteger() },
            { "VertexWaterXPosition3", (parser, x) => x.VertexWaterXPosition3 = parser.ParseFloat() },
            { "VertexWaterYPosition3", (parser, x) => x.VertexWaterYPosition3 = parser.ParseFloat() },
            { "VertexWaterZPosition3", (parser, x) => x.VertexWaterZPosition3 = parser.ParseFloat() },
            { "VertexWaterXGridCells3", (parser, x) => x.VertexWaterXGridCells3 = parser.ParseInteger() },
            { "VertexWaterYGridCells3", (parser, x) => x.VertexWaterYGridCells3 = parser.ParseInteger() },
            { "VertexWaterGridSize3", (parser, x) => x.VertexWaterGridSize3 = parser.ParseFloat() },
            { "VertexWaterAttenuationA3", (parser, x) => x.VertexWaterAttenuationA3 = parser.ParseFloat() },
            { "VertexWaterAttenuationB3", (parser, x) => x.VertexWaterAttenuationB3 = parser.ParseFloat() },
            { "VertexWaterAttenuationC3", (parser, x) => x.VertexWaterAttenuationC3 = parser.ParseFloat() },
            { "VertexWaterAttenuationRange3", (parser, x) => x.VertexWaterAttenuationRange3 = parser.ParseFloat() },

            { "VertexWaterAvailableMaps4", (parser, x) => x.VertexWaterAvailableMaps4 = parser.ParseString() },
            { "VertexWaterHeightClampLow4", (parser, x) => x.VertexWaterHeightClampLow4 = parser.ParseFloat() },
            { "VertexWaterHeightClampHi4", (parser, x) => x.VertexWaterHeightClampHi4 = parser.ParseFloat() },
            { "VertexWaterAngle4", (parser, x) => x.VertexWaterAngle4 = parser.ParseInteger() },
            { "VertexWaterXPosition4", (parser, x) => x.VertexWaterXPosition4 = parser.ParseFloat() },
            { "VertexWaterYPosition4", (parser, x) => x.VertexWaterYPosition4 = parser.ParseFloat() },
            { "VertexWaterZPosition4", (parser, x) => x.VertexWaterZPosition4 = parser.ParseFloat() },
            { "VertexWaterXGridCells4", (parser, x) => x.VertexWaterXGridCells4 = parser.ParseInteger() },
            { "VertexWaterYGridCells4", (parser, x) => x.VertexWaterYGridCells4 = parser.ParseInteger() },
            { "VertexWaterGridSize4", (parser, x) => x.VertexWaterGridSize4 = parser.ParseFloat() },
            { "VertexWaterAttenuationA4", (parser, x) => x.VertexWaterAttenuationA4 = parser.ParseFloat() },
            { "VertexWaterAttenuationB4", (parser, x) => x.VertexWaterAttenuationB4 = parser.ParseFloat() },
            { "VertexWaterAttenuationC4", (parser, x) => x.VertexWaterAttenuationC4 = parser.ParseFloat() },
            { "VertexWaterAttenuationRange4", (parser, x) => x.VertexWaterAttenuationRange4 = parser.ParseFloat() },

            { "DownwindAngle", (parser, x) => x.DownwindAngle = parser.ParseFloat() },
            { "DrawSkyBox", (parser, x) => x.DrawSkyBox = parser.ParseBoolean() },
            { "SkyBoxPositionZ", (parser, x) => x.SkyBoxPositionZ = parser.ParseFloat() },
            { "SkyBoxScale", (parser, x) => x.SkyBoxScale = parser.ParseFloat() },

            { "DefaultCameraMinHeight", (parser, x) => x.DefaultCameraMinHeight = parser.ParseFloat() },
            { "DefaultCameraMaxHeight", (parser, x) => x.DefaultCameraMaxHeight = parser.ParseFloat() },
            { "DefaultCameraPitchAngle", (parser, x) => x.DefaultCameraPitchAngle = parser.ParseFloat() },
            { "DefaultCameraYawAngle", (parser, x) => x.DefaultCameraYawAngle = parser.ParseFloat() },
            { "DefaultCameraScrollSpeedScalar", (parser, x) => x.DefaultCameraScrollSpeedScalar = parser.ParseFloat() },
            { "CameraLockHeightDelta", (parser, x) => x.CameraLockHeightDelta = parser.ParseFloat() },
            { "CameraTerrainSampleRadiusForHeight", (parser, x) => x.CameraTerrainSampleRadiusForHeight = parser.ParseFloat() },

            { "CameraPitch", (parser, x) => x.CameraPitch = parser.ParseFloat() },
            { "CameraYaw", (parser, x) => x.CameraYaw = parser.ParseFloat() },
            { "CameraHeight", (parser, x) => x.CameraHeight = parser.ParseFloat() },
            { "MaxCameraHeight", (parser, x) => x.MaxCameraHeight = parser.ParseFloat() },
            { "MinCameraHeight", (parser, x) => x.MinCameraHeight = parser.ParseFloat() },
            { "UseCameraInReplay", (parser, x) => x.UseCameraInReplay = parser.ParseBoolean() },
            { "CameraAdjustSpeed", (parser, x) => x.CameraAdjustSpeed = parser.ParseFloat() },
            { "ScrollAmountCutoff", (parser, x) => x.ScrollAmountCutoff = parser.ParseFloat() },
            { "EnforceMaxCameraHeight", (parser, x) => x.EnforceMaxCameraHeight = parser.ParseBoolean() },
            { "TerrainHeightAtEdgeOfMap", (parser, x) => x.TerrainHeightAtEdgeOfMap = parser.ParseFloat() },
            { "UnitDamagedThreshold", (parser, x) => x.UnitDamagedThreshold = parser.ParseFloat() },
            { "UnitReallyDamagedThreshold", (parser, x) => x.UnitReallyDamagedThreshold = parser.ParseFloat() },
            { "GroundStiffness", (parser, x) => x.GroundStiffness = parser.ParseFloat() },
            { "StructureStiffness", (parser, x) => x.StructureStiffness = parser.ParseFloat() },
            { "Gravity", (parser, x) => x.Gravity = parser.ParseFloat() },

            { "PartitionCellSize", (parser, x) => x.PartitionCellSize = parser.ParseFloat() },
            { "TerrainResourceCellSize", (parser, x) => x.TerrainResourceCellSize = parser.ParseFloat() },

            { "ParticleScale", (parser, x) => x.ParticleScale = parser.ParseFloat() },

            { "AutoFireParticleSmallPrefix", (parser, x) => x.AutoFireParticleSmallPrefix = parser.ParseString() },
            { "AutoFireParticleSmallSystem", (parser, x) => x.AutoFireParticleSmallSystem = parser.ParseAssetReference() },
            { "AutoFireParticleSmallMax", (parser, x) => x.AutoFireParticleSmallMax = parser.ParseInteger() },

            { "AutoFireParticleMediumPrefix", (parser, x) => x.AutoFireParticleMediumPrefix = parser.ParseString() },
            { "AutoFireParticleMediumSystem", (parser, x) => x.AutoFireParticleMediumSystem = parser.ParseAssetReference() },
            { "AutoFireParticleMediumMax", (parser, x) => x.AutoFireParticleMediumMax = parser.ParseInteger() },

            { "AutoFireParticleLargePrefix", (parser, x) => x.AutoFireParticleLargePrefix = parser.ParseString() },
            { "AutoFireParticleLargeSystem", (parser, x) => x.AutoFireParticleLargeSystem = parser.ParseAssetReference() },
            { "AutoFireParticleLargeMax", (parser, x) => x.AutoFireParticleLargeMax = parser.ParseInteger() },

            { "AutoSmokeParticleSmallPrefix", (parser, x) => x.AutoSmokeParticleSmallPrefix = parser.ParseString() },
            { "AutoSmokeParticleSmallSystem", (parser, x) => x.AutoSmokeParticleSmallSystem = parser.ParseAssetReference() },
            { "AutoSmokeParticleSmallMax", (parser, x) => x.AutoSmokeParticleSmallMax = parser.ParseInteger() },

            { "AutoSmokeParticleMediumPrefix", (parser, x) => x.AutoSmokeParticleMediumPrefix = parser.ParseString() },
            { "AutoSmokeParticleMediumSystem", (parser, x) => x.AutoSmokeParticleMediumSystem = parser.ParseAssetReference() },
            { "AutoSmokeParticleMediumMax", (parser, x) => x.AutoSmokeParticleMediumMax = parser.ParseInteger() },

            { "AutoSmokeParticleLargePrefix", (parser, x) => x.AutoSmokeParticleLargePrefix = parser.ParseString() },
            { "AutoSmokeParticleLargeSystem", (parser, x) => x.AutoSmokeParticleLargeSystem = parser.ParseAssetReference() },
            { "AutoSmokeParticleLargeMax", (parser, x) => x.AutoSmokeParticleLargeMax = parser.ParseInteger() },

            { "AutoAflameParticlePrefix", (parser, x) => x.AutoAflameParticlePrefix = parser.ParseString() },
            { "AutoAflameParticleSystem", (parser, x) => x.AutoAflameParticleSystem = parser.ParseAssetReference() },
            { "AutoAflameParticleMax", (parser, x) => x.AutoAflameParticleMax = parser.ParseInteger() },

            { "HistoricDamageLimit", (parser, x) => x.HistoricDamageLimit = parser.ParseInteger() },

            { "AmmoPipScaleFactor", (parser, x) => x.AmmoPipScaleFactor = parser.ParseFloat() },
            { "ContainerPipScaleFactor", (parser, x) => x.ContainerPipScaleFactor = parser.ParseFloat() },
            { "AmmoPipScreenOffset", (parser, x) => x.AmmoPipScreenOffset = Coord2D.Parse(parser) },
            { "ContainerPipScreenOffset", (parser, x) => x.ContainerPipScreenOffset = Coord2D.Parse(parser) },
            { "AmmoPipWorldOffset", (parser, x) => x.AmmoPipWorldOffset = Coord3D.Parse(parser) },
            { "ContainerPipWorldOffset", (parser, x) => x.ContainerPipWorldOffset = Coord3D.Parse(parser) },

            { "LevelGainAnimationName", (parser, x) => x.LevelGainAnimationName = parser.ParseAssetReference() },
            { "LevelGainAnimationTime", (parser, x) => x.LevelGainAnimationTime = parser.ParseFloat() },
            { "LevelGainAnimationZRise", (parser, x) => x.LevelGainAnimationZRise = parser.ParseFloat() },

            { "GetHealedAnimationName", (parser, x) => x.GetHealedAnimationName = parser.ParseAssetReference() },
            { "GetHealedAnimationTime", (parser, x) => x.GetHealedAnimationTime = parser.ParseFloat() },
            { "GetHealedAnimationZRise", (parser, x) => x.GetHealedAnimationZRise = parser.ParseFloat() },

            { "GenericDamageFieldName", (parser, x) => x.GenericDamageFieldName = parser.ParseAssetReference() },
            { "GenericDamageWarningName", (parser, x) => x.GenericDamageWarningName = parser.ParseAssetReference() },

            { "MaxTerrainTracks", (parser, x) => x.MaxTerrainTracks = parser.ParseInteger() },
            { "TimeOfDay", (parser, x) => x.TimeOfDay = parser.ParseEnum<TimeOfDay>() },
            { "Weather", (parser, x) => x.Weather = parser.ParseEnum<MapWeatherType>() },
            { "MakeTrackMarks", (parser, x) => x.MakeTrackMarks = parser.ParseBoolean() },
            { "ForceModelsToFollowTimeOfDay", (parser, x) => x.ForceModelsToFollowTimeOfDay = parser.ParseBoolean() },
            { "ForceModelsToFollowWeather", (parser, x) => x.ForceModelsToFollowWeather = parser.ParseBoolean() },

            { "InfantryLightMorningScale", (parser, x) => x.InfantryLightMorningScale = parser.ParseFloat() },
            { "InfantryLightAfternoonScale", (parser, x) => x.InfantryLightAfternoonScale = parser.ParseFloat() },
            { "InfantryLightEveningScale", (parser, x) => x.InfantryLightEveningScale = parser.ParseFloat() },
            { "InfantryLightNightScale", (parser, x) => x.InfantryLightNightScale = parser.ParseFloat() },

            { "TerrainLightingMorningAmbient", (parser, x) => x.TerrainLightingMorningAmbient = IniColorRgb.Parse(parser) },
            { "TerrainLightingMorningDiffuse", (parser, x) => x.TerrainLightingMorningDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainLightingMorningLightPos", (parser, x) => x.TerrainLightingMorningLightPos = Coord3D.Parse(parser) },

            { "TerrainLightingEveningAmbient", (parser, x) => x.TerrainLightingEveningAmbient = IniColorRgb.Parse(parser) },
            { "TerrainLightingEveningDiffuse", (parser, x) => x.TerrainLightingEveningDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainLightingEveningLightPos", (parser, x) => x.TerrainLightingEveningLightPos = Coord3D.Parse(parser) },

            { "TerrainLightingNightAmbient", (parser, x) => x.TerrainLightingNightAmbient = IniColorRgb.Parse(parser) },
            { "TerrainLightingNightDiffuse", (parser, x) => x.TerrainLightingNightDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainLightingNightLightPos", (parser, x) => x.TerrainLightingNightLightPos = Coord3D.Parse(parser) },

            { "TerrainObjectsLightingMorningAmbient", (parser, x) => x.TerrainObjectsLightingMorningAmbient = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingMorningDiffuse", (parser, x) => x.TerrainObjectsLightingMorningDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingMorningLightPos", (parser, x) => x.TerrainObjectsLightingMorningLightPos = Coord3D.Parse(parser) },

            { "TerrainObjectsLightingEveningAmbient", (parser, x) => x.TerrainObjectsLightingEveningAmbient = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingEveningDiffuse", (parser, x) => x.TerrainObjectsLightingEveningDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingEveningLightPos", (parser, x) => x.TerrainObjectsLightingEveningLightPos = Coord3D.Parse(parser) },

            { "TerrainObjectsLightingNightAmbient", (parser, x) => x.TerrainObjectsLightingNightAmbient = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingNightDiffuse", (parser, x) => x.TerrainObjectsLightingNightDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingNightLightPos", (parser, x) => x.TerrainObjectsLightingNightLightPos = Coord3D.Parse(parser) },

            { "TerrainLightingAfternoonAmbient", (parser, x) => x.TerrainLightingAfternoonAmbient = IniColorRgb.Parse(parser) },
            { "TerrainLightingAfternoonDiffuse", (parser, x) => x.TerrainLightingAfternoonDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainLightingAfternoonLightPos", (parser, x) => x.TerrainLightingAfternoonLightPos = Coord3D.Parse(parser) },

            { "TerrainObjectsLightingAfternoonAmbient", (parser, x) => x.TerrainObjectsLightingAfternoonAmbient = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingAfternoonDiffuse", (parser, x) => x.TerrainObjectsLightingAfternoonDiffuse = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingAfternoonLightPos", (parser, x) => x.TerrainObjectsLightingAfternoonLightPos = Coord3D.Parse(parser) },

            { "TerrainLightingAfternoonAmbient2", (parser, x) => x.TerrainLightingAfternoonAmbient2 = IniColorRgb.Parse(parser) },
            { "TerrainLightingAfternoonDiffuse2", (parser, x) => x.TerrainLightingAfternoonDiffuse2 = IniColorRgb.Parse(parser) },
            { "TerrainLightingAfternoonLightPos2", (parser, x) => x.TerrainLightingAfternoonLightPos2 = Coord3D.Parse(parser) },

            { "TerrainObjectsLightingAfternoonAmbient2", (parser, x) => x.TerrainObjectsLightingAfternoonAmbient2 = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingAfternoonDiffuse2", (parser, x) => x.TerrainObjectsLightingAfternoonDiffuse2 = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingAfternoonLightPos2", (parser, x) => x.TerrainObjectsLightingAfternoonLightPos2 = Coord3D.Parse(parser) },

            { "TerrainLightingAfternoonAmbient3", (parser, x) => x.TerrainLightingAfternoonAmbient3 = IniColorRgb.Parse(parser) },
            { "TerrainLightingAfternoonDiffuse3", (parser, x) => x.TerrainLightingAfternoonDiffuse3 = IniColorRgb.Parse(parser) },
            { "TerrainLightingAfternoonLightPos3", (parser, x) => x.TerrainLightingAfternoonLightPos3 = Coord3D.Parse(parser) },

            { "TerrainObjectsLightingAfternoonAmbient3", (parser, x) => x.TerrainObjectsLightingAfternoonAmbient3 = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingAfternoonDiffuse3", (parser, x) => x.TerrainObjectsLightingAfternoonDiffuse3 = IniColorRgb.Parse(parser) },
            { "TerrainObjectsLightingAfternoonLightPos3", (parser, x) => x.TerrainObjectsLightingAfternoonLightPos3 = Coord3D.Parse(parser) },

            { "AudioOn", (parser, x) => x.AudioOn = parser.ParseBoolean() },
            { "MusicOn", (parser, x) => x.MusicOn = parser.ParseBoolean() },
            { "SoundsOn", (parser, x) => x.SoundsOn = parser.ParseBoolean() },
            { "SpeechOn", (parser, x) => x.SpeechOn = parser.ParseBoolean() },
            { "VideoOn", (parser, x) => x.VideoOn = parser.ParseBoolean() },

            { "DebugAI", (parser, x) => x.DebugAI = parser.ParseBoolean() },
            { "DebugAIObstacles", (parser, x) => x.DebugAIObstacles = parser.ParseBoolean() },

            { "MaxRoadSegments", (parser, x) => x.MaxRoadSegments = parser.ParseInteger() },
            { "MaxRoadVertex", (parser, x) => x.MaxRoadVertex = parser.ParseInteger() },
            { "MaxRoadIndex", (parser, x) => x.MaxRoadIndex = parser.ParseInteger() },
            { "MaxRoadTypes", (parser, x) => x.MaxRoadTypes = parser.ParseInteger() },

            { "GoodCommandPointLimit", (parser, x) => x.GoodCommandPointLimit = parser.ParseInteger() },
            { "EvilCommandPointLimit", (parser, x) => x.EvilCommandPointLimit = parser.ParseInteger() },
            { "PowerLimit", (parser, x) => x.PowerLimit = parser.ParseInteger() },
            { "ResourceMultiplierLimit", (parser, x) => x.ResourceMultiplierLimit = parser.ParseFloat() },
            { "InitialMaxRingLevel", (parser, x) => x.InitialMaxRingLevel = parser.ParseInteger() },
            { "SkipMapUnroll", (parser, x) => x.SkipMapUnroll = parser.ParseBoolean() },
            { "ResourceBonusMultiplier", (parser, x) => x.ResourceBonusMultiplier = parser.ParseFloat() },
            { "GoodCommandPoints", (parser, x) => x.GoodCommandPoints = CommandPoints.Parse(parser) },
            { "EvilCommandPoints", (parser, x) => x.EvilCommandPoints = CommandPoints.Parse(parser) },
            { "GoodCommandPointsBonus", (parser, x) => x.GoodCommandPointsBonus = parser.ParseInteger() },
            { "EvilCommandPointsBonus", (parser, x) => x.EvilCommandPointsBonus = parser.ParseInteger() },
            { "GoodCommandPointsAI", (parser, x) => x.GoodCommandPointsAI = CommandPoints.Parse(parser) },
            { "EvilCommandPointsAI", (parser, x) => x.EvilCommandPointsAI = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP2", (parser, x) => x.GoodCommandPointsMP2 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP2", (parser, x) => x.EvilCommandPointsMP2 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP3", (parser, x) => x.GoodCommandPointsMP3 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP3", (parser, x) => x.EvilCommandPointsMP3 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP4", (parser, x) => x.GoodCommandPointsMP4 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP4", (parser, x) => x.EvilCommandPointsMP4 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP5", (parser, x) => x.GoodCommandPointsMP5 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP5", (parser, x) => x.EvilCommandPointsMP5 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP6", (parser, x) => x.GoodCommandPointsMP6 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP6", (parser, x) => x.EvilCommandPointsMP6 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP7", (parser, x) => x.GoodCommandPointsMP7 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP7", (parser, x) => x.EvilCommandPointsMP7 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP8", (parser, x) => x.GoodCommandPointsMP8 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP8", (parser, x) => x.EvilCommandPointsMP8 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP56", (parser, x) => x.GoodCommandPointsMP56 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP56", (parser, x) => x.EvilCommandPointsMP56 = CommandPoints.Parse(parser) },
            { "GoodCommandPointsMP78", (parser, x) => x.GoodCommandPointsMP78 = CommandPoints.Parse(parser) },
            { "EvilCommandPointsMP78", (parser, x) => x.EvilCommandPointsMP78 = CommandPoints.Parse(parser) },
            { "MultiPlayMoneyMult", (parser, x) => x.MultiPlayMoneyMult = MultiPlayerTuningFactor.Parse(parser) },
            { "MultiPlayUnitXPMult", (parser, x) => x.MultiPlayUnitXPMult = MultiPlayerTuningFactor.Parse(parser) },
            { "MultiPlayBuildingXPMult", (parser, x) => x.MultiPlayBuildingXPMult = MultiPlayerTuningFactor.Parse(parser) },
            { "MultiPlayUnitSpeedMult", (parser, x) => x.MultiPlayUnitSpeedMult = MultiPlayerTuningFactor.Parse(parser) },
            { "MultiPlayBuildingSpeedMult", (parser, x) => x.MultiPlayBuildingSpeedMult = MultiPlayerTuningFactor.Parse(parser) },

            { "HandicapBuildSpeed5", (parser, x) => x.HandicapBuildSpeed5 = parser.ParsePercentage() },
            { "HandicapBuildSpeed10", (parser, x) => x.HandicapBuildSpeed10 = parser.ParsePercentage() },
            { "HandicapBuildSpeed15", (parser, x) => x.HandicapBuildSpeed15 = parser.ParsePercentage() },
            { "HandicapBuildSpeed20", (parser, x) => x.HandicapBuildSpeed20 = parser.ParsePercentage() },
            { "HandicapBuildSpeed25", (parser, x) => x.HandicapBuildSpeed25 = parser.ParsePercentage() },
            { "HandicapBuildSpeed30", (parser, x) => x.HandicapBuildSpeed30 = parser.ParsePercentage() },
            { "HandicapBuildSpeed35", (parser, x) => x.HandicapBuildSpeed35 = parser.ParsePercentage() },
            { "HandicapBuildSpeed40", (parser, x) => x.HandicapBuildSpeed40 = parser.ParsePercentage() },
            { "HandicapBuildSpeed45", (parser, x) => x.HandicapBuildSpeed45 = parser.ParsePercentage() },
            { "HandicapBuildSpeed50", (parser, x) => x.HandicapBuildSpeed50 = parser.ParsePercentage() },
            { "HandicapBuildSpeed55", (parser, x) => x.HandicapBuildSpeed55 = parser.ParsePercentage() },
            { "HandicapBuildSpeed60", (parser, x) => x.HandicapBuildSpeed60 = parser.ParsePercentage() },
            { "HandicapBuildSpeed65", (parser, x) => x.HandicapBuildSpeed65 = parser.ParsePercentage() },
            { "HandicapBuildSpeed70", (parser, x) => x.HandicapBuildSpeed70 = parser.ParsePercentage() },
            { "HandicapBuildSpeed75", (parser, x) => x.HandicapBuildSpeed75 = parser.ParsePercentage() },
            { "HandicapBuildSpeed80", (parser, x) => x.HandicapBuildSpeed80 = parser.ParsePercentage() },
            { "HandicapBuildSpeed85", (parser, x) => x.HandicapBuildSpeed85 = parser.ParsePercentage() },
            { "HandicapBuildSpeed90", (parser, x) => x.HandicapBuildSpeed90 = parser.ParsePercentage() },
            { "HandicapBuildSpeed95", (parser, x) => x.HandicapBuildSpeed95 = parser.ParsePercentage() },
            { "HandicapBuildSpeed100", (parser, x) => x.HandicapBuildSpeed100 = parser.ParsePercentage() },

            { "ValuePerSupplyBox", (parser, x) => x.ValuePerSupplyBox = parser.ParseInteger() },
            { "SupplyBoxesPerTree", (parser, x) => x.SupplyBoxesPerTree = parser.ParseInteger() },

            { "BuildSpeed", (parser, x) => x.BuildSpeed = parser.ParseFloat() },
            { "MinDistFromEdgeOfMapForBuild", (parser, x) => x.MinDistFromEdgeOfMapForBuild = parser.ParseFloat() },
            { "SupplyBuildBorder", (parser, x) => x.SupplyBuildBorder = parser.ParseFloat() },

            { "AllowedHeightVariationForBuilding", (parser, x) => x.AllowedHeightVariationForBuilding = parser.ParseFloat() },

            { "MinLowEnergyProductionSpeed", (parser, x) => x.MinLowEnergyProductionSpeed = parser.ParseFloat() },
            { "MaxLowEnergyProductionSpeed", (parser, x) => x.MaxLowEnergyProductionSpeed = parser.ParseFloat() },
            { "LowEnergyPenaltyModifier", (parser, x) => x.LowEnergyPenaltyModifier = parser.ParseFloat() },
            { "MultipleFactory", (parser, x) => x.MultipleFactory = parser.ParseFloat() },
            { "RefundPercent", (parser, x) => x.RefundPercent = parser.ParsePercentage() },
            { "StealthFriendlyOpacity", (parser, x) => x.StealthFriendlyOpacity = parser.ParsePercentage() },

            { "CommandCenterHealRange", (parser, x) => x.CommandCenterHealRange = parser.ParseFloat() },
            { "CommandCenterHealAmount", (parser, x) => x.CommandCenterHealAmount = parser.ParseFloat() },
            { "MaxLineBuildObjects", (parser, x) => x.MaxLineBuildObjects = parser.ParseInteger() },
            { "MaxTunnelCapacity", (parser, x) => x.MaxTunnelCapacity = parser.ParseInteger() },

            { "StandardMinefieldDensity", (parser, x) => x.StandardMinefieldDensity = parser.ParseFloat() },
            { "StandardMinefieldDistance", (parser, x) => x.StandardMinefieldDistance = parser.ParseFloat() },

            { "HorizontalScrollSpeedFactor", (parser, x) => x.HorizontalScrollSpeedFactor = parser.ParseFloat() },
            { "VerticalScrollSpeedFactor", (parser, x) => x.VerticalScrollSpeedFactor = parser.ParseFloat() },

            { "ScreenEdgeScrollSpeedFactor", (parser, x) => x.ScreenEdgeScrollSpeedFactor = parser.ParseFloat() },
            { "ScreenEdgeScrollRampTime", (parser, x) => x.ScreenEdgeScrollRampTime = parser.ParseFloat() },

            { "KeyboardScrollSpeedFactor", (parser, x) => x.KeyboardScrollSpeedFactor = parser.ParseFloat() },
            { "MovementPenaltyDamageState", (parser, x) => x.MovementPenaltyDamageState = parser.ParseEnum<BodyDamageType>() },

            { "MaxParticleCount", (parser, x) => x.MaxParticleCount = parser.ParseInteger() },
            { "MaxFieldParticleCount", (parser, x) => x.MaxFieldParticleCount = parser.ParseInteger() },

            { "WeaponBonus", (parser, x) => x.WeaponBonuses.Parse(parser) },

            { "HealthBonus_Veteran", (parser, x) => x.HealthBonusVeteran = parser.ParsePercentage() },
            { "HealthBonus_Elite", (parser, x) => x.HealthBonusElite = parser.ParsePercentage() },
            { "HealthBonus_Heroic", (parser, x) => x.HealthBonusHeroic = parser.ParsePercentage() },

            { "HumanSoloPlayerHealthBonus_Easy", (parser, x) => x.HumanSoloPlayerHealthBonusEasy = parser.ParsePercentage() },
            { "HumanSoloPlayerHealthBonus_Normal", (parser, x) => x.HumanSoloPlayerHealthBonusNormal = parser.ParsePercentage() },
            { "HumanSoloPlayerHealthBonus_Hard", (parser, x) => x.HumanSoloPlayerHealthBonusHard = parser.ParsePercentage() },

            { "AttributeModifierArmorMaxBonus", (parser, x) => x.AttributeModifierArmorMaxBonus = parser.ParsePercentage() },

            { "GroupSelectMinSelectSize", (parser, x) => x.GroupSelectMinSelectSize = parser.ParseInteger() },
            { "GroupSelectVolumeBase", (parser, x) => x.GroupSelectVolumeBase = parser.ParseFloat() },
            { "GroupSelectVolumeIncrement", (parser, x) => x.GroupSelectVolumeIncrement = parser.ParseFloat() },
            { "MaxUnitSelectSounds", (parser, x) => x.MaxUnitSelectSounds = parser.ParseInteger() },

            { "DamageRadiusMinimumForSplash", (parser, x) => x.DamageRadiusMinimumForSplash = parser.ParseFloat() },

            { "SelectionFlashSaturationFactor", (parser, x) => x.SelectionFlashSaturationFactor = parser.ParseFloat() },
            { "SelectionFlashHouseColor", (parser, x) => x.SelectionFlashHouseColor = parser.ParseBoolean() },

            { "CameraAudibleRadius", (parser, x) => x.CameraAudibleRadius = parser.ParseInteger() },
            { "GroupMoveClickToGatherAreaFactor", (parser, x) => x.GroupMoveClickToGatherAreaFactor = parser.ParseFloat() },

            { "ShakeSubtleIntensity", (parser, x) => x.ShakeSubtleIntensity = parser.ParseFloat() },
            { "ShakeNormalIntensity", (parser, x) => x.ShakeNormalIntensity = parser.ParseFloat() },
            { "ShakeStrongIntensity", (parser, x) => x.ShakeStrongIntensity = parser.ParseFloat() },
            { "ShakeSevereIntensity", (parser, x) => x.ShakeSevereIntensity = parser.ParseFloat() },
            { "ShakeCineExtremeIntensity", (parser, x) => x.ShakeCineExtremeIntensity = parser.ParseFloat() },
            { "ShakeCineInsaneIntensity", (parser, x) => x.ShakeCineInsaneIntensity = parser.ParseFloat() },

            { "MaxShakeIntensity", (parser, x) => x.MaxShakeIntensity = parser.ParseFloat() },
            { "MaxShakeRange", (parser, x) => x.MaxShakeRange = parser.ParseFloat() },

            { "SellPercentage", (parser, x) => x.SellPercentage = parser.ParsePercentage() },

            { "BaseRegenHealthPercentPerSecond", (parser, x) => x.BaseRegenHealthPercentPerSecond = parser.ParsePercentage() },
            { "BaseRegenDelay", (parser, x) => x.BaseRegenDelay = parser.ParseInteger() },

            { "SpecialPowerViewObject", (parser, x) => x.SpecialPowerViewObject = parser.ParseAssetReference() },

            { "StandardPublicBone", (parser, x) => x.StandardPublicBones.Add(parser.ParseString()) },

            { "DefaultStartingCash", (parser, x) => x.DefaultStartingCash = parser.ParseInteger() },

            { "UnlookPersistDuration", (parser, x) => x.UnlookPersistDuration = parser.ParseInteger() },

            { "ShroudColor", (parser, x) => x.ShroudColor = IniColorRgb.Parse(parser) },
            { "ClearAlpha", (parser, x) => x.ClearAlpha = parser.ParseByte() },
            { "FogAlpha", (parser, x) => x.FogAlpha = parser.ParseByte() },
            { "ShroudAlpha", (parser, x) => x.ShroudAlpha = parser.ParseByte() },

            { "TaintOn", (parser, x) => x.TaintOn = parser.ParseBoolean() },
            { "TaintColor", (parser, x) => x.TaintColor = IniColorRgb.Parse(parser) },
            { "TaintAlpha", (parser, x) => x.TaintAlpha = parser.ParseByte() },
            { "ElvenWoodColor", (parser, x) => x.ElvenWoodColor = IniColorRgb.Parse(parser) },

            { "NetworkFPSHistoryLength", (parser, x) => x.NetworkFpsHistoryLength = parser.ParseInteger() },
            { "NetworkLatencyHistoryLength", (parser, x) => x.NetworkLatencyHistoryLength = parser.ParseInteger() },
            { "NetworkRunAheadMetricsTime", (parser, x) => x.NetworkRunAheadMetricsTime = parser.ParseInteger() },
            { "NetworkCushionHistoryLength", (parser, x) => x.NetworkCushionHistoryLength = parser.ParseInteger() },
            { "NetworkRunAheadSlack", (parser, x) => x.NetworkRunAheadSlack = parser.ParseInteger() },
            { "NetworkKeepAliveDelay", (parser, x) => x.NetworkKeepAliveDelay = parser.ParseInteger() },
            { "NetworkDisconnectTime", (parser, x) => x.NetworkDisconnectTime = parser.ParseInteger() },
            { "NetworkPlayerTimeoutTime", (parser, x) => x.NetworkPlayerTimeoutTime = parser.ParseInteger() },
            { "NetworkDisconnectScreenNotifyTime", (parser, x) => x.NetworkDisconnectScreenNotifyTime = parser.ParseInteger() },

            { "KeyboardCameraRotateSpeed", (parser, x) => x.KeyboardCameraRotateSpeed = parser.ParseFloat() },

            { "UserDataLeafName", (parser, x) => x.UserDataLeafName = parser.ParseString() },

            { "DefaultVoiceAttackChargeTimeout", (parser, x) => x.DefaultVoiceAttackChargeTimeout = parser.ParseInteger() },
            { "DefaultMaxDistanceForEngaged", (parser, x) => x.DefaultMaxDistanceForEngaged = parser.ParseInteger() },
            { "DefaultEngagedStateTimeout", (parser, x) => x.DefaultEngagedStateTimeout = parser.ParseInteger() },
            { "AnimationSharingCap", (parser, x) => x.AnimationSharingCap = parser.ParseInteger() },
            { "AnimationSharingFrameTolerance", (parser, x) => x.AnimationSharingFrameTolerance = parser.ParseInteger() },
            { "AnimationSharingSpeedTolerance", (parser, x) => x.AnimationSharingSpeedTolerance = parser.ParseFloat() },
            { "AnimationSharingWorryThreshold", (parser, x) => x.AnimationSharingWorryThreshold = parser.ParseFloat() },
            { "AnimationSharingDrasticThreshold", (parser, x) => x.AnimationSharingDrasticThreshold = parser.ParseFloat() },

            { "ParticleCursorAnim2DTemplateName", (parser, x) => x.ParticleCursorAnim2DTemplateName = parser.ParseAssetReference() },
            { "ParticleCursorBurstCount", (parser, x) => x.ParticleCursorBurstCount = parser.ParseInteger() },
            { "ParticleCursorBurstFactor", (parser, x) => x.ParticleCursorBurstFactor = RandomVariable.Parse(parser) },
            { "ParticleCursorStopBurstFactor", (parser, x) => x.ParticleCursorStopBurstFactor = parser.ParseFloat() },
            { "ParticleCursorBurstFrequency", (parser, x) => x.ParticleCursorBurstFrequency = parser.ParseInteger() },
            { "ParticleCursorParticleLife", (parser, x) => x.ParticleCursorParticleLife = RandomVariable.Parse(parser) },
            { "ParticleCursorSystemLife", (parser, x) => x.ParticleCursorSystemLife = RandomVariable.Parse(parser) },
            { "ParticleCursorDriftVelX", (parser, x) => x.ParticleCursorDriftVelX = RandomVariable.Parse(parser) },
            { "ParticleCursorDriftVelY", (parser, x) => x.ParticleCursorDriftVelY = RandomVariable.Parse(parser) },
            { "ParticleCursorVelocityDrag", (parser, x) => x.ParticleCursorVelocityDrag = RandomVariable.Parse(parser) },
            { "ParticleCursorParticleSize", (parser, x) => x.ParticleCursorParticleSize = RandomVariable.Parse(parser) },
            { "ParticleCursorPerFrameSize", (parser, x) => x.ParticleCursorPerFrameSize = parser.ParseBoolean() },
            { "ParticleCursorAlpha", (parser, x) => x.ParticleCursorAlpha = parser.ParseByte() },
            { "ParticleCursorOffset", (parser, x) => x.ParticleCursorOffset = Coord2D.Parse(parser) },

            { "ProgressMovieOffset", (parser, x) => x.ProgressMovieOffset = Coord2D.Parse(parser) },
            { "ProgressMovieSize", (parser, x) => x.ProgressMovieSize = Coord2D.Parse(parser) },

            { "UseHelpTextSystem", (parser, x) => x.UseHelpTextSystem = parser.ParseBoolean() },
            { "EnableHouseColor", (parser, x) => x.EnableHouseColor = parser.ParseBoolean() },

            { "TreeFadeObjectFilter", (parser, x) => x.TreeFadeObjectFilter = ObjectFilter.Parse(parser) },
            { "CamouflageDetectorObjectFilter", (parser, x) => x.CamouflageDetectorObjectFilter = ObjectFilter.Parse(parser) },
            { "VeterancyPipDrawObjectFilter", (parser, x) => x.VeterancyPipDrawObjectFilter = ObjectFilter.Parse(parser) },

            { "ReinvisibityDelay", (parser, x) => x.ReinvisibilityDelay = parser.ParseInteger() },
            { "InvisibilityOpacityMin", (parser, x) => x.InvisibilityOpacityMin = parser.ParseFloat() },
            { "InvisibilityOpacityMax", (parser, x) => x.InvisibilityOpacityMax = parser.ParseFloat() },
            { "InvisibilityOpacityCycleFrames", (parser, x) => x.InvisibilityOpacityCycleFrames = parser.ParseInteger() },

            { "BuilderFadeOutTime", (parser, x) => x.BuilderFadeOutTime = parser.ParseInteger() },
            { "BuilderFadeInTime", (parser, x) => x.BuilderFadeInTime = parser.ParseInteger() },
            { "BuilderMoveFromNewStructureDistance", (parser, x) => x.BuilderMoveFromNewStructureDistance = parser.ParseInteger() },
            { "MaxCastleRadius", (parser, x) => x.MaxCastleRadius = parser.ParseInteger() },

            { "VictoryConditionStructureObjectFilter", (parser, x) => x.VictoryConditionStructureObjectFilter = ObjectFilter.Parse(parser) },
            { "VictoryConditionUnitObjectFilter", (parser, x) => x.VictoryConditionUnitObjectFilter = ObjectFilter.Parse(parser) },

            { "TutorialMap", (parser, x) => x.TutorialMap = parser.ParseFileName() },
            { "TutorialLoadMovie", (parser, x) => x.TutorialLoadMovie = parser.ParseAssetReference() },
            { "TutorialObjective", (parser, x) => x.TutorialObjective = parser.ParseLocalizedStringKey() },

            { "BasicTutorialMap", (parser, x) => x.BasicTutorialMap = parser.ParseFileName() },
            { "BasicTutorialLoadScreenStillImage", (parser, x) => x.BasicTutorialLoadScreenStillImage = parser.ParseAssetReference() },
            { "BasicTutorialLoadScreenMusicTrack", (parser, x) => x.BasicTutorialLoadScreenMusicTrack = parser.ParseAssetReference() },
            { "BasicTutorialObjective", (parser, x) => x.BasicTutorialObjective = parser.ParseLocalizedStringKey() },
            { "BasicTutorialMillisecondsAfterStartToStartFadeUp", (parser, x) => x.BasicTutorialMillisecondsAfterStartToStartFadeUp = parser.ParseInteger() },

            { "AdvancedTutorialMap", (parser, x) => x.AdvancedTutorialMap = parser.ParseFileName() },
            { "AdvancedTutorialLoadScreenStillImage", (parser, x) => x.AdvancedTutorialLoadScreenStillImage = parser.ParseAssetReference() },
            { "AdvancedTutorialLoadScreenMusicTrack", (parser, x) => x.AdvancedTutorialLoadScreenMusicTrack = parser.ParseAssetReference() },
            { "AdvancedTutorialObjective", (parser, x) => x.AdvancedTutorialObjective = parser.ParseLocalizedStringKey() },
            { "AdvancedTutorialMillisecondsAfterStartToStartFadeUp", (parser, x) => x.AdvancedTutorialMillisecondsAfterStartToStartFadeUp = parser.ParseInteger() },

            { "ObjectsThatScore", (parser, x) => x.ObjectsThatScore = ObjectFilter.Parse(parser) },
            { "ScoreKeeper_UnitsBuiltMultiplier", (parser, x) => x.ScoreKeeper_UnitsBuiltMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_UnitsDestroyedMultiplier", (parser, x) => x.ScoreKeeper_UnitsDestroyedMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_StructuresBuiltMultiplier", (parser, x) => x.ScoreKeeper_StructuresBuiltMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_StructuresDestroyedMultiplier", (parser, x) => x.ScoreKeeper_StructuresDestroyedMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_HeroesVettedMultiplier", (parser, x) => x.ScoreKeeper_HeroesVettedMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_UnitsVettedMultiplier", (parser, x) => x.ScoreKeeper_UnitsVettedMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_ObjectivesCompletedMultiplier", (parser, x) => x.ScoreKeeper_ObjectivesCompletedMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_SuppliesCollectedMultiplier", (parser, x) => x.ScoreKeeper_SuppliesCollectedMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_SkillPointsMultiplier", (parser, x) => x.ScoreKeeper_SkillPointsMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_PowerPointsMultiplier", (parser, x) => x.ScoreKeeper_PowerPointsMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_RegionCommandPointsMultiplier", (parser, x) => x.ScoreKeeper_RegionCommandPointsMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_RegionResourcesMultiplier", (parser, x) => x.ScoreKeeper_RegionResourcesMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_RegionPowerPointsMultiplier", (parser, x) => x.ScoreKeeper_RegionPowerPointsMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_TimeTakenMultiplier", (parser, x) => x.ScoreKeeper_TimeTakenMultiplier = parser.ParseInteger() },
            { "ScoreKeeper_TimeTakenMaximumScore", (parser, x) => x.ScoreKeeper_TimeTakenMaximumScore = parser.ParseInteger() },
            { "ScoreKeeper_TimeTakenMinimumScore", (parser, x) => x.ScoreKeeper_TimeTakenMinimumScore = parser.ParseInteger() },
            { "ScoreKeeper_TotalVictoryRequiredScore", (parser, x) => x.ScoreKeeper_TotalVictoryRequiredScore = parser.ParseInteger() },
            { "ScoreKeeper_NormalVictoryRequiredScore", (parser, x) => x.ScoreKeeper_NormalVictoryRequiredScore = parser.ParseInteger() },
            { "ScoreKeeper_NormalVictoryRequiredObjectivesPercentage", (parser, x) => x.ScoreKeeper_NormalVictoryRequiredObjectivesPercentage = parser.ParseInteger() },
            { "ScoreKeeper_PlayerEliminatedMultiplier", (parser, x) => x.ScoreKeeper_PlayerEliminatedMultiplier = parser.ParseFloat() },

            { "TintUnitIfPathingForMoreThan", (parser, x) => x.TintUnitIfPathingForMoreThan = parser.ParseInteger() },

            { "GarrisonedRangeMultiplier", (parser, x) => x.GarrisonedRangeMultiplier = parser.ParseFloat() },

            { "MaxPathfindCellsPerFrame", (parser, x) => x.MaxPathfindCellsPerFrame = parser.ParseInteger() },
            { "MaxCellsFindMeleeEngagementLocation", (parser, x) => x.MaxCellsFindMeleeEngagementLocation = parser.ParseInteger() },
            { "MaxCellsAdjustDestination", (parser, x) => x.MaxCellsAdjustDestination = parser.ParseInteger() },
            { "MaxCellsAdjustHordeMeleeDestination", (parser, x) => x.MaxCellsAdjustHordeMeleeDestination = parser.ParseInteger() },
            { "MaxCellsAdjustTargetDestination", (parser, x) => x.MaxCellsAdjustTargetDestination = parser.ParseInteger() },
            { "MaxCellsAdjustToPossibleDestination", (parser, x) => x.MaxCellsAdjustToPossibleDestination = parser.ParseInteger() },
            { "MaxCellsAdjustToMeleeDestination", (parser, x) => x.MaxCellsAdjustToMeleeDestination = parser.ParseInteger() },
            { "MaxCellsAdjustToNearestGroundCell", (parser, x) => x.MaxCellsAdjustToNearestGroundCell = parser.ParseInteger() },
            { "MaxCellsAdjustToNearestValidCell", (parser, x) => x.MaxCellsAdjustToNearestValidCell = parser.ParseInteger() },
            { "MaxCellsPatchPath", (parser, x) => x.MaxCellsPatchPath = parser.ParseInteger() },
            { "MaxCellsFindPathLimit", (parser, x) => x.MaxCellsFindPathLimit = parser.ParseInteger() },
            { "MaxCellsFindAttackPath", (parser, x) => x.MaxCellsFindAttackPath = parser.ParseInteger() },
            { "MaxCellsFindAttackPathSideways", (parser, x) => x.MaxCellsFindAttackPathSideways = parser.ParseInteger() },
            { "MaxCellsToExamineTowardsGoal", (parser, x) => x.MaxCellsToExamineTowardsGoal = parser.ParseInteger() },

            { "NumMinutesBeforePlayersCanTransferMoney", (parser, x) => x.NumMinutesBeforePlayersCanTransferMoney = parser.ParseInteger() },

            { "StateMachineDebug", (parser, x) => x.StateMachineDebug = parser.ParseBoolean() },
            { "UseCameraConstraints", (parser, x) => x.UseCameraConstraints = parser.ParseBoolean() },
            { "ShroudOn", (parser, x) => x.ShroudOn = parser.ParseBoolean() },
            { "FogOfWarOn", (parser, x) => x.FogOfWarOn = parser.ParseBoolean() },
            { "ShowCollisionExtents", (parser, x) => x.ShowCollisionExtents = parser.ParseBoolean() },

            { "DebugProjectileTileWidth", (parser, x) => x.DebugProjectileTileWidth = parser.ParseInteger() },
            { "DebugProjectileTileDuration", (parser, x) => x.DebugProjectileTileDuration = parser.ParseInteger() },
            { "DebugProjectileTileColor", (parser, x) => x.DebugProjectileTileColor = IniColorRgb.Parse(parser) },

            { "DebugVisibilityTileCount", (parser, x) => x.DebugVisibilityTileCount = parser.ParseInteger() },
            { "DebugVisibilityTileWidth", (parser, x) => x.DebugVisibilityTileWidth = parser.ParseFloat() },
            { "DebugVisibilityTileDuration", (parser, x) => x.DebugVisibilityTileDuration = parser.ParseInteger() },

            { "DebugVisibilityTileTargettableColor", (parser, x) => x.DebugVisibilityTileTargettableColor = IniColorRgb.Parse(parser) },
            { "DebugVisibilityTileDeshroudColor", (parser, x) => x.DebugVisibilityTileDeshroudColor = IniColorRgb.Parse(parser) },
            { "DebugVisibilityTileGapColor", (parser, x) => x.DebugVisibilityTileGapColor = IniColorRgb.Parse(parser) },

            { "DebugThreatMapTileDuration", (parser, x) => x.DebugThreatMapTileDuration = parser.ParseInteger() },
            { "MaxDebugThreatMapValue", (parser, x) => x.MaxDebugThreatMapValue = parser.ParseInteger() },

            { "DebugCashValueMapTileDuration", (parser, x) => x.DebugCashValueMapTileDuration = parser.ParseInteger() },
            { "MaxDebugCashValueMapValue", (parser, x) => x.MaxDebugCashValueMapValue = parser.ParseInteger() },

            { "VTune", (parser, x) => x.VTune = parser.ParseBoolean() }
        };

        public string ShellMapName { get; private set; }
        public string MapName { get; private set; }
        public string MoveHintName { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool ShowProps { get; private set; }

        public bool UseTrees { get; private set; }
        public bool UseFpsLimit { get; private set; }
        public int FramesPerSecondLimit { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool UseHighQualityVideo { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool DisablePixelShader { get; private set; }

        public int ChipsetType { get; private set; }
        public int MaxShellScreens { get; private set; }
        public bool Wireframe { get; private set; }
        public bool UseCloudMap { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public bool AllowTreeFading { get; private set; }

        public bool UseLightMap { get; private set; }
        public bool BilinearTerrainTex { get; private set; }
        public bool TrilinearTerrainTex { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool AnisotropicTerrainTex { get; private set; }

        public bool MultiPassTerrain { get; private set; }
        public bool AdjustCliffTextures { get; private set; }
        public bool StretchTerrain { get; private set; }
        public bool UseHalfHeightMap { get; private set; }
        public bool ShowObjectHealth { get; private set; }
        public bool HideGarrisonFlags { get; private set; }
        public bool Use3WayTerrainBlends { get; private set; }
        public bool DrawEntireTerrain { get; private set; }
        public TerrainLod TerrainLod { get; private set; }
        public int TerrainLodTargetTimeMS { get; private set; }
        public int TextureReductionFactor { get; private set; }
        public bool RightMouseAlwaysScrolls { get; private set; }
        public bool UseWaterPlane { get; private set; }
        public bool UseCloudPlane { get; private set; }
        public bool UseShadowVolumes { get; private set; }
        public bool UseShadowDecals { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public bool UseShadowMapping { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool ShowSelectedUnitMarker { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool UseSimpleHordeDecals { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool UseSimpleMergeDecals { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float OpacityOfSimpleMergeDecals { get; private set; }

        public bool UseBehindBuildingMarker { get; private set; }
        public int DefaultOcclusionDelay { get; private set; }
        public float OccludedColorLuminanceScale { get; private set; }
        public float WaterPositionX { get; private set; }
        public float WaterPositionY { get; private set; }
        public float WaterPositionZ { get; private set; }
        public float WaterExtentX { get; private set; }
        public float WaterExtentY { get; private set; }
        public int WaterType { get; private set; }

        public string DefaultUnitHealingBuffFxList { get; private set; }
        public string DefaultStructureRepairBuffFxList { get; private set; }

        public float DefaultStructureRubbleHeight { get; private set; }

        public string VertexWaterAvailableMaps1 { get; private set; }
        public float VertexWaterHeightClampLow1 { get; private set; }
        public float VertexWaterHeightClampHi1 { get; private set; }
        public int VertexWaterAngle1 { get; private set; }
        public float VertexWaterXPosition1 { get; private set; }
        public float VertexWaterYPosition1 { get; private set; }
        public float VertexWaterZPosition1 { get; private set; }
        public int VertexWaterXGridCells1 { get; private set; }
        public int VertexWaterYGridCells1 { get; private set; }
        public float VertexWaterGridSize1 { get; private set; }
        public float VertexWaterAttenuationA1 { get; private set; }
        public float VertexWaterAttenuationB1 { get; private set; }
        public float VertexWaterAttenuationC1 { get; private set; }
        public float VertexWaterAttenuationRange1 { get; private set; }

        public string VertexWaterAvailableMaps2 { get; private set; }
        public float VertexWaterHeightClampLow2 { get; private set; }
        public float VertexWaterHeightClampHi2 { get; private set; }
        public int VertexWaterAngle2 { get; private set; }
        public float VertexWaterXPosition2 { get; private set; }
        public float VertexWaterYPosition2 { get; private set; }
        public float VertexWaterZPosition2 { get; private set; }
        public int VertexWaterXGridCells2 { get; private set; }
        public int VertexWaterYGridCells2 { get; private set; }
        public float VertexWaterGridSize2 { get; private set; }
        public float VertexWaterAttenuationA2 { get; private set; }
        public float VertexWaterAttenuationB2 { get; private set; }
        public float VertexWaterAttenuationC2 { get; private set; }
        public float VertexWaterAttenuationRange2 { get; private set; }

        public string VertexWaterAvailableMaps3 { get; private set; }
        public float VertexWaterHeightClampLow3 { get; private set; }
        public float VertexWaterHeightClampHi3 { get; private set; }
        public int VertexWaterAngle3 { get; private set; }
        public float VertexWaterXPosition3 { get; private set; }
        public float VertexWaterYPosition3 { get; private set; }
        public float VertexWaterZPosition3 { get; private set; }
        public int VertexWaterXGridCells3 { get; private set; }
        public int VertexWaterYGridCells3 { get; private set; }
        public float VertexWaterGridSize3 { get; private set; }
        public float VertexWaterAttenuationA3 { get; private set; }
        public float VertexWaterAttenuationB3 { get; private set; }
        public float VertexWaterAttenuationC3 { get; private set; }
        public float VertexWaterAttenuationRange3 { get; private set; }

        public string VertexWaterAvailableMaps4 { get; private set; }
        public float VertexWaterHeightClampLow4 { get; private set; }
        public float VertexWaterHeightClampHi4 { get; private set; }
        public int VertexWaterAngle4 { get; private set; }
        public float VertexWaterXPosition4 { get; private set; }
        public float VertexWaterYPosition4 { get; private set; }
        public float VertexWaterZPosition4 { get; private set; }
        public int VertexWaterXGridCells4 { get; private set; }
        public int VertexWaterYGridCells4 { get; private set; }
        public float VertexWaterGridSize4 { get; private set; }
        public float VertexWaterAttenuationA4 { get; private set; }
        public float VertexWaterAttenuationB4 { get; private set; }
        public float VertexWaterAttenuationC4 { get; private set; }
        public float VertexWaterAttenuationRange4 { get; private set; }

        public float DownwindAngle { get; private set; }
        public bool DrawSkyBox { get; private set; }
        public float SkyBoxPositionZ { get; private set; }
        public float SkyBoxScale { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float DefaultCameraMinHeight { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float DefaultCameraMaxHeight { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float DefaultCameraPitchAngle { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float DefaultCameraYawAngle { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float DefaultCameraScrollSpeedScalar { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float CameraLockHeightDelta { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float CameraTerrainSampleRadiusForHeight { get; private set; }

        public float CameraPitch { get; private set; }
        public float CameraYaw { get; private set; }
        public float CameraHeight { get; private set; }
        public float MaxCameraHeight { get; private set; }
        public float MinCameraHeight { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool UseCameraInReplay { get; private set; }

        public float CameraAdjustSpeed { get; private set; }
        public float ScrollAmountCutoff { get; private set; }
        public bool EnforceMaxCameraHeight { get; private set; }
        public float TerrainHeightAtEdgeOfMap { get; private set; }
        public float UnitDamagedThreshold { get; private set; }
        public float UnitReallyDamagedThreshold { get; private set; }
        public float GroundStiffness { get; private set; }
        public float StructureStiffness { get; private set; }
        public float Gravity { get; private set; }

        public float PartitionCellSize { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float TerrainResourceCellSize { get; private set; }

        public float ParticleScale { get; private set; }

        public string AutoFireParticleSmallPrefix { get; private set; }
        public string AutoFireParticleSmallSystem { get; private set; }
        public int AutoFireParticleSmallMax { get; private set; }

        public string AutoFireParticleMediumPrefix { get; private set; }
        public string AutoFireParticleMediumSystem { get; private set; }
        public int AutoFireParticleMediumMax { get; private set; }

        public string AutoFireParticleLargePrefix { get; private set; }
        public string AutoFireParticleLargeSystem { get; private set; }
        public int AutoFireParticleLargeMax { get; private set; }

        public string AutoSmokeParticleSmallPrefix { get; private set; }
        public string AutoSmokeParticleSmallSystem { get; private set; }
        public int AutoSmokeParticleSmallMax { get; private set; }

        public string AutoSmokeParticleMediumPrefix { get; private set; }
        public string AutoSmokeParticleMediumSystem { get; private set; }
        public int AutoSmokeParticleMediumMax { get; private set; }

        public string AutoSmokeParticleLargePrefix { get; private set; }
        public string AutoSmokeParticleLargeSystem { get; private set; }
        public int AutoSmokeParticleLargeMax { get; private set; }

        public string AutoAflameParticlePrefix { get; private set; }
        public string AutoAflameParticleSystem { get; private set; }
        public int AutoAflameParticleMax { get; private set; }

        public int HistoricDamageLimit { get; private set; }

        public float AmmoPipScaleFactor { get; private set; }
        public float ContainerPipScaleFactor { get; private set; }
        public Coord2D AmmoPipScreenOffset { get; private set; }
        public Coord2D ContainerPipScreenOffset { get; private set; }
        public Coord3D AmmoPipWorldOffset { get; private set; }
        public Coord3D ContainerPipWorldOffset { get; private set; }

        public string LevelGainAnimationName { get; private set; }
        public float LevelGainAnimationTime { get; private set; }
        public float LevelGainAnimationZRise { get; private set; }

        public string GetHealedAnimationName { get; private set; }
        public float GetHealedAnimationTime { get; private set; }
        public float GetHealedAnimationZRise { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string GenericDamageFieldName { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string GenericDamageWarningName { get; private set; }

        public int MaxTerrainTracks { get; private set; }
        public TimeOfDay TimeOfDay { get; private set; }
        public MapWeatherType Weather { get; private set; }
        public bool MakeTrackMarks { get; private set; }
        public bool ForceModelsToFollowTimeOfDay { get; private set; }
        public bool ForceModelsToFollowWeather { get; private set; }

        public float InfantryLightMorningScale { get; private set; }
        public float InfantryLightAfternoonScale { get; private set; }
        public float InfantryLightEveningScale { get; private set; }
        public float InfantryLightNightScale { get; private set; }

        public IniColorRgb TerrainLightingMorningAmbient { get; private set; }
        public IniColorRgb TerrainLightingMorningDiffuse { get; private set; }
        public Coord3D TerrainLightingMorningLightPos { get; private set; }

        public IniColorRgb TerrainLightingEveningAmbient { get; private set; }
        public IniColorRgb TerrainLightingEveningDiffuse { get; private set; }
        public Coord3D TerrainLightingEveningLightPos { get; private set; }

        public IniColorRgb TerrainLightingNightAmbient { get; private set; }
        public IniColorRgb TerrainLightingNightDiffuse { get; private set; }
        public Coord3D TerrainLightingNightLightPos { get; private set; }

        public IniColorRgb TerrainObjectsLightingMorningAmbient { get; private set; }
        public IniColorRgb TerrainObjectsLightingMorningDiffuse { get; private set; }
        public Coord3D TerrainObjectsLightingMorningLightPos { get; private set; }

        public IniColorRgb TerrainObjectsLightingEveningAmbient { get; private set; }
        public IniColorRgb TerrainObjectsLightingEveningDiffuse { get; private set; }
        public Coord3D TerrainObjectsLightingEveningLightPos { get; private set; }

        public IniColorRgb TerrainObjectsLightingNightAmbient { get; private set; }
        public IniColorRgb TerrainObjectsLightingNightDiffuse { get; private set; }
        public Coord3D TerrainObjectsLightingNightLightPos { get; private set; }

        public IniColorRgb TerrainLightingAfternoonAmbient { get; private set; }
        public IniColorRgb TerrainLightingAfternoonDiffuse { get; private set; }
        public Coord3D TerrainLightingAfternoonLightPos { get; private set; }

        public IniColorRgb TerrainObjectsLightingAfternoonAmbient { get; private set; }
        public IniColorRgb TerrainObjectsLightingAfternoonDiffuse { get; private set; }
        public Coord3D TerrainObjectsLightingAfternoonLightPos { get; private set; }

        public IniColorRgb TerrainLightingAfternoonAmbient2 { get; private set; }
        public IniColorRgb TerrainLightingAfternoonDiffuse2 { get; private set; }
        public Coord3D TerrainLightingAfternoonLightPos2 { get; private set; }

        public IniColorRgb TerrainObjectsLightingAfternoonAmbient2 { get; private set; }
        public IniColorRgb TerrainObjectsLightingAfternoonDiffuse2 { get; private set; }
        public Coord3D TerrainObjectsLightingAfternoonLightPos2 { get; private set; }

        public IniColorRgb TerrainLightingAfternoonAmbient3 { get; private set; }
        public IniColorRgb TerrainLightingAfternoonDiffuse3 { get; private set; }
        public Coord3D TerrainLightingAfternoonLightPos3 { get; private set; }

        public IniColorRgb TerrainObjectsLightingAfternoonAmbient3 { get; private set; }
        public IniColorRgb TerrainObjectsLightingAfternoonDiffuse3 { get; private set; }
        public Coord3D TerrainObjectsLightingAfternoonLightPos3 { get; private set; }

        public bool AudioOn { get; private set; }
        public bool MusicOn { get; private set; }
        public bool SoundsOn { get; private set; }
        public bool SpeechOn { get; private set; }
        public bool VideoOn { get; private set; }

        public bool DebugAI { get; private set; }
        public bool DebugAIObstacles { get; private set; }

        public int MaxRoadSegments { get; private set; }
        public int MaxRoadVertex { get; private set; }
        public int MaxRoadIndex { get; private set; }
        public int MaxRoadTypes { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int GoodCommandPointLimit { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int EvilCommandPointLimit { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int PowerLimit { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float ResourceMultiplierLimit { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int InitialMaxRingLevel { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public bool SkipMapUnroll { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float ResourceBonusMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints GoodCommandPoints { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints EvilCommandPoints { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int GoodCommandPointsBonus { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int EvilCommandPointsBonus { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints GoodCommandPointsAI { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints EvilCommandPointsAI { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints GoodCommandPointsMP2 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints EvilCommandPointsMP2 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints GoodCommandPointsMP3 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints EvilCommandPointsMP3 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints GoodCommandPointsMP4 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints EvilCommandPointsMP4 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints GoodCommandPointsMP5 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints EvilCommandPointsMP5 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints GoodCommandPointsMP6 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints EvilCommandPointsMP6 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints GoodCommandPointsMP7 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints EvilCommandPointsMP7 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints GoodCommandPointsMP8 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public CommandPoints EvilCommandPointsMP8 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints GoodCommandPointsMP56 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints EvilCommandPointsMP56 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints GoodCommandPointsMP78 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public CommandPoints EvilCommandPointsMP78 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public MultiPlayerTuningFactor MultiPlayMoneyMult { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public MultiPlayerTuningFactor MultiPlayUnitXPMult { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public MultiPlayerTuningFactor MultiPlayBuildingXPMult { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public MultiPlayerTuningFactor MultiPlayUnitSpeedMult { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public MultiPlayerTuningFactor MultiPlayBuildingSpeedMult { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed5 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed10 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed15 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed20 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed25 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed30 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed35 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed40 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed45 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed50 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed55 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed60 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed65 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed70 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed75 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed80 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed85 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed90 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed95 { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float HandicapBuildSpeed100 { get; private set; }

        public int ValuePerSupplyBox { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int SupplyBoxesPerTree { get; private set; }

        public float BuildSpeed { get; private set; }
        public float MinDistFromEdgeOfMapForBuild { get; private set; }
        public float SupplyBuildBorder { get; private set; }

        public float AllowedHeightVariationForBuilding { get; private set; }

        public float MinLowEnergyProductionSpeed { get; private set; }
        public float MaxLowEnergyProductionSpeed { get; private set; }
        public float LowEnergyPenaltyModifier { get; private set; }
        public float MultipleFactory { get; private set; }
        public float RefundPercent { get; private set; }
        public float StealthFriendlyOpacity { get; private set; }

        public float CommandCenterHealRange { get; private set; }
        public float CommandCenterHealAmount { get; private set; }
        public int MaxLineBuildObjects { get; private set; }
        public int MaxTunnelCapacity { get; private set; }

        public float StandardMinefieldDensity { get; private set; }
        public float StandardMinefieldDistance { get; private set; }

        public float HorizontalScrollSpeedFactor { get; private set; }
        public float VerticalScrollSpeedFactor { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float ScreenEdgeScrollSpeedFactor { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float ScreenEdgeScrollRampTime { get; private set; }

        public float KeyboardScrollSpeedFactor { get; private set; }
        public BodyDamageType MovementPenaltyDamageState { get; private set; }

        public int MaxParticleCount { get; private set; }
        public int MaxFieldParticleCount { get; private set; }

        public WeaponBonusSet WeaponBonuses { get; } = new WeaponBonusSet();

        public float HealthBonusVeteran { get; private set; }
        public float HealthBonusElite { get; private set; }
        public float HealthBonusHeroic { get; private set; }

        public float HumanSoloPlayerHealthBonusEasy { get; private set; }
        public float HumanSoloPlayerHealthBonusNormal { get; private set; }
        public float HumanSoloPlayerHealthBonusHard { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float AttributeModifierArmorMaxBonus { get; private set; }

        public int GroupSelectMinSelectSize { get; private set; }
        public float GroupSelectVolumeBase { get; private set; }
        public float GroupSelectVolumeIncrement { get; private set; }
        public int MaxUnitSelectSounds { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float DamageRadiusMinimumForSplash { get; private set; }

        public float SelectionFlashSaturationFactor { get; private set; }
        public bool SelectionFlashHouseColor { get; private set; }

        public int CameraAudibleRadius { get; private set; }
        public float GroupMoveClickToGatherAreaFactor { get; private set; }

        public float ShakeSubtleIntensity { get; private set; }
        public float ShakeNormalIntensity { get; private set; }
        public float ShakeStrongIntensity { get; private set; }
        public float ShakeSevereIntensity { get; private set; }
        public float ShakeCineExtremeIntensity { get; private set; }
        public float ShakeCineInsaneIntensity { get; private set; }

        public float MaxShakeIntensity { get; private set; }
        public float MaxShakeRange { get; private set; }

        public float SellPercentage { get; private set; }

        public float BaseRegenHealthPercentPerSecond { get; private set; }
        public int BaseRegenDelay { get; private set; }

        public string SpecialPowerViewObject { get; private set; }

        public List<string> StandardPublicBones { get; } = new List<string>();

        public int DefaultStartingCash { get; private set; }

        public int UnlookPersistDuration { get; private set; }

        public IniColorRgb ShroudColor { get; private set; }
        public byte ClearAlpha { get; private set; }
        public byte FogAlpha { get; private set; }
        public byte ShroudAlpha { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool TaintOn { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public IniColorRgb TaintColor { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public byte TaintAlpha { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public IniColorRgb ElvenWoodColor { get; private set; }

        public int NetworkFpsHistoryLength { get; private set; }
        public int NetworkLatencyHistoryLength { get; private set; }
        public int NetworkRunAheadMetricsTime { get; private set; }
        public int NetworkCushionHistoryLength { get; private set; }
        public int NetworkRunAheadSlack { get; private set; }
        public int NetworkKeepAliveDelay { get; private set; }
        public int NetworkDisconnectTime { get; private set; }
        public int NetworkPlayerTimeoutTime { get; private set; }
        public int NetworkDisconnectScreenNotifyTime { get; private set; }

        public float KeyboardCameraRotateSpeed { get; private set; }

        public string UserDataLeafName { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int DefaultVoiceAttackChargeTimeout { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int DefaultMaxDistanceForEngaged { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int DefaultEngagedStateTimeout { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int AnimationSharingCap { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int AnimationSharingFrameTolerance { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float AnimationSharingSpeedTolerance { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float AnimationSharingWorryThreshold { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float AnimationSharingDrasticThreshold { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string ParticleCursorAnim2DTemplateName { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ParticleCursorBurstCount { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public RandomVariable ParticleCursorBurstFactor { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public float ParticleCursorStopBurstFactor { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ParticleCursorBurstFrequency { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public RandomVariable ParticleCursorParticleLife { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public RandomVariable ParticleCursorSystemLife { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public RandomVariable ParticleCursorDriftVelX { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public RandomVariable ParticleCursorDriftVelY { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public RandomVariable ParticleCursorVelocityDrag { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public RandomVariable ParticleCursorParticleSize { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool ParticleCursorPerFrameSize { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public byte ParticleCursorAlpha { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public Coord2D ParticleCursorOffset { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public Coord2D ProgressMovieOffset { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public Coord2D ProgressMovieSize { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool UseHelpTextSystem { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public bool EnableHouseColor { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public ObjectFilter TreeFadeObjectFilter { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public ObjectFilter CamouflageDetectorObjectFilter { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public ObjectFilter VeterancyPipDrawObjectFilter { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int ReinvisibilityDelay { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float InvisibilityOpacityMin { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float InvisibilityOpacityMax { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int InvisibilityOpacityCycleFrames { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int BuilderFadeOutTime { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int BuilderFadeInTime { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int BuilderMoveFromNewStructureDistance { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCastleRadius { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public ObjectFilter VictoryConditionStructureObjectFilter { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public ObjectFilter VictoryConditionUnitObjectFilter { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string TutorialMap { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string TutorialLoadMovie { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string TutorialObjective { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string BasicTutorialMap { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string BasicTutorialLoadScreenStillImage { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string BasicTutorialLoadScreenMusicTrack { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string BasicTutorialObjective { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int BasicTutorialMillisecondsAfterStartToStartFadeUp { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string AdvancedTutorialMap { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string AdvancedTutorialLoadScreenStillImage { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string AdvancedTutorialLoadScreenMusicTrack { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public string AdvancedTutorialObjective { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int AdvancedTutorialMillisecondsAfterStartToStartFadeUp { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public ObjectFilter ObjectsThatScore { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_UnitsBuiltMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_UnitsDestroyedMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_StructuresBuiltMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_StructuresDestroyedMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_HeroesVettedMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_UnitsVettedMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_ObjectivesCompletedMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_SuppliesCollectedMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int ScoreKeeper_SkillPointsMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_PowerPointsMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_RegionCommandPointsMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_RegionResourcesMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_RegionPowerPointsMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_TimeTakenMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_TimeTakenMaximumScore { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_TimeTakenMinimumScore { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_TotalVictoryRequiredScore { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_NormalVictoryRequiredScore { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int ScoreKeeper_NormalVictoryRequiredObjectivesPercentage { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float ScoreKeeper_PlayerEliminatedMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public int TintUnitIfPathingForMoreThan { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public float GarrisonedRangeMultiplier { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxPathfindCellsPerFrame { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsFindMeleeEngagementLocation { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsAdjustDestination { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsAdjustHordeMeleeDestination { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsAdjustTargetDestination { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsAdjustToPossibleDestination { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsAdjustToMeleeDestination { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsAdjustToNearestGroundCell { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsAdjustToNearestValidCell { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsPatchPath { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsFindPathLimit { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsFindAttackPath { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsFindAttackPathSideways { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int MaxCellsToExamineTowardsGoal { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarthII)]
        public int NumMinutesBeforePlayersCanTransferMoney { get; private set; }

        public bool StateMachineDebug { get; private set; }
        public bool UseCameraConstraints { get; private set; }
        public bool ShroudOn { get; private set; }
        public bool FogOfWarOn { get; private set; }
        public bool ShowCollisionExtents { get; private set; }

        public int DebugProjectileTileWidth { get; private set; }
        public int DebugProjectileTileDuration { get; private set; }
        public IniColorRgb DebugProjectileTileColor { get; private set; }

        public int DebugVisibilityTileCount { get; private set; }
        public float DebugVisibilityTileWidth { get; private set; }
        public int DebugVisibilityTileDuration { get; private set; }

        public IniColorRgb DebugVisibilityTileTargettableColor { get; private set; }
        public IniColorRgb DebugVisibilityTileDeshroudColor { get; private set; }
        public IniColorRgb DebugVisibilityTileGapColor { get; private set; }

        public int DebugThreatMapTileDuration { get; private set; }
        public int MaxDebugThreatMapValue { get; private set; }

        public int DebugCashValueMapTileDuration { get; private set; }
        public int MaxDebugCashValueMapValue { get; private set; }

        public bool VTune { get; private set; }
    }

    public enum BodyDamageType
    {
        [IniEnum("PRISTINE")]
        Pristine,

        [IniEnum("DAMAGED")]
        Damaged,

        [IniEnum("REALLYDAMAGED")]
        ReallyDamaged,

        [IniEnum("RUBBLE")]
        Rubble
    }

    public enum TerrainLod
    {
        [IniEnum("DISABLE")]
        Disable
    }

    public struct Coord2D
    {
        internal static Coord2D Parse(IniParser parser)
        {
            return new Coord2D
            {
                X = parser.ParseAttributeFloat("X"),
                Y = parser.ParseAttributeFloat("Y")
            };
        }

        public float X;
        public float Y;
    }

    public sealed class WeaponBonusSet : Dictionary<WeaponBonusType, WeaponBonus>
    {
        internal void Parse(IniParser parser)
        {
            var bonusType = parser.ParseEnum<WeaponBonusType>();

            if (!this.TryGetValue(bonusType, out var bonus))
            {
                Add(bonusType, bonus = new WeaponBonus());
            }

            var attribute = parser.ParseEnum<WeaponBonusAttributeType>();
            var value = parser.ParsePercentage();

            bonus[attribute] = value;
        }
    }

    public sealed class WeaponBonus : Dictionary<WeaponBonusAttributeType, float>
    {
        
    }

    public enum WeaponBonusAttributeType
    {
        [IniEnum("RATE_OF_FIRE")]
        RateOfFire,

        [IniEnum("RANGE")]
        Range,

        [IniEnum("DAMAGE")]
        Damage
    }

    public enum WeaponBonusType
    {
        [IniEnum("GARRISONED")]
        Garrisoned,

        [IniEnum("HORDE")]
        Horde,

        [IniEnum("CONTINUOUS_FIRE_MEAN")]
        ContinuousFireMean,

        [IniEnum("CONTINUOUS_FIRE_FAST")]
        ContinuousFireFast,

        [IniEnum("NATIONALISM")]
        Nationalism,

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade,

        [IniEnum("DRONE_SPOTTING")]
        DroneSpotting,

        [IniEnum("ENTHUSIASTIC")]
        Enthusiastic,

        [IniEnum("VETERAN")]
        Veteran,

        [IniEnum("ELITE")]
        Elite,

        [IniEnum("HERO")]
        Hero,

        [IniEnum("BATTLEPLAN_BOMBARDMENT")]
        BattleplanBombardment,

        [IniEnum("BATTLEPLAN_HOLDTHELINE")]
        BattleplanHoldTheLine,

        [IniEnum("BATTLEPLAN_SEARCHANDDESTROY")]
        BattleplanSearchAndDestroy,

        [IniEnum("SUBLIMINAL")]
        Subliminal,

        [IniEnum("SOLO_HUMAN_EASY")]
        SoloHumanEasy,

        [IniEnum("SOLO_HUMAN_NORMAL")]
        SoloHumanNormal,

        [IniEnum("SOLO_HUMAN_HARD")]
        SoloHumanHard,

        [IniEnum("SOLO_AI_EASY")]
        SoloAIEasy,

        [IniEnum("SOLO_AI_NORMAL")]
        SoloAINormal,

        [IniEnum("SOLO_AI_HARD")]
        SoloAIHard,

        [IniEnum("FANATICISM"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Fanaticism,

        [IniEnum("TARGET_FAERIE_FIRE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        TargetFaerieFire,

        [IniEnum("FRENZY_ONE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FrenzyOne,

        [IniEnum("FRENZY_TWO"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FrenzyTwo,

        [IniEnum("FRENZY_THREE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FrenzyThree,
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class MultiPlayerTuningFactor
    {
        internal static MultiPlayerTuningFactor Parse(IniParser parser)
        {
            return new MultiPlayerTuningFactor
            {
                MP1 = parser.ParseAttributeFloat("MP1"),
                MP2 = parser.ParseAttributeFloat("MP2"),
                MP3 = parser.ParseAttributeFloat("MP3"),
                MP4 = parser.ParseAttributeFloat("MP4"),
                MP5 = parser.ParseAttributeFloat("MP5"),
                MP6 = parser.ParseAttributeFloat("MP6"),
                MP7 = parser.ParseAttributeFloat("MP7"),
                MP8 = parser.ParseAttributeFloat("MP8"),
            };
        }

        public float MP1 { get; private set; }
        public float MP2 { get; private set; }
        public float MP3 { get; private set; }
        public float MP4 { get; private set; }
        public float MP5 { get; private set; }
        public float MP6 { get; private set; }
        public float MP7 { get; private set; }
        public float MP8 { get; private set; }
    }

    [AddedIn(SageGame.BattleForMiddleEarthII)]
    public struct CommandPoints
    {
        internal static CommandPoints Parse(IniParser parser)
        {
            var result = new CommandPoints
            {
                StartingValue = parser.ParseInteger()
            };

            var maxValueToken = parser.GetNextTokenOptional();
            if (maxValueToken != null)
            {
                result.MaximumValue = parser.ScanInteger(maxValueToken.Value);
            }

            return result;
        }

        public int StartingValue;
        public int MaximumValue;
    }
}
