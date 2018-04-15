using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
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
            ContentManager contentManager,
            RoadTemplate template,
            Vector3 startPosition,
            Vector3 endPosition)
        {
            startPosition.Z += 1f;
            endPosition.Z += 1f;

            var direction = Vector3.Normalize(endPosition - startPosition);
            var centerToEdgeDirection = Vector3.Cross(Vector3.UnitZ, direction);

            var halfWidth = template.RoadWidth / 2;

            var vertices = new RoadVertex[4];

            vertices[0] = new RoadVertex
            {
                Position = startPosition - centerToEdgeDirection * halfWidth,
                Normal = Vector3.UnitZ,
                UV = new Vector2(0, 0), // TODO
            };

            vertices[1] = new RoadVertex
            {
                Position = startPosition + centerToEdgeDirection * halfWidth,
                Normal = Vector3.UnitZ,
                UV = new Vector2(0, 0.324f), // TODO
            };

            vertices[2] = new RoadVertex
            {
                Position = endPosition - centerToEdgeDirection * halfWidth,
                Normal = Vector3.UnitZ,
                UV = new Vector2(1, 0), // TODO
            };

            vertices[3] = new RoadVertex
            {
                Position = endPosition + centerToEdgeDirection * halfWidth,
                Normal = Vector3.UnitZ,
                UV = new Vector2(1, 0.324f), // TODO
            };

            _boundingBox = BoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            _vertexBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                vertices,
                BufferUsage.VertexBuffer));

            var indices = new ushort[]
            {
                0,
                1,
                2,
                1,
                2,
                3
            };

            _numIndices = (uint) indices.Length;

            _indexBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                indices,
                BufferUsage.IndexBuffer));

            _material = AddDisposable(new RoadMaterial(
                contentManager,
                contentManager.EffectLibrary.Road));

            var texture = contentManager.Load<Texture>(Path.Combine("Art", "Textures", template.Texture));
            _material.SetTexture(texture);
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
