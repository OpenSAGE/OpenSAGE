using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Mathematics;
using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class FixedFunctionShaderResources : ShaderResourcesBase
    {
        private readonly Dictionary<PipelineKey, Pipeline> _pipelines;
        private readonly ResourceLayout _materialResourceLayout;
        private readonly ResourceLayout[] _resourceLayouts;

        public FixedFunctionShaderResources(
            GraphicsDevice graphicsDevice,
            GlobalShaderResources globalShaderResources,
            MeshShaderResources meshShaderResources)
            : base(
                graphicsDevice,
                "FixedFunction",
                new GlobalResourceSetIndices(0u, LightingType.Object, 1u, 2u, 3u, 7u),
                MeshShaderResources.MeshVertex.VertexDescriptors)
        {
            _pipelines = new Dictionary<PipelineKey, Pipeline>();

            _materialResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MaterialConstants", ResourceKind.UniformBuffer, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Texture0", ResourceKind.TextureReadOnly, ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("Texture1", ResourceKind.TextureReadOnly, ShaderStages.Fragment))));

            _resourceLayouts = meshShaderResources.CreateResourceLayouts(
                globalShaderResources,
                _materialResourceLayout);
        }

        public Pipeline GetCachedPipeline(
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

                _pipelines.Add(key, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendState,
                        depthState,
                        rasterizerState,
                        PrimitiveTopology.TriangleList,
                        ShaderSet.Description,
                        _resourceLayouts,
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
                return obj is PipelineKey a && Equals(a);
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

        public ResourceSet CreateMaterialResourceSet(
            DeviceBuffer materialConstantsBuffer,
            Texture texture0,
            Texture texture1)
        {
            return GraphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _materialResourceLayout,
                    materialConstantsBuffer,
                    texture0,
                    texture1));
        }

        public enum TextureMappingType
        {
            Uv = 0,
            Environment = 1,
            LinearOffset = 2,
            Rotate = 3,
            SineLinearOffset = 4,
            StepLinearOffset = 5,
            Screen = 6,
            Scale = 7,
            Grid = 8,
            Random = 9,
            BumpEnv = 10,
            WsEnvironment = 11,
        }

        public struct TextureMapping
        {
            public TextureMappingType MappingType;

            public float Speed;
            public float Fps;
            public int Log2Width;

            public Vector2 UVPerSec;
            public Vector2 UVScale;
            public Vector2 UVCenter;
            public Vector2 UVAmplitude;
            public Vector2 UVFrequency;
            public Vector2 UVPhase;

            public Vector2 UVStep;
            public float StepsPerSecond;

#pragma warning disable IDE1006, CS0169
            private readonly float _Padding;
#pragma warning restore IDE1006, CS0169
        }

        public struct VertexMaterial
        {
            public Vector3 Ambient;

#pragma warning disable CS0169
            private readonly float _padding1;
#pragma warning restore CS0169

            public Vector3 Diffuse;

#pragma warning disable CS0169
            private readonly float _padding2;
#pragma warning restore CS0169

            public Vector3 Specular;
            public float Shininess;
            public Vector3 Emissive;
            public float Opacity;

            public TextureMapping TextureMappingStage0;
            public TextureMapping TextureMappingStage1;
        }

        public enum DiffuseLightingType
        {
            Disable = 0,
            Modulate = 1,
            Add = 2,
            BumpEnvMap = 3,
        }

        public enum SecondaryTextureBlend
        {
            Disable = 0,
            Detail = 1,
            Scale = 2,
            InvScale = 3,
            DetailBlend = 4,
            Add = 5
        }

        public struct ShadingConfiguration
        {
            public DiffuseLightingType DiffuseLightingType;
            public Bool32 SpecularEnabled;
            public Bool32 TexturingEnabled;
            public SecondaryTextureBlend SecondaryTextureColorBlend;
            public SecondaryTextureBlend SecondaryTextureAlphaBlend;
            public Bool32 AlphaTest;

#pragma warning disable CS0169
            private readonly Vector2 _padding;
#pragma warning restore CS0169
        }

        public struct MaterialConstantsType
        {
#pragma warning disable CS0169
            private readonly Vector3 _padding;
#pragma warning restore CS0169

            public int NumTextureStages;

            public VertexMaterial Material;
            public ShadingConfiguration Shading;
        }
    }
}
