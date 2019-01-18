using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Shaders
{
    internal sealed class MeshShaderResources : DisposableBase
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly Dictionary<MeshConstantsKey, ResourceSet> _meshConstantsResourceSets;
        private readonly ResourceLayout _meshConstantsResourceLayout;
        private readonly ResourceLayout _skinningResourceLayout;

        public readonly ResourceSet SamplerResourceSet;

        public MeshShaderResources(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _meshConstantsResourceSets = new Dictionary<MeshConstantsKey, ResourceSet>();

            _meshConstantsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MeshConstants", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment))));

            _skinningResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("SkinningBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex))));

            var samplerResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("Sampler", ResourceKind.Sampler, ShaderStages.Fragment))));

            SamplerResourceSet = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    samplerResourceLayout,
                    graphicsDevice.Aniso4xSampler)));
        }

        public ResourceSet GetCachedMeshResourceSet(bool isSkinned, bool hasHouseColor)
        {
            var key = new MeshConstantsKey(isSkinned, hasHouseColor);

            if (!_meshConstantsResourceSets.TryGetValue(key, out var result))
            {
                var meshConstantsBuffer = AddDisposable(new ConstantBuffer<MeshConstants>(_graphicsDevice));
                meshConstantsBuffer.Value.SkinningEnabled = isSkinned;
                meshConstantsBuffer.Value.HasHouseColor = hasHouseColor;
                meshConstantsBuffer.Update(_graphicsDevice);

                var meshConstantsResourceSet = AddDisposable(_graphicsDevice.ResourceFactory.CreateResourceSet(
                    new ResourceSetDescription(
                        _meshConstantsResourceLayout,
                        meshConstantsBuffer.Buffer)));

                _meshConstantsResourceSets.Add(key, result = meshConstantsResourceSet);
            }

            return result;
        }

        private readonly struct MeshConstantsKey : IEquatable<MeshConstantsKey>
        {
            public readonly bool IsSkinned;
            public readonly bool HasHouseColor;

            public MeshConstantsKey(bool isSkinned, bool hasHouseColor)
            {
                IsSkinned = isSkinned;
                HasHouseColor = hasHouseColor;
            }

            public override bool Equals(object obj)
            {
                return obj is MeshConstantsKey && Equals((MeshConstantsKey) obj);
            }

            public bool Equals(MeshConstantsKey other)
            {
                return
                    IsSkinned == other.IsSkinned &&
                    HasHouseColor == other.HasHouseColor;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(IsSkinned, HasHouseColor);
            }

            public static bool operator ==(in MeshConstantsKey key1, in MeshConstantsKey key2)
            {
                return key1.Equals(key2);
            }

            public static bool operator !=(in MeshConstantsKey key1, in MeshConstantsKey key2)
            {
                return !(key1 == key2);
            }
        }

        public ResourceSet CreateSkinningResourceSet(DeviceBuffer skinningBuffer)
        {
            return _graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    _skinningResourceLayout,
                    skinningBuffer));
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MeshConstants
        {
#pragma warning disable CS0169
            private readonly Vector2 _padding;
#pragma warning restore CS0169
            public Bool32 SkinningEnabled;
            public Bool32 HasHouseColor;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RenderItemConstantsVS
        {
            public Matrix4x4 World;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RenderItemConstantsPS
        {
            public Vector3 HouseColor;
#pragma warning disable CS0169
            private readonly float _padding;
#pragma warning restore CS0169
        }

        public static class MeshVertex
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Basic
            {
                public Vector3 Position0;
                public Vector3 Position1;
                public Vector3 Normal0;
                public Vector3 Normal1;
                public Vector3 Tangent;
                public Vector3 Binormal;
                public uint BoneIndex0;
                public uint BoneIndex1;
                public float BoneWeight0;
                public float BoneWeight1;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TexCoords
            {
                public Vector2 UV0;
                public Vector2 UV1;
            }

            public static readonly VertexLayoutDescription[] VertexDescriptors = new[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("TANGENT", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("BINORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
                    new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
                    new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.UInt1),
                    new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1),
                    new VertexElementDescription("BLENDINDICES", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float1)),

                new VertexLayoutDescription(
                    new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2))
            };
        }
    }
}
