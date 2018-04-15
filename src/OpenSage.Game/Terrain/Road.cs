using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class Road : DisposableBase
    {
        private readonly DeviceBuffer _vertexBuffer;
        private readonly BoundingBox _boundingBox;

        private readonly DeviceBuffer _indexBuffer;
        private readonly uint _numIndices;

        private readonly RoadMaterial _material;

        internal Road(
            GraphicsDevice graphicsDevice,
            RoadTemplate template,
            in Vector3 startPosition,
            in Vector3 endPosition)
        {
            var vertices = new RoadVertex[4];

            //_boundingBox = BoundingBox.CreateFromPoints();

            _vertexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                vertices,
                BufferUsage.VertexBuffer));

            _numIndices = 6;

            var indices = new ushort[_numIndices];

            _indexBuffer = AddDisposable(graphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer));
        }

        internal void BuildRenderList(RenderList renderList)
        {
            renderList.Opaque.AddRenderItemDrawIndexed(
                _material,
                _vertexBuffer,
                null,
                CullFlags.None,
                _boundingBox,
                Matrix4x4.Identity,
                0,
                _numIndices,
                _indexBuffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RoadVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
            new VertexElementDescription("POSITION", VertexElementSemantic.Position, VertexElementFormat.Float3),
            new VertexElementDescription("NORMAL", VertexElementSemantic.Normal, VertexElementFormat.Float3),
            new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
