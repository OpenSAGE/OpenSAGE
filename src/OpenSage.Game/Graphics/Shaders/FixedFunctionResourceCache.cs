using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class FixedFunctionResourceCache : DisposableBase
    {
        private readonly Dictionary<PipelineKey, Pipeline> _pipelines;
        private readonly ContentManager _contentManager;

        public readonly ResourceSet SamplerResourceSet;

        public readonly Pipeline DepthPipeline;

        public FixedFunctionResourceCache(ContentManager contentManager)
        {
            _pipelines = new Dictionary<PipelineKey, Pipeline>();
            _contentManager = contentManager;

            SamplerResourceSet = AddDisposable(contentManager.GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    contentManager.ShaderLibrary.FixedFunction.ResourceLayouts[6],
                    contentManager.GraphicsDevice.Aniso4xSampler)));

            DepthPipeline = AddDisposable(MeshDepthResourceUtility.CreateDepthPipeline(contentManager, PrimitiveTopology.TriangleList));
        }

        public Pipeline GetPipeline(
            FaceCullMode cullMode,
            bool depthWriteEnabled,
            ComparisonKind depthComparison,
            bool blendEnabled,
            BlendFactor sourceFactor,
            BlendFactor destinationColorFactor,
            BlendFactor destinationAlphaFactor)
        {
            var key = new PipelineKey(
                cullMode,
                depthWriteEnabled,
                depthComparison,
                blendEnabled,
                sourceFactor,
                destinationColorFactor,
                destinationAlphaFactor);

            if (!_pipelines.TryGetValue(key, out var result))
            {
                var blendState = new BlendStateDescription(
                    RgbaFloat.White,
                    new BlendAttachmentDescription(
                        blendEnabled,
                        sourceFactor,
                        destinationColorFactor,
                        BlendFunction.Add,
                        sourceFactor,
                        destinationAlphaFactor,
                        BlendFunction.Add));

                var depthState = DepthStencilStateDescription.DepthOnlyLessEqual;
                depthState.DepthWriteEnabled = depthWriteEnabled;
                depthState.DepthComparison = depthComparison;

                var rasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
                rasterizerState.CullMode = cullMode;

                _pipelines.Add(key, result = AddDisposable(_contentManager.GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendState,
                        depthState,
                        rasterizerState,
                        PrimitiveTopology.TriangleList,
                        _contentManager.ShaderLibrary.FixedFunction.Description,
                        _contentManager.ShaderLibrary.FixedFunction.ResourceLayouts,
                        RenderPipeline.GameOutputDescription))));
            }

            return result;
        }

        private readonly struct PipelineKey : IEquatable<PipelineKey>
        {
            public readonly FaceCullMode CullMode;
            public readonly bool DepthWriteEnabled;
            public readonly ComparisonKind DepthComparison;
            public readonly bool BlendEnabled;
            public readonly BlendFactor SourceFactor;
            public readonly BlendFactor DestinationColorFactor;
            public readonly BlendFactor DestinationAlphaFactor;

            private readonly int _hashCode;

            public PipelineKey(
                FaceCullMode cullMode,
                bool depthWriteEnabled,
                ComparisonKind depthComparison,
                bool blendEnabled,
                BlendFactor sourceFactor,
                BlendFactor destinationColorFactor,
                BlendFactor destinationAlphaFactor)
            {
                CullMode = cullMode;
                DepthWriteEnabled = depthWriteEnabled;
                DepthComparison = depthComparison;
                BlendEnabled = blendEnabled;
                SourceFactor = sourceFactor;
                DestinationColorFactor = destinationColorFactor;
                DestinationAlphaFactor = destinationAlphaFactor;

                _hashCode = HashCode.Combine(
                    CullMode,
                    DepthWriteEnabled,
                    DepthComparison,
                    BlendEnabled,
                    SourceFactor,
                    DestinationColorFactor,
                    DestinationAlphaFactor);
            }

            public override bool Equals(object obj)
            {
                return obj is PipelineKey && Equals((PipelineKey) obj);
            }

            public bool Equals(PipelineKey other)
            {
                return
                    CullMode == other.CullMode &&
                    DepthWriteEnabled == other.DepthWriteEnabled &&
                    DepthComparison == other.DepthComparison &&
                    BlendEnabled == other.BlendEnabled &&
                    SourceFactor == other.SourceFactor &&
                    DestinationColorFactor == other.DestinationColorFactor &&
                    DestinationAlphaFactor == other.DestinationAlphaFactor;
            }

            public override int GetHashCode() => _hashCode;

            public static bool operator ==(PipelineKey key1, PipelineKey key2)
            {
                return key1.Equals(key2);
            }

            public static bool operator !=(PipelineKey key1, PipelineKey key2)
            {
                return !(key1 == key2);
            }
        }
    }
}
