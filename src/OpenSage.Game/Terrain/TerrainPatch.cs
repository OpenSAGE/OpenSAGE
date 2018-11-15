using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Graphics.Rendering;
using OpenSage.Mathematics;
using Veldrid;
using Rectangle = OpenSage.Mathematics.Rectangle;

namespace OpenSage.Terrain
{
    public sealed class TerrainPatch
    {
        private readonly DeviceBuffer _vertexBuffer;
        private readonly DeviceBuffer _indexBuffer;
        private readonly uint _numIndices;

        private readonly TerrainMaterial _terrainMaterial;

        public Rectangle Bounds { get; }

        public BoundingBox BoundingBox { get; }

        public Triangle[] Triangles { get; }

        internal TerrainPatch(
            TerrainMaterial terrainMaterial,
            Rectangle patchBounds,
            DeviceBuffer vertexBuffer,
            DeviceBuffer indexBuffer,
            uint numIndices,
            Triangle[] triangles,
            BoundingBox boundingBox)
        {
            _terrainMaterial = terrainMaterial;

            Bounds = patchBounds;

            _vertexBuffer = vertexBuffer;
            _indexBuffer = indexBuffer;
            _numIndices = numIndices;

            BoundingBox = boundingBox;
            Triangles = triangles;
        }

        internal void Intersect(
            Ray ray,
            ref float? closestIntersection)
        {
            if (!ray.Intersects(BoundingBox, out var _))
            {
                return;
            }

            for (var i = 0; i < Triangles.Length; i++)
            {
                if (ray.Intersects(Triangles[i], out var intersection))
                {
                    if (closestIntersection != null)
                    {
                        if (intersection < closestIntersection)
                        {
                            closestIntersection = intersection;
                        }
                    }
                    else
                    {
                        closestIntersection = intersection;
                    }
                }
            }
        }

        internal void BuildRenderList(RenderList renderList, Texture macroTexture)
        {
            _terrainMaterial.SetMacroTexture(macroTexture);

            renderList.Opaque.AddRenderItemDrawIndexed(
                _terrainMaterial,
                _vertexBuffer,
                null,
                CullFlags.None,
                BoundingBox,
                Matrix4x4.Identity,
                0,
                _numIndices,
                _indexBuffer);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TerrainVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;

        public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
            new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("NORMAL", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3),
            new VertexElementDescription("TEXCOORD", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2));
    }
}
