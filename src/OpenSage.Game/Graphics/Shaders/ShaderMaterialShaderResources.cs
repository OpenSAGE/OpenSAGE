using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal abstract class ShaderMaterialShaderResources : ShaderResourcesBase
    {
        private readonly ResourceLayout _materialResourceLayout;

        public readonly Dictionary<string, ResourceBinding> MaterialResourceBindings;
        
        public readonly Pipeline Pipeline;

        protected ShaderMaterialShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources,
            MeshShaderResources meshShaderResources,
            string shaderName,
            Func<IEnumerable<ResourceBinding>> createMaterialResourceBindings)
            : base(
                  graphicsDevice,
                  shaderName,
                  new GlobalResourceSetIndices(0u, LightingType.Object, 1u, 2u, 3u, 6u),
                  MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var materialResourceBindings = createMaterialResourceBindings().ToArray();

            MaterialResourceBindings = materialResourceBindings.ToDictionary(x => x.Description.Name);

            var materialResourceLayoutElements = materialResourceBindings
                .Select(x => x.Description)
                .ToArray();

            _materialResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(materialResourceLayoutElements)));

            var resourceLayouts = meshShaderResources.CreateResourceLayouts(
                globalShaderResources,
                _materialResourceLayout);

            Pipeline = AddDisposable(graphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleDisabled,
                    DepthStencilStateDescription.DepthOnlyLessEqual,
                    RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                    PrimitiveTopology.TriangleStrip,
                    ShaderSet.Description,
                    resourceLayouts,
                    RenderPipeline.GameOutputDescription)));
        }

        public ResourceSet CreateMaterialResourceSet(BindableResource[] resources)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _materialResourceLayout,
                    resources));
        }
    }
}
