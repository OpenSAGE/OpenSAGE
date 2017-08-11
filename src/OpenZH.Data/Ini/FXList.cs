using System.Collections.Generic;
using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
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
            { "FXListAtBonePos", (parser, x) => x.Items.Add(FXListAtBonePosFXListItem.Parse(parser)) },
            { "LightPulse", (parser, x) => x.Items.Add(LightPulseFXListItem.Parse(parser)) },
            { "ParticleSystem", (parser, x) => x.Items.Add(ParticleSystemFXListItem.Parse(parser)) },
            { "Sound", (parser, x) => x.Items.Add(SoundFXListItem.Parse(parser)) },
            { "TerrainScorch", (parser, x) => x.Items.Add(TerrainScorchFXListItem.Parse(parser)) },
            { "Tracer", (parser, x) => x.Items.Add(TracerFXListItem.Parse(parser)) },
            { "ViewShake", (parser, x) => x.Items.Add(ViewShakeFXListItem.Parse(parser)) }
        };

        public string Name { get; private set; }

        public List<FXListItem> Items { get; } = new List<FXListItem>();
    }

    public abstract class FXListItem
    {
        
    }

    public sealed class ParticleSystemFXListItem : FXListItem
    {
        internal static ParticleSystemFXListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ParticleSystemFXListItem> FieldParseTable = new IniParseTable<ParticleSystemFXListItem>
        {
            { "AttachToObject", (parser, x) => x.AttachToObject = parser.ParseBoolean() },
            { "Count", (parser, x) => x.Count = parser.ParseInteger() },
            { "CreateAtGroundHeight", (parser, x) => x.CreateAtGroundHeight = parser.ParseBoolean() },
            { "Height", (parser, x) => x.Height = RandomVariable.Parse(parser) },
            { "InitialDelay", (parser, x) => x.InitialDelay = RandomVariable.Parse(parser) },
            { "Name", (parser, x) => x.Name = parser.ParseAsciiString() },
            { "Offset", (parser, x) => x.Offset = Coord3D.Parse(parser) },
            { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() },
            { "Radius", (parser, x) => x.Radius = RandomVariable.Parse(parser) },
            { "Ricochet", (parser, x) => x.Ricochet = parser.ParseBoolean() },
            { "RotateY", (parser, x) => x.RotateY = parser.ParseInteger() },
            { "UseCallersRadius", (parser, x) => x.UseCallersRadius = parser.ParseBoolean() },
        };

        public bool AttachToObject { get; private set; }
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
        internal static SoundFXListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SoundFXListItem> FieldParseTable = new IniParseTable<SoundFXListItem>
        {
            { "Name", (parser, x) => x.Name = parser.ParseAsciiString() },
        };

        public string Name { get; private set; }
    }

    public sealed class LightPulseFXListItem : FXListItem
    {
        internal static LightPulseFXListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

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
        internal static ViewShakeFXListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ViewShakeFXListItem> FieldParseTable = new IniParseTable<ViewShakeFXListItem>
        {
            { "Type", (parser, x) => x.Type = parser.ParseEnum<ViewShakeType>() }
        };

        public ViewShakeType Type { get; private set; }
    }

    public sealed class FXListAtBonePosFXListItem : FXListItem
    {
        internal static FXListAtBonePosFXListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<FXListAtBonePosFXListItem> FieldParseTable = new IniParseTable<FXListAtBonePosFXListItem>
        {
            { "FX", (parser, x) => x.FX = parser.ParseAsciiString() },
            { "BoneName", (parser, x) => x.BoneName = parser.ParseAsciiString() },
            { "OrientToBone", (parser, x) => x.OrientToBone = parser.ParseBoolean() }
        };

        public string FX { get; private set; }
        public string BoneName { get; private set; }
        public bool OrientToBone { get; private set; }
    }

    public sealed class TerrainScorchFXListItem : FXListItem
    {
        internal static TerrainScorchFXListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

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
        internal static TracerFXListItem Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

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

    public enum ViewShakeType
    {
        [IniEnum("SUBTLE")]
        Subtle,

        [IniEnum("NORMAL")]
        Normal,

        [IniEnum("STRONG")]
        Strong,

        [IniEnum("SEVERE")]
        Severe
    }

    public enum TerrainScorchType
    {
        [IniEnum("RANDOM")]
        Random
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
            return new RandomVariable
            {
                Low = parser.ParseInteger(),
                High = parser.ParseInteger(),
                DistributionType = parser.ParseEnum<DistributionType>()
            };
        }

        public int Low;
        public int High;
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
