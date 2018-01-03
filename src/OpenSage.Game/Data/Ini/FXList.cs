using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class FXList
    {
        internal static FXList Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<FXList> FieldParseTable = new IniParseTable<FXList>
        {
            { "BuffNugget", (parser, x) => x.Items.Add(BuffNugget.Parse(parser)) },
            { "CameraShakerVolume", (parser, x) => x.Items.Add(CameraShakerVolumeFXNugget.Parse(parser)) },
            { "DynamicDecal", (parser, x) => x.Items.Add(DynamicDecalFXNugget.Parse(parser)) },
            { "EvaEvent", (parser, x) => x.Items.Add(EvaEventFXNugget.Parse(parser)) },
            { "FXListAtBonePos", (parser, x) => x.Items.Add(FXListAtBonePosFXListItem.Parse(parser)) },
            { "LightPulse", (parser, x) => x.Items.Add(LightPulseFXListItem.Parse(parser)) },
            { "ParticleSystem", (parser, x) => x.Items.Add(ParticleSystemFXListItem.Parse(parser)) },
            { "Sound", (parser, x) => x.Items.Add(SoundFXListItem.Parse(parser)) },
            { "TerrainScorch", (parser, x) => x.Items.Add(TerrainScorchFXListItem.Parse(parser)) },
            { "TintDrawable", (parser, x) => x.Items.Add(TintDrawableFXNugget.Parse(parser)) },
            { "Tracer", (parser, x) => x.Items.Add(TracerFXListItem.Parse(parser)) },
            { "ViewShake", (parser, x) => x.Items.Add(ViewShakeFXListItem.Parse(parser)) },

            { "PlayEvenIfShrouded", (parser, x) => x.PlayEvenIfShrouded = parser.ParseBoolean() },

            {
                "CullingInfo",
                (parser, x) =>
                {
                    x.CullTracking = parser.ParseAttributeFloat("TrackingSeconds");
                    x.CullTrackingMin = parser.ParseAttributeInteger("StartCullingAbove");
                    x.CullTrackingMax = parser.ParseAttributeInteger("CullAllAbove");
                }
            }
        };

        public string Name { get; private set; }

        public List<FXListItem> Items { get; } = new List<FXListItem>();

        public bool PlayEvenIfShrouded { get; private set; }

        public float CullTracking { get; private set; }
        public int CullTrackingMin { get; private set; }
        public int CullTrackingMax { get; private set; }
    }

    public abstract class FXListItem
    {
        
    }

    public sealed class ParticleSystemFXListItem : FXListItem
    {
        internal static ParticleSystemFXListItem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParticleSystemFXListItem> FieldParseTable = new IniParseTable<ParticleSystemFXListItem>
        {
            { "AttachToObject", (parser, x) => x.AttachToObject = parser.ParseBoolean() },
            { "AttachToBone", (parser, x) => x.AttachToBone = parser.ParseBoneName() },
            { "Count", (parser, x) => x.Count = parser.ParseInteger() },
            { "CreateAtGroundHeight", (parser, x) => x.CreateAtGroundHeight = parser.ParseBoolean() },
            { "Height", (parser, x) => x.Height = RandomVariable.Parse(parser) },
            { "InitialDelay", (parser, x) => x.InitialDelay = RandomVariable.Parse(parser) },
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() },
            { "Offset", (parser, x) => x.Offset = Coord3D.Parse(parser) },
            { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() },
            { "Radius", (parser, x) => x.Radius = RandomVariable.Parse(parser) },
            { "Ricochet", (parser, x) => x.Ricochet = parser.ParseBoolean() },
            { "RotateY", (parser, x) => x.RotateY = parser.ParseInteger() },
            { "UseCallersRadius", (parser, x) => x.UseCallersRadius = parser.ParseBoolean() },
        };

        public bool AttachToObject { get; private set; }

        [AddedIn(SageGame.BattleForMiddleEarth)]
        public string AttachToBone { get; private set; }

        public int Count { get; private set; } = 1;
        public bool CreateAtGroundHeight { get; private set; }
        public RandomVariable Height { get; private set; }
        public RandomVariable InitialDelay { get; private set; }
        public string Name { get; private set; }
        public Coord3D Offset { get; private set; }
        public bool OrientToObject { get; private set; }
        public RandomVariable Radius { get; private set; }
        public bool Ricochet { get; private set; }
        public int RotateY { get; private set; }
        public bool UseCallersRadius { get; private set; }
    }

    public sealed class SoundFXListItem : FXListItem
    {
        internal static SoundFXListItem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SoundFXListItem> FieldParseTable = new IniParseTable<SoundFXListItem>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAssetReference() },
        };

        public string Name { get; private set; }
    }

    public sealed class LightPulseFXListItem : FXListItem
    {
        internal static LightPulseFXListItem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<LightPulseFXListItem> FieldParseTable = new IniParseTable<LightPulseFXListItem>
        {
            { "Color", (parser, x) => x.Color = IniColorRgb.Parse(parser) },
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "RadiusAsPercentOfObjectSize", (parser, x) => x.RadiusAsPercentOfObjectSize = parser.ParsePercentage() },
            { "IncreaseTime", (parser, x) => x.IncreaseTime = parser.ParseInteger() },
            { "DecreaseTime", (parser, x) => x.DecreaseTime = parser.ParseInteger() }
        };

        public IniColorRgb Color { get; private set; }
        public int Radius { get; private set; }
        public float RadiusAsPercentOfObjectSize { get; private set; }
        public int IncreaseTime { get; private set; }
        public int DecreaseTime { get; private set; }
    }

    public sealed class ViewShakeFXListItem : FXListItem
    {
        internal static ViewShakeFXListItem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ViewShakeFXListItem> FieldParseTable = new IniParseTable<ViewShakeFXListItem>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<ViewShakeType>() }
        };

        public ViewShakeType Type { get; private set; }
    }

    public sealed class FXListAtBonePosFXListItem : FXListItem
    {
        internal static FXListAtBonePosFXListItem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FXListAtBonePosFXListItem> FieldParseTable = new IniParseTable<FXListAtBonePosFXListItem>
        {
            { "FX", (parser, x) => x.FX = parser.ParseAssetReference() },
            { "BoneName", (parser, x) => x.BoneName = parser.ParseAssetReference() },
            { "OrientToBone", (parser, x) => x.OrientToBone = parser.ParseBoolean() }
        };

        public string FX { get; private set; }
        public string BoneName { get; private set; }
        public bool OrientToBone { get; private set; }
    }

    public sealed class TerrainScorchFXListItem : FXListItem
    {
        internal static TerrainScorchFXListItem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TerrainScorchFXListItem> FieldParseTable = new IniParseTable<TerrainScorchFXListItem>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<TerrainScorchType>() },
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() }
        };

        public TerrainScorchType Type { get; private set; }
        public int Radius { get; private set; }
    }

    public sealed class TracerFXListItem : FXListItem
    {
        internal static TracerFXListItem Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TracerFXListItem> FieldParseTable = new IniParseTable<TracerFXListItem>
        {
            { "Color", (parser, x) => x.Color = IniColorRgb.Parse(parser) },
            { "DecayAt", (parser, x) => x.DecayAt = parser.ParseFloat() },
            { "Length", (parser, x) => x.Length = parser.ParseFloat() },
            { "Probability", (parser, x) => x.Probability = parser.ParseFloat() },
            { "Speed", (parser, x) => x.Speed = parser.ParseInteger() },
            { "Width", (parser, x) => x.Width = parser.ParseFloat() },
        };

        public IniColorRgb Color { get; private set; }
        public float DecayAt { get; private set; }
        public float Length { get; private set; }
        public float Probability { get; private set; }
        public int Speed { get; private set; }
        public float Width { get; private set; }
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class BuffNugget : FXListItem
    {
        internal static BuffNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BuffNugget> FieldParseTable = new IniParseTable<BuffNugget>
        {
            { "BuffType", (parser, x) => x.BuffType = parser.ParseIdentifier() },
            { "BuffThingTemplate", (parser, x) => x.BuffThingTemplate = parser.ParseAssetReference() },
            { "BuffInfantryTemplate", (parser, x) => x.BuffInfantryTemplate = parser.ParseAssetReference() },
            { "BuffCavalryTemplate", (parser, x) => x.BuffCavalryTemplate = parser.ParseAssetReference() },
            { "BuffTrollTemplate", (parser, x) => x.BuffTrollTemplate = parser.ParseAssetReference() },
            { "BuffOrcTemplate", (parser, x) => x.BuffOrcTemplate = parser.ParseAssetReference() },
            { "IsComplexBuff", (parser, x) => x.IsComplexBuff = parser.ParseBoolean() },
            { "BuffLifeTime", (parser, x) => x.BuffLifeTime = parser.ParseUnsignedInteger() },
            { "Extrusion", (parser, x) => x.Extrusion = parser.ParseFloat() },
            { "Color", (parser, x) => x.Color = IniColorRgb.Parse(parser) },
        };

        public string BuffType { get; private set; }
        public string BuffThingTemplate { get; private set; }
        public string BuffInfantryTemplate { get; private set; }
        public string BuffCavalryTemplate { get; private set; }
        public string BuffTrollTemplate { get; private set; }
        public string BuffOrcTemplate { get; private set; }
        public bool IsComplexBuff { get; private set; }
        public uint BuffLifeTime { get; private set; }
        public float Extrusion { get; private set; }
        public IniColorRgb Color { get; private set; }
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class CameraShakerVolumeFXNugget : FXListItem
    {
        internal static CameraShakerVolumeFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CameraShakerVolumeFXNugget> FieldParseTable = new IniParseTable<CameraShakerVolumeFXNugget>
        {
            { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
            { "Duration_Seconds", (parser, x) => x.Duration = parser.ParseFloat() },
            { "Amplitude_Degrees", (parser, x) => x.Amplitude = parser.ParseFloat() }
        };

        public int Radius { get; private set; }
        public float Duration { get; private set; }
        public float Amplitude { get; private set; }
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class DynamicDecalFXNugget : FXListItem
    {
        internal static DynamicDecalFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DynamicDecalFXNugget> FieldParseTable = new IniParseTable<DynamicDecalFXNugget>
        {
            { "DecalName", (parser, x) => x.DecalName = parser.ParseAssetReference() },
            { "Size", (parser, x) => x.Size = parser.ParseInteger() },
            { "Color", (parser, x) => x.Color = IniColorRgb.Parse(parser) },

            { "OpacityStart", (parser, x) => x.OpacityStart = parser.ParseInteger() },
            { "OpacityFadeTimeOne", (parser, x) => x.OpacityFadeTimeOne = parser.ParseInteger() },
            { "OpacityPeak", (parser, x) => x.OpacityPeak = parser.ParseInteger() },
            { "OpacityPeakTime", (parser, x) => x.OpacityPeakTime = parser.ParseInteger() },
            { "OpacityFadeTimeTwo", (parser, x) => x.OpacityFadeTimeTwo = parser.ParseInteger() },
            { "OpacityEnd", (parser, x) => x.OpacityEnd = parser.ParseInteger() },

            { "StartingDelay", (parser, x) => x.StartingDelay = parser.ParseInteger() },
            { "Lifetime", (parser, x) => x.Lifetime = parser.ParseInteger() },
        };

        public string DecalName { get; private set; }
        public int Size { get; private set; }
        public IniColorRgb Color { get; private set; }

        public int OpacityStart { get; private set; }
        public int OpacityFadeTimeOne { get; private set; }
        public int OpacityPeak { get; private set; }
        public int OpacityPeakTime { get; private set; }
        public int OpacityFadeTimeTwo { get; private set; }
        public int OpacityEnd { get; private set; }

        public int StartingDelay { get; private set; }
        public int Lifetime { get; private set; }
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class EvaEventFXNugget : FXListItem
    {
        internal static EvaEventFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<EvaEventFXNugget> FieldParseTable = new IniParseTable<EvaEventFXNugget>
        {
            { "EvaEventOwner", (parser, x) => x.EvaEventOwner = parser.ParseAssetReference() },
            { "EvaEventAlly", (parser, x) => x.EvaEventAlly = parser.ParseAssetReference() }
        };

        public string EvaEventOwner { get; private set; }
        public string EvaEventAlly { get; private set; }
    }

    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class TintDrawableFXNugget : FXListItem
    {
        internal static TintDrawableFXNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<TintDrawableFXNugget> FieldParseTable = new IniParseTable<TintDrawableFXNugget>
        {
            { "Color", (parser, x) => x.Color = IniColorRgb.Parse(parser) },
            { "PreColorTime", (parser, x) => x.PreColorTime = parser.ParseInteger() },
            { "PostColorTime", (parser, x) => x.PostColorTime = parser.ParseInteger() },
            { "SustainedColorTime", (parser, x) => x.SustainedColorTime = parser.ParseInteger() },
            { "Frequency", (parser, x) => x.Frequency = parser.ParseInteger() },
            { "Amplitude", (parser, x) => x.Amplitude = parser.ParseInteger() },
        };

        public IniColorRgb Color { get; private set; }
        public int PreColorTime { get; private set; }
        public int PostColorTime { get; private set; }
        public int SustainedColorTime { get; private set; }
        public int Frequency { get; private set; }
        public int Amplitude { get; private set; }
    }

    public enum ViewShakeType
    {
        [IniEnum("SUBTLE")]
        Subtle,

        [IniEnum("NORMAL")]
        Normal,

        [IniEnum("STRONG")]
        Strong,

        [IniEnum("SEVERE")]
        Severe,

        [IniEnum("CINE_EXTREME"), AddedIn(SageGame.BattleForMiddleEarth)]
        CineExtreme
    }

    public enum TerrainScorchType
    {
        [IniEnum("RANDOM")]
        Random,

        [IniEnum("SCORCH_4"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Scorch4,
    }

    public struct IniColorRgb
    {
        internal static IniColorRgb Parse(IniParser parser)
        {
            return new IniColorRgb
            {
                R = parser.ParseAttributeByte("R"),
                G = parser.ParseAttributeByte("G"),
                B = parser.ParseAttributeByte("B")
            };
        }

        public byte R;
        public byte G;
        public byte B;
    }

    public struct Coord3D
    {
        internal static Coord3D Parse(IniParser parser)
        {
            return new Coord3D
            {
                X = parser.ParseAttributeFloat("X"),
                Y = parser.ParseAttributeFloat("Y"),
                Z = parser.ParseAttributeFloat("Z")
            };
        }

        public float X;
        public float Y;
        public float Z;
    }

    public struct RandomVariable
    {
        internal static RandomVariable Parse(IniParser parser)
        {
            var result = new RandomVariable
            {
                Low = parser.ParseFloat(),
                High = parser.ParseFloat()
            };

            var distributionType = parser.GetNextTokenOptional();
            result.DistributionType = (distributionType != null)
                ? IniParser.ParseEnum<DistributionType>(distributionType.Value)
                : DistributionType.Uniform;

            return result;
        }

        public float Low;
        public float High;
        public DistributionType DistributionType;
    }

    public enum DistributionType
    {
        [IniEnum("CONSTANT")]
        Constant,

        [IniEnum("UNIFORM")]
        Uniform
    }
}
