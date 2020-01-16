using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

namespace OpenSage.FX
{
    public sealed class FXList : DisposableBase
    {
        private readonly FXListData _data;
        private readonly List<FXNugget> _nuggets;

        internal FXList(FXListData data)
        {
            _data = data;

            _nuggets = new List<FXNugget>();
            foreach (var nuggetData in data.Nuggets)
            {
                var nugget = nuggetData.CreateNugget();
                if (nugget != null) // TODO: This should never be null.
                {
                    _nuggets.Add(AddDisposable(nugget));
                }
            }
        }

        internal void Execute(FXListContext context)
        {
            foreach (var nugget in _nuggets)
            {
                nugget.Execute(context);
            }
        }
    }

    internal sealed class FXListContext
    {
        public readonly GameObject GameObject;
        public readonly Matrix4x4 WorldMatrix;
        public readonly AssetLoadContext AssetLoadContext;

        public FXListContext(
            GameObject gameObject,
            in Matrix4x4 worldMatrix,
            AssetLoadContext assetLoadContext)
        {
            GameObject = gameObject;
            WorldMatrix = worldMatrix;
            AssetLoadContext = assetLoadContext;
        }
    }

    public sealed class FXListData : BaseAsset
    {
        internal static FXListData Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("FXList", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<FXListData> FieldParseTable = new IniParseTable<FXListData>
        {
            { "AttachedModel", (parser, x) => x.Nuggets.Add(AttachedModelFXNuggetData.Parse(parser)) },
            { "BuffNugget", (parser, x) => x.Nuggets.Add(BuffNuggetData.Parse(parser)) },
            { "CameraShakerVolume", (parser, x) => x.Nuggets.Add(CameraShakerVolumeFXNuggetData.Parse(parser)) },
            { "CursorParticleSystem", (parser, x) => x.Nuggets.Add(CursorParticleSystemFXNuggetData.Parse(parser)) },
            { "DynamicDecal", (parser, x) => x.Nuggets.Add(DynamicDecalFXNuggetData.Parse(parser)) },
            { "EvaEvent", (parser, x) => x.Nuggets.Add(EvaEventFXNuggetData.Parse(parser)) },
            { "FXListAtBonePos", (parser, x) => x.Nuggets.Add(FXListAtBonePosFXNuggetData.Parse(parser)) },
            { "LightPulse", (parser, x) => x.Nuggets.Add(LightPulseFXNuggetData.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.Nuggets.Add(FXParticleSysBoneNuggetData.Parse(parser)) },
            { "ParticleSystem", (parser, x) => x.Nuggets.Add(ParticleSystemFXNuggetData.Parse(parser)) },
            { "Sound", (parser, x) => x.Nuggets.Add(SoundFXNuggetData.Parse(parser)) },
            { "TerrainScorch", (parser, x) => x.Nuggets.Add(TerrainScorchFXNuggetData.Parse(parser)) },
            { "TintDrawable", (parser, x) => x.Nuggets.Add(TintDrawableFXNuggetData.Parse(parser)) },
            { "Tracer", (parser, x) => x.Nuggets.Add(TracerFXNuggetData.Parse(parser)) },
            { "ViewShake", (parser, x) => x.Nuggets.Add(ViewShakeFXNuggetData.Parse(parser)) },

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

        public List<FXNuggetData> Nuggets { get; } = new List<FXNuggetData>();

        public bool PlayEvenIfShrouded { get; private set; }

        public float CullTracking { get; private set; }
        public int CullTrackingMin { get; private set; }
        public int CullTrackingMax { get; private set; }
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

        [IniEnum("CINE_EXTREME"), AddedIn(SageGame.Bfme)]
        CineExtreme
    }

    public enum TerrainScorchType
    {
        [IniEnum("RANDOM")]
        Random,

        [IniEnum("SCORCH_4"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Scorch4,
    }

    [AddedIn(SageGame.Bfme2)]
    public enum DynamicDecalShaderType
    {
        [IniEnum("ADDITIVE")]
        Additive
    }
}
