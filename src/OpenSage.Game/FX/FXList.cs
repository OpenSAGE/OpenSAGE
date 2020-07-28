using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

namespace OpenSage.FX
{
    public sealed class FXList : BaseAsset
    {
        internal static FXList Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("FXList", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<FXList> FieldParseTable = new IniParseTable<FXList>
        {
            { "AttachedModel", (parser, x) => x.Nuggets.Add(AttachedModelFXNugget.Parse(parser)) },
            { "BuffNugget", (parser, x) => x.Nuggets.Add(BuffNugget.Parse(parser)) },
            { "CameraShakerVolume", (parser, x) => x.Nuggets.Add(CameraShakerVolumeFXNugget.Parse(parser)) },
            { "CursorParticleSystem", (parser, x) => x.Nuggets.Add(CursorParticleSystemFXNugget.Parse(parser)) },
            { "DynamicDecal", (parser, x) => x.Nuggets.Add(DynamicDecalFXNugget.Parse(parser)) },
            { "EvaEvent", (parser, x) => x.Nuggets.Add(EvaEventFXNugget.Parse(parser)) },
            { "FXListAtBonePos", (parser, x) => x.Nuggets.Add(FXListAtBonePosFXNugget.Parse(parser)) },
            { "LightPulse", (parser, x) => x.Nuggets.Add(LightPulseFXNugget.Parse(parser)) },
            { "ParticleSysBone", (parser, x) => x.Nuggets.Add(FXParticleSysBoneNugget.Parse(parser)) },
            { "ParticleSystem", (parser, x) => x.Nuggets.Add(ParticleSystemFXNugget.Parse(parser)) },
            { "Sound", (parser, x) => x.Nuggets.Add(SoundFXNugget.Parse(parser)) },
            { "TerrainScorch", (parser, x) => x.Nuggets.Add(TerrainScorchFXNugget.Parse(parser)) },
            { "TintDrawable", (parser, x) => x.Nuggets.Add(TintDrawableFXNugget.Parse(parser)) },
            { "Tracer", (parser, x) => x.Nuggets.Add(TracerFXNugget.Parse(parser)) },
            { "ViewShake", (parser, x) => x.Nuggets.Add(ViewShakeFXNugget.Parse(parser)) },

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

        public List<FXNugget> Nuggets { get; } = new List<FXNugget>();

        public bool PlayEvenIfShrouded { get; private set; }

        public float CullTracking { get; private set; }
        public int CullTrackingMin { get; private set; }
        public int CullTrackingMax { get; private set; }

        internal void Execute(FXListExecutionContext context)
        {
            foreach (var nugget in Nuggets)
            {
                nugget.Execute(context);
            }
        }

        internal void Execute(BehaviorUpdateContext context)
        {
            Execute(
                new FXListExecutionContext(
                    context.GameObject.Transform.Rotation,
                    context.GameObject.Transform.Translation,
                    context.GameContext));
        }
    }
}
