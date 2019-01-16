using System;
using System.Collections.Generic;
using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class SpriteResourceCache : DisposableBase
    {
        private readonly ContentManager _contentManager;
        private readonly Dictionary<int, Pipeline> _pipelines;
        private readonly Dictionary<Sampler, ResourceSet> _samplerResourceSets;

        private static int GetPipelineKey(
            in BlendStateDescription blendStateDescription,
            in OutputDescription outputDescription)
        {
            return HashCode.Combine(
                blendStateDescription,
                outputDescription);
        }

        public SpriteResourceCache(ContentManager contentManager)
        {
            _contentManager = contentManager;

            _pipelines = new Dictionary<int, Pipeline>();
            _samplerResourceSets = new Dictionary<Sampler, ResourceSet>();
        }

        public Pipeline GetPipeline(
            in BlendStateDescription blendStateDescription,
            in OutputDescription outputDescription)
        {
            var key = GetPipelineKey(blendStateDescription, outputDescription);

            if (!_pipelines.TryGetValue(key, out var result))
            {
                _pipelines.Add(key, result = AddDisposable(_contentManager.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendStateDescription,
                        DepthStencilStateDescription.Disabled,
                        RasterizerStateDescriptionUtility.CullNoneSolid, // TODO
                        PrimitiveTopology.TriangleList,
                        _contentManager.ShaderLibrary.Sprite.Description,
                        _contentManager.ShaderLibrary.Sprite.ResourceLayouts,
                        outputDescription))));
            }

            return result;
        }

        public ResourceSet GetSamplerResourceSet(Sampler sampler)
        {
            if (!_samplerResourceSets.TryGetValue(sampler, out var result))
            {
                _samplerResourceSets.Add(sampler, result = AddDisposable(_contentManager.GraphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _contentManager.ShaderLibrary.Sprite.ResourceLayouts[1],
                        sampler))));
            }

            return result;
        }
    }
}
