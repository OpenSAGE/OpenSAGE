using System;
using System.Numerics;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal readonly struct RenderItem : IComparable<RenderItem>
    {
        public readonly ShaderSet ShaderSet;
        public readonly Pipeline Pipeline;
        public readonly BoundingBox BoundingBox;
        public readonly Matrix4x4 World;
        public readonly MeshShaderResources.RenderItemConstantsPS? RenderItemConstantsPS;
        public readonly BeforeRenderDelegate BeforeRenderCallback;
        public readonly uint StartIndex;
        public readonly uint IndexCount;
        public readonly DeviceBuffer IndexBuffer;

        public readonly int Key;

        public RenderItem(
            ShaderSet shaderSet,
            Pipeline pipeline,
            in BoundingBox boundingBox,
            in Matrix4x4 world,
            uint startIndex,
            uint indexCount,
            DeviceBuffer indexBuffer,
            BeforeRenderDelegate beforeRenderCallback,
            MeshShaderResources.RenderItemConstantsPS? renderItemConstantsPS = null)
        {
            ShaderSet = shaderSet;
            Pipeline = pipeline;
            BoundingBox = boundingBox;
            World = world;
            StartIndex = startIndex;
            IndexCount = indexCount;
            IndexBuffer = indexBuffer;
            BeforeRenderCallback = beforeRenderCallback;
            RenderItemConstantsPS = renderItemConstantsPS;

            // Key.
            Key = 0;

            // Bit 24-31: ShaderSet
            Key |= (shaderSet.Id << 24);

            // Bit 8-23: Pipeline
            Key |= (pipeline.GetHashCode()) << 8;
        }

        int IComparable<RenderItem>.CompareTo(RenderItem other)
        {
            return Key.CompareTo(other.Key);
        }
    }

    public delegate void BeforeRenderDelegate(CommandList commandList, RenderContext context);
}
