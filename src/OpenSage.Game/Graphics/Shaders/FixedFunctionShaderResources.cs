using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Graphics.Mathematics;
using OpenSage.Rendering;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class FixedFunctionShaderResources : ShaderSetBase
    {
        private readonly Dictionary<PipelineKey, Pipeline> _pipelineCache = new();
        private readonly Dictionary<MaterialKey, Material> _materialCache = new();
        private readonly Dictionary<MaterialConstantsType, DeviceBuffer> _materialConstantsBufferCache = new();
        private readonly Dictionary<MaterialConstantsKey, ResourceSet> _materialConstantsCache = new();

        public FixedFunctionShaderResources(
            ShaderSetStore store)
            : base(store, "FixedFunction", MeshShaderResources.MeshVertex.VertexDescriptors)
        {
        }

        public Material GetCachedMaterial(
            FaceCullMode cullMode,
            bool depthWriteEnabled,
            ComparisonKind depthComparison,
            bool blendEnabled,
            BlendFactor sourceFactor,
            BlendFactor destinationColorFactor,
            BlendFactor destinationAlphaFactor,
            in MaterialConstantsType materialConstants,
            Texture texture0,
            Texture texture1)
        {
            var pipelineKey = new PipelineKey(
                cullMode,
                depthWriteEnabled,
                depthComparison,
                blendEnabled,
                sourceFactor,
                destinationColorFactor,
                destinationAlphaFactor);

            var materialConstantsKey = new MaterialConstantsKey(
                materialConstants,
                texture0,
                texture1);

            var materialKey = new MaterialKey(
                pipelineKey,
                materialConstantsKey);

            if (!_materialCache.TryGetValue(materialKey, out var result))
            {
                var pipeline = GetCachedPipeline(pipelineKey);

                var materialResourceSet = GetCachedMaterialResourceSet(materialConstantsKey);

                result = AddDisposable(
                    new Material(
                        this,
                        pipeline,
                        materialResourceSet,
                        blendEnabled ? SurfaceType.Transparent : SurfaceType.Opaque));

                _materialCache.Add(materialKey, result);
            }

            return result;
        }

        private Pipeline GetCachedPipeline(in PipelineKey pipelineKey)
        {
            if (!_pipelineCache.TryGetValue(pipelineKey, out var result))
            {
                var blendState = new BlendStateDescription(
                    RgbaFloat.White,
                    new BlendAttachmentDescription(
                        pipelineKey.BlendEnabled,
                        pipelineKey.SourceFactor,
                        pipelineKey.DestinationColorFactor,
                        BlendFunction.Add,
                        pipelineKey.SourceFactor,
                        pipelineKey.DestinationAlphaFactor,
                        BlendFunction.Add));

                var depthState = DepthStencilStateDescription.DepthOnlyLessEqual;
                depthState.DepthWriteEnabled = pipelineKey.DepthWriteEnabled;
                depthState.DepthComparison = pipelineKey.DepthComparison;

                var rasterizerState = RasterizerStateDescriptionUtility.DefaultFrontIsCounterClockwise;
                rasterizerState.CullMode = pipelineKey.CullMode;

                _pipelineCache.Add(pipelineKey, result = AddDisposable(GraphicsDevice.ResourceFactory.CreateGraphicsPipeline(
                    new GraphicsPipelineDescription(
                        blendState,
                        depthState,
                        rasterizerState,
                        PrimitiveTopology.TriangleList,
                        Description,
                        ResourceLayouts,
                        Store.OutputDescription))));
            }

            return result;
        }

        private ResourceSet GetCachedMaterialResourceSet(in MaterialConstantsKey key)
        {
            if (!_materialConstantsCache.TryGetValue(key, out var result))
            {
                var materialConstantsBuffer = GetCachedMaterialConstantsBuffer(key.MaterialConstants);

                result = AddDisposable(
                    GraphicsDevice.ResourceFactory.CreateResourceSet(
                        new ResourceSetDescription(
                            MaterialResourceLayout,
                            materialConstantsBuffer,
                            key.Texture0,
                            key.Texture1,
                            GraphicsDevice.Aniso4xSampler)));

                _materialConstantsCache.Add(key, result);
            }

            return result;
        }

        private DeviceBuffer GetCachedMaterialConstantsBuffer(in MaterialConstantsType key)
        {
            if (!_materialConstantsBufferCache.TryGetValue(key, out var result))
            {
                result = AddDisposable(GraphicsDevice.CreateStaticBuffer(key, BufferUsage.UniformBuffer));

                _materialConstantsBufferCache.Add(key, result);
            }

            return result;
        }

        private readonly record struct MaterialKey(
            PipelineKey PipelineKey,
            MaterialConstantsKey MaterialConstantsKey);

        private readonly record struct PipelineKey(
            FaceCullMode CullMode,
            bool DepthWriteEnabled,
            ComparisonKind DepthComparison,
            bool BlendEnabled,
            BlendFactor SourceFactor,
            BlendFactor DestinationColorFactor,
            BlendFactor DestinationAlphaFactor);

        private readonly record struct MaterialConstantsKey(
            MaterialConstantsType MaterialConstants,
            Texture Texture0,
            Texture Texture1);

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

        public struct TextureMapping : IEquatable<TextureMapping>
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

            public override bool Equals(object obj)
            {
                return obj is TextureMapping mapping && Equals(mapping);
            }

            public bool Equals(TextureMapping other)
            {
                return
                    MappingType == other.MappingType &&
                    Speed == other.Speed &&
                    Fps == other.Fps &&
                    Log2Width == other.Log2Width &&
                    UVPerSec.Equals(other.UVPerSec) &&
                    UVScale.Equals(other.UVScale) &&
                    UVCenter.Equals(other.UVCenter) &&
                    UVAmplitude.Equals(other.UVAmplitude) &&
                    UVFrequency.Equals(other.UVFrequency) &&
                    UVPhase.Equals(other.UVPhase) &&
                    UVStep.Equals(other.UVStep) &&
                    StepsPerSecond == other.StepsPerSecond;
            }

            public override int GetHashCode()
            {
                var hash = new HashCode();
                hash.Add(MappingType);
                hash.Add(Speed);
                hash.Add(Fps);
                hash.Add(Log2Width);
                hash.Add(UVPerSec);
                hash.Add(UVScale);
                hash.Add(UVCenter);
                hash.Add(UVAmplitude);
                hash.Add(UVFrequency);
                hash.Add(UVPhase);
                hash.Add(UVStep);
                hash.Add(StepsPerSecond);
                return hash.ToHashCode();
            }
        }

        public struct VertexMaterial : IEquatable<VertexMaterial>
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

            public override bool Equals(object obj)
            {
                return obj is VertexMaterial material && Equals(material);
            }

            public bool Equals(VertexMaterial other)
            {
                return
                    Ambient.Equals(other.Ambient) &&
                    Diffuse.Equals(other.Diffuse) &&
                    Specular.Equals(other.Specular) &&
                    Shininess == other.Shininess &&
                    Emissive.Equals(other.Emissive) &&
                    Opacity == other.Opacity &&
                    TextureMappingStage0.Equals(other.TextureMappingStage0) &&
                    TextureMappingStage1.Equals(other.TextureMappingStage1);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    Ambient,
                    Diffuse,
                    Specular,
                    Shininess,
                    Emissive,
                    Opacity,
                    TextureMappingStage0,
                    TextureMappingStage1);
            }
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

        public struct ShadingConfiguration : IEquatable<ShadingConfiguration>
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

            public override bool Equals(object obj)
            {
                return obj is ShadingConfiguration configuration && Equals(configuration);
            }

            public bool Equals(ShadingConfiguration other)
            {
                return
                    DiffuseLightingType == other.DiffuseLightingType &&
                    SpecularEnabled.Equals(other.SpecularEnabled) &&
                    TexturingEnabled.Equals(other.TexturingEnabled) &&
                    SecondaryTextureColorBlend == other.SecondaryTextureColorBlend &&
                    SecondaryTextureAlphaBlend == other.SecondaryTextureAlphaBlend &&
                    AlphaTest.Equals(other.AlphaTest);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    DiffuseLightingType,
                    SpecularEnabled,
                    TexturingEnabled,
                    SecondaryTextureColorBlend,
                    SecondaryTextureAlphaBlend,
                    AlphaTest);
            }
        }

        public struct MaterialConstantsType : IEquatable<MaterialConstantsType>
        {
#pragma warning disable CS0169
            private readonly Vector3 _padding;
#pragma warning restore CS0169

            public int NumTextureStages;

            public VertexMaterial Material;
            public ShadingConfiguration Shading;

            public override bool Equals(object obj)
            {
                return obj is MaterialConstantsType type && Equals(type);
            }

            public bool Equals(MaterialConstantsType other)
            {
                return
                    NumTextureStages == other.NumTextureStages &&
                    Material.Equals(other.Material) &&
                    Shading.Equals(other.Shading);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(
                    NumTextureStages,
                    Material,
                    Shading);
            }
        }
    }
}
