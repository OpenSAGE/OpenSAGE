using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class ShaderMaterialResourceCache : DisposableBase
    {
        private readonly Dictionary<ShaderSet, Pipeline> _pipelines;
        private readonly ContentManager _contentManager;

        public readonly Pipeline DepthPipeline;

        public ShaderMaterialResourceCache(ContentManager contentManager)
        {
            _pipelines = new Dictionary<ShaderSet, Pipeline>();
            _contentManager = contentManager;

            DepthPipeline = AddDisposable(MeshDepthResourceUtility.CreateDepthPipeline(contentManager, PrimitiveTopology.TriangleStrip));
        }

        public Pipeline GetPipeline(ShaderSet shaderSet)
        {
            if (!_pipelines.TryGetValue(shaderSet, out var result))
            {
                _pipelines.Add(shaderSet, result = AddDisposable(_contentManager.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        BlendStateDescription.SingleDisabled,
                        DepthStencilStateDescription.DepthOnlyLessEqual,
                        RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                        PrimitiveTopology.TriangleStrip,
                        shaderSet.Description,
                        shaderSet.ResourceLayouts,
                        RenderPipeline.GameOutputDescription))));
            }

            return result;
        }
    }
}
