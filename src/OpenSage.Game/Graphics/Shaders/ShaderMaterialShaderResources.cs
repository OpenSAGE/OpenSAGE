using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal abstract class ShaderMaterialShaderResources : ShaderSet
    {
        public readonly Dictionary<string, ResourceBinding> MaterialResourceBindings;
        
        public readonly Pipeline Pipeline;

        protected ShaderMaterialShaderResources(
            ShaderSetStore store,
            string shaderName,
            Func<IEnumerable<ResourceBinding>> createMaterialResourceBindings)
            : base(store, shaderName, MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            var materialResourceBindings = createMaterialResourceBindings().ToArray();

            MaterialResourceBindings = materialResourceBindings.ToDictionary(x => x.Description.Name);

            Pipeline = AddDisposable(
                GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        BlendStateDescription.SingleDisabled,
                        DepthStencilStateDescription.DepthOnlyLessEqual,
                        RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise,
                        PrimitiveTopology.TriangleList,
                        Description,
                        ResourceLayouts,
                        store.OutputDescription)));
        }

        public ResourceSet CreateMaterialResourceSet(BindableResource[] resources)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    MaterialResourceLayout,
                    resources));
        }
    }
}
