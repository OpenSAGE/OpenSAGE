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
        private readonly Dictionary<MeshConstantsKey, ConstantBuffer<MeshConstants>> _meshConstantsBuffers = new();

        public readonly ResourceLayout RenderItemConstantsResourceLayout;

        public MeshShaderResources(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            RenderItemConstantsResourceLayout = AddDisposable(graphicsDevice.ResourceFactory.CreateResourceLayout(
                new ResourceLayoutDescription(
                    new ResourceLayoutElementDescription("MeshConstants", ResourceKind.UniformBuffer, ShaderStages.Vertex | ShaderStages.Fragment),
                    new ResourceLayoutElementDescription("RenderItemConstantsVS", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("SkinningBuffer", ResourceKind.StructuredBufferReadOnly, ShaderStages.Vertex),
                    new ResourceLayoutElementDescription("RenderItemConstantsPS", ResourceKind.UniformBuffer, ShaderStages.Fragment))));
        }

        public ConstantBuffer<MeshConstants> GetCachedMeshConstantsBuffer(bool isSkinned, bool hasHouseColor)
        {
            var key = new MeshConstantsKey(isSkinned, hasHouseColor);

            if (!_meshConstantsBuffers.TryGetValue(key, out var result))
            {
                var meshConstantsBuffer = AddDisposable(new ConstantBuffer<MeshConstants>(_graphicsDevice));
                meshConstantsBuffer.Value.SkinningEnabled = isSkinned;
                meshConstantsBuffer.Value.HasHouseColor = hasHouseColor;
                meshConstantsBuffer.Update(_graphicsDevice);

                _meshConstantsBuffers.Add(key, result = meshConstantsBuffer);
            }

            return result;
        }

        private readonly record struct MeshConstantsKey(bool IsSkinned, bool HasHouseColor);

        public ResourceLayout[] CreateResourceLayouts(
            GlobalShaderResources globalShaderResources,
            ResourceLayout materialResourceLayout)
        {
            return new[]
            {
                globalShaderResources.GlobalConstantsResourceLayout,
                globalShaderResources.ForwardPassResourceLayout,
                materialResourceLayout,
                RenderItemConstantsResourceLayout,
            };
        }

        public ResourceSet CreateRenderItemConstantsResourceSet(
            ConstantBuffer<MeshConstants> meshConstantsBuffer,
            ConstantBuffer<RenderItemConstantsVS> renderItemConstantsVSBuffer,
            DeviceBuffer skinningBuffer,
            ConstantBuffer<RenderItemConstantsPS> renderItemConstantsPSBuffer)
        {
            return _graphicsDevice.ResourceFactory.CreateResourceSet(
                new ResourceSetDescription(
                    RenderItemConstantsResourceLayout,
                    meshConstantsBuffer.Buffer,
                    renderItemConstantsVSBuffer.Buffer,
                    skinningBuffer,
                    renderItemConstantsPSBuffer.Buffer));
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
        public struct RenderItemConstantsPS : IEquatable<RenderItemConstantsPS>
        {
            public Vector3 HouseColor;
            public float Opacity;
            public Vector3 TintColor;
#pragma warning disable CS0169
            private readonly float _padding;

            public override bool Equals(object obj)
            {
                return obj is RenderItemConstantsPS pS && Equals(pS);
            }

            public bool Equals(RenderItemConstantsPS other)
            {
                return HouseColor.Equals(other.HouseColor) &&
                       Opacity == other.Opacity &&
                       TintColor.Equals(other.TintColor);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(HouseColor, Opacity, TintColor);
            }

            public static bool operator ==(RenderItemConstantsPS left, RenderItemConstantsPS right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(RenderItemConstantsPS left, RenderItemConstantsPS right)
            {
                return !(left == right);
            }
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
