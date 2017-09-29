using System;
using System.Numerics;
using System.Runtime.InteropServices;
using LLGfx;
using OpenSage.Graphics.Util;

namespace OpenSage.Graphics.Effects
{
    public sealed class MeshEffect : Effect, IEffectMatrices, IEffectLights, IEffectTime
    {
        public const int MaxTextures = 32;

        private readonly DynamicBuffer<SkinningConstants> _skinningConstantBuffer;
        private SkinningConstants _skinningConstants;

        private readonly DynamicBuffer<MeshTransformConstants> _transformConstantBuffer;
        private MeshTransformConstants _transformConstants;

        private readonly DynamicBuffer<LightingConstants> _lightingConstantBuffer;
        private LightingConstants _lightingConstants;

        private readonly DynamicBuffer<PerDrawConstants> _perDrawConstantBuffer;
        private PerDrawConstants _perDrawConstants;

        private MeshEffectDirtyFlags _dirtyFlags;

        private Matrix4x4[] _absoluteBonesMatrices;

        private Matrix4x4 _world = Matrix4x4.Identity;
        private Matrix4x4 _view;
        private Matrix4x4 _projection;

        private StaticBuffer<VertexMaterial> _materialsBuffer;
        private TextureSet _textures;
        private StaticBuffer<MeshTextureIndex> _textureIndicesBuffer;
        private StaticBuffer<uint> _materialIndicesBuffer;

        [Flags]
        private enum MeshEffectDirtyFlags
        {
            None = 0,

            PerDrawConstants = 0x1,

            SkinningConstants = 0x2,

            TransformConstants = 0x4,

            LightingConstants = 0x8,

            MaterialsBuffer = 0x10,
            Textures = 0x20,
            TextureIndicesBuffer = 0x40,
            MaterialIndicesBuffer = 0x80,

            All = PerDrawConstants
                | SkinningConstants
                | TransformConstants
                | LightingConstants
                | MaterialsBuffer
                | Textures
                | TextureIndicesBuffer
                | MaterialIndicesBuffer
        }

        public MeshEffect(GraphicsDevice graphicsDevice)
            : base(
                  graphicsDevice, 
                  "MeshVS", 
                  "MeshPS",
                  CreateVertexDescriptor(),
                  CreatePipelineLayoutDescription())
        {
            _skinningConstantBuffer = AddDisposable(DynamicBuffer<SkinningConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer));

            _transformConstantBuffer = AddDisposable(DynamicBuffer<MeshTransformConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer));

            _lightingConstantBuffer = AddDisposable(DynamicBuffer<LightingConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer));

            _perDrawConstantBuffer = AddDisposable(DynamicBuffer<PerDrawConstants>.Create(graphicsDevice, BufferUsageFlags.ConstantBuffer));
        }

        private static VertexDescriptor CreateVertexDescriptor()
        {
            var vertexDescriptor = new VertexDescriptor();
            vertexDescriptor.SetAttributeDescriptor(0, "POSITION", 0, VertexFormat.Float3, 0, 0);
            vertexDescriptor.SetAttributeDescriptor(1, "NORMAL", 0, VertexFormat.Float3, 0, 12);
            vertexDescriptor.SetAttributeDescriptor(2, "BLENDINDICES", 0, VertexFormat.UInt, 0, 24);
            vertexDescriptor.SetAttributeDescriptor(3, "TEXCOORD", 0, VertexFormat.Float2, 1, 0);
            vertexDescriptor.SetAttributeDescriptor(4, "TEXCOORD", 1, VertexFormat.Float2, 1, 8);
            vertexDescriptor.SetLayoutDescriptor(0, 28);
            vertexDescriptor.SetLayoutDescriptor(1, 16);
            return vertexDescriptor;
        }

        private static PipelineLayoutDescription CreatePipelineLayoutDescription()
        {
            return new PipelineLayoutDescription
            {
                Entries = new[]
                {
                    // PerDrawCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Pixel,
                        ResourceType.ConstantBuffer,
                        1),

                    // MeshTransformCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Vertex,
                        ResourceType.ConstantBuffer,
                        0),

                    // SkinningCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Vertex,
                        ResourceType.ConstantBuffer,
                        1),

                    // LightingCB
                    PipelineLayoutEntry.CreateResource(
                        ShaderStageVisibility.Pixel,
                        ResourceType.ConstantBuffer,
                        0),

                    // Sorted in descending frequency of updating

                    // MaterialIndices
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Vertex,
                        ResourceType.StructuredBuffer,
                        0, 1),

                    // TextureIndices
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.StructuredBuffer,
                        1, 1),

                    // Materials
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.StructuredBuffer,
                        0, 1),

                    // Textures
                    PipelineLayoutEntry.CreateResourceView(
                        ShaderStageVisibility.Pixel,
                        ResourceType.Texture,
                        2, MaxTextures),
                },

                StaticSamplerStates = new[]
                {
                    new StaticSamplerDescription(
                        ShaderStageVisibility.Pixel,
                        0,
                        new SamplerStateDescription(SamplerFilter.Anisotropic))
                }
            };
        }

        protected override void OnBegin()
        {
            _dirtyFlags = MeshEffectDirtyFlags.All;
        }

        protected override void OnApply(CommandEncoder commandEncoder)
        {
            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.SkinningConstants))
            {
                if (_absoluteBonesMatrices != null)
                {
                    _skinningConstants.CopyFrom(_absoluteBonesMatrices);
                }

                _skinningConstantBuffer.UpdateData(ref _skinningConstants);

                commandEncoder.SetInlineConstantBuffer(2, _skinningConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.SkinningConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.TransformConstants))
            {
                _transformConstants.World = _world;
                _transformConstants.WorldViewProjection = _world * _view * _projection;

                _transformConstantBuffer.UpdateData(ref _transformConstants);

                commandEncoder.SetInlineConstantBuffer(1, _transformConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.TransformConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.LightingConstants))
            {
                Matrix4x4.Invert(_view, out var viewInverse);
                _lightingConstants.CameraPosition = viewInverse.Translation;

                _lightingConstantBuffer.UpdateData(ref _lightingConstants);

                commandEncoder.SetInlineConstantBuffer(3, _lightingConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.LightingConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.PerDrawConstants))
            {
                _perDrawConstantBuffer.UpdateData(ref _perDrawConstants);

                commandEncoder.SetInlineConstantBuffer(0, _perDrawConstantBuffer);

                _dirtyFlags &= ~MeshEffectDirtyFlags.PerDrawConstants;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.MaterialsBuffer))
            {
                commandEncoder.SetStaticBuffer(6, _materialsBuffer);
                _dirtyFlags &= ~MeshEffectDirtyFlags.MaterialsBuffer;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.Textures))
            {
                commandEncoder.SetTextureSet(7, _textures);
                _dirtyFlags &= ~MeshEffectDirtyFlags.Textures;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.TextureIndicesBuffer))
            {
                commandEncoder.SetStaticBuffer(5, _textureIndicesBuffer);
                _dirtyFlags &= ~MeshEffectDirtyFlags.TextureIndicesBuffer;
            }

            if (_dirtyFlags.HasFlag(MeshEffectDirtyFlags.MaterialIndicesBuffer))
            {
                commandEncoder.SetStaticBuffer(4, _materialIndicesBuffer);
                _dirtyFlags &= ~MeshEffectDirtyFlags.MaterialIndicesBuffer;
            }
        }

        public void SetAbsoluteBoneTransforms(Matrix4x4[] transforms)
        {
            if (transforms == _absoluteBonesMatrices)
            {
                return;
            }
            _absoluteBonesMatrices = transforms;
            _dirtyFlags |= MeshEffectDirtyFlags.SkinningConstants;
        }

        public void SetSkinningEnabled(bool enabled)
        {
            _transformConstants.SkinningEnabled = enabled;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetWorld(Matrix4x4 matrix)
        {
            _world = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetView(Matrix4x4 matrix)
        {
            _view = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
            _dirtyFlags |= MeshEffectDirtyFlags.LightingConstants;
        }

        public void SetProjection(Matrix4x4 matrix)
        {
            _projection = matrix;
            _dirtyFlags |= MeshEffectDirtyFlags.TransformConstants;
        }

        public void SetLights(ref Lights lights)
        {
            _lightingConstants.Lights = lights;
            _dirtyFlags |= MeshEffectDirtyFlags.LightingConstants;
        }

        public void SetMaterials(StaticBuffer<VertexMaterial> materialsBuffer)
        {
            _materialsBuffer = materialsBuffer;
            _dirtyFlags |= MeshEffectDirtyFlags.MaterialsBuffer;
        }

        public void SetTextures(TextureSet textures)
        {
            _textures = textures;
            _dirtyFlags |= MeshEffectDirtyFlags.Textures;
        }

        public void SetTextureIndices(StaticBuffer<MeshTextureIndex> textureIndicesBuffer)
        {
            _textureIndicesBuffer = textureIndicesBuffer;
            _dirtyFlags |= MeshEffectDirtyFlags.TextureIndicesBuffer;
        }

        public void SetMaterialIndices(StaticBuffer<uint> materialIndicesBuffer)
        {
            _materialIndicesBuffer = materialIndicesBuffer;
            _dirtyFlags |= MeshEffectDirtyFlags.MaterialIndicesBuffer;
        }

        public void SetPrimitiveOffset(uint primitiveOffset)
        {
            _perDrawConstants.PrimitiveOffset = primitiveOffset;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        public void SetNumTextureStages(uint numTextureStages)
        {
            _perDrawConstants.NumTextureStages = numTextureStages;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        public void SetAlphaTest(bool alphaTest)
        {
            _perDrawConstants.AlphaTest = alphaTest;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        public void SetTexturing(bool texturing)
        {
            _perDrawConstants.Texturing = texturing;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        public void SetTimeInSeconds(float time)
        {
            _perDrawConstants.TimeInSeconds = time;
            _dirtyFlags |= MeshEffectDirtyFlags.PerDrawConstants;
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct SkinningConstants
        {
            // Array of MaxBones * float4x3
            public fixed float Bones[ModelMesh.MaxBones * 12];

            public void CopyFrom(Matrix4x4[] matrices)
            {
                fixed (float* boneArray = Bones)
                {
                    for (var i = 0; i < matrices.Length; i++)
                    {
                        PointerUtil.CopyToMatrix4x3(
                            ref matrices[i],
                            boneArray + (i * 12));
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MeshTransformConstants
        {
            public Matrix4x4 WorldViewProjection;
            public Matrix4x4 World;
            public bool SkinningEnabled;
        }

        [StructLayout(LayoutKind.Explicit, Size = 20)]
        private struct PerDrawConstants
        {
            [FieldOffset(0)]
            public uint PrimitiveOffset;

            // Not actually per-draw, but we don't have a per-mesh CB.
            [FieldOffset(4)]
            public uint NumTextureStages;

            [FieldOffset(8)]
            public bool AlphaTest;

            [FieldOffset(12)]
            public bool Texturing;

            [FieldOffset(16)]
            public float TimeInSeconds;
        }
    }
}
