using System;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ParticleResourceCache : DisposableBase
    {
        private readonly Pipeline _alphaPipeline;
        private readonly Pipeline _additivePipeline;

        public ParticleResourceCache(ContentManager contentManager)
        {
            var graphicsDevice = contentManager.GraphicsDevice;

            Pipeline CreatePipeline(in BlendStateDescription blendStateDescription)
            {
                return graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendStateDescription,
                        DepthStencilStateDescription.DepthOnlyLessEqualRead,
                        RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                        PrimitiveTopology.TriangleList,
                        contentManager.ShaderLibrary.Particle.Description,
                        contentManager.ShaderLibrary.Particle.ResourceLayouts,
                        RenderPipeline.GameOutputDescription));
            }

            _alphaPipeline = AddDisposable(CreatePipeline(BlendStateDescription.SingleAlphaBlend));
            _additivePipeline = AddDisposable(CreatePipeline(BlendStateDescription.SingleAdditiveBlend));
        }

        public Pipeline GetPipeline(ParticleSystemShader shader)
        {
            switch (shader)
            {
                case ParticleSystemShader.Alpha:
                case ParticleSystemShader.AlphaTest: // TODO: proper implementation for AlphaTest
                    return _alphaPipeline;

                case ParticleSystemShader.Additive:
                    return _additivePipeline;

                default:
                    throw new ArgumentOutOfRangeException(nameof(shader));
            }
        }
    }
}
