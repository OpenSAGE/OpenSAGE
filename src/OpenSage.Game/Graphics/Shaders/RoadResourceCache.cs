using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class RoadResourceCache : DisposableBase
    {
        private readonly Dictionary<Texture, ResourceSet> _resourceSets;
        private readonly ContentManager _contentManager;

        public readonly Pipeline Pipeline;

        public RoadResourceCache(ContentManager contentManager)
        {
            _resourceSets = new Dictionary<Texture, ResourceSet>();
            _contentManager = contentManager;

            Pipeline = AddDisposable(contentManager.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                new GraphicsPipelineDescription(
                    BlendStateDescription.SingleAlphaBlend,
                    DepthStencilStateDescription.DepthOnlyLessEqualRead,
                    RasterizerStateDescriptionUtility.CullNoneSolid, // TODO
                    PrimitiveTopology.TriangleList,
                    contentManager.ShaderLibrary.Road.Description,
                    contentManager.ShaderLibrary.Road.ResourceLayouts,
                    RenderPipeline.GameOutputDescription)));
        }

        public ResourceSet GetResourceSet(Texture texture)
        {
            if (!_resourceSets.TryGetValue(texture, out var result))
            {
                result = AddDisposable(_contentManager.GraphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _contentManager.ShaderLibrary.Road.ResourceLayouts[4],
                        texture,
                        _contentManager.GraphicsDevice.Aniso4xSampler)));

                _resourceSets.Add(texture, result);
            }
            return result;
        }
    }
}
