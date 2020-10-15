using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.Logic.Object
{
    public sealed class W3dTankDraw : W3dModelDraw
    {
        private readonly W3dTankDrawModuleData _data;
        private readonly FXParticleSystemTemplate _treadDebrisLeft;
        private readonly FXParticleSystemTemplate _treadDebrisRight;

        private static readonly string[] MeshNames = new[]
        {
            "TREADSL01",
            "TREADSR01",
        };

        internal W3dTankDraw(W3dTankDrawModuleData data, GameObject gameObject, GameContext context)
            : base(data, gameObject, context)
        {
            _data = data;
            _treadDebrisLeft = data.TreadDebrisLeft?.Value ?? context.AssetLoadContext.AssetStore.FXParticleSystemTemplates.GetByName("TrackDebrisDirtLeft");
            _treadDebrisRight = data.TreadDebrisRight?.Value ?? context.AssetLoadContext.AssetStore.FXParticleSystemTemplates.GetByName("TrackDebrisDirtRight");
        }

        internal override void Update(in TimeInterval gameTime)
        {
            base.Update(gameTime);

            // TODO: Tread debris

            // Animate treads
            var animationRate = _data.TreadAnimationRate * GameObject.Speed;
            foreach (var meshName in MeshNames)
            {
                // TODO: We need to update FixedFunctionShaderResources.TextureMapping.UVPerSec.X with animationRate.
                // But right now that value is shared amongst all model instances that use this model.
                // We need to make it a per-model-instance value.
            }
        }
    }

    /// <summary>
    /// Default Draw used by tanks. Hardcoded to call for the TrackDebrisDirtRight and 
    /// TrackDebrisDirtLeft particle system definitions.
    /// </summary>
    public class W3dTankDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dTankDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal static new readonly IniParseTable<W3dTankDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dTankDrawModuleData>
            {
                { "TreadDebrisLeft", (parser, x) => x.TreadDebrisLeft = parser.ParseFXParticleSystemTemplateReference() },
                { "TreadDebrisRight", (parser, x) => x.TreadDebrisRight = parser.ParseFXParticleSystemTemplateReference() },

                { "TreadAnimationRate", (parser, x) => x.TreadAnimationRate = parser.ParseFloat() },
                { "TreadDriveSpeedFraction", (parser, x) => x.TreadDriveSpeedFraction = parser.ParseFloat() },
                { "TreadPivotSpeedFraction", (parser, x) => x.TreadPivotSpeedFraction = parser.ParseFloat() },
            });

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public LazyAssetReference<FXParticleSystemTemplate> TreadDebrisLeft { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public LazyAssetReference<FXParticleSystemTemplate> TreadDebrisRight { get; private set; }

        /// <summary>
        /// How fast to move the tread texture, in texture units per second.
        /// </summary>
        public float TreadAnimationRate { get; private set; }

        public float TreadDriveSpeedFraction { get; private set; }
        public float TreadPivotSpeedFraction { get; private set; }

        internal override DrawModule CreateDrawModule(GameObject gameObject, GameContext context)
        {
            return new W3dTankDraw(this, gameObject, context);
        }
    }
}
