using System;
using System.Collections.Generic;
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
            HeightMap heightMap,
            RoadTemplate template,
            Vector3 startPosition,
            Vector3 endPosition)
        {
            const float heightBias = 1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;

            var distance = Vector3.Distance(startPosition, endPosition);
            var direction = Vector3.Normalize(endPosition - startPosition);
            var centerToEdgeDirection = Vector3.Cross(Vector3.UnitZ, direction);
            var up = Vector3.Cross(direction, centerToEdgeDirection);

            var halfWidth = template.RoadWidth / 2;

            var textureAtlasSplit = 1 / 3f;

            var vertices = new List<RoadVertex>();

            // Step along road segment in units of 10. If the delta between
            // (a) the straight line from previous point to finish and
            // (b) the actual height of the terrain at this point
            // is > a threshold, create extra vertices.
            // TODO: I don't know if this is the right algorithm.

            void AddVertexPair(in Vector3 position, float distanceAlongRoad)
            {
                var u = distanceAlongRoad / 50;

                var p0 = position - centerToEdgeDirection * halfWidth;
                p0.Z += heightBias;

                vertices.Add(new RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, 0)
                });

                var p1 = position + centerToEdgeDirection * halfWidth;
                p1.Z += heightBias;

                vertices.Add(new RoadVertex
                {
                    Position = p1,
                    Normal = up,
                    UV = new Vector2(u, textureAtlasSplit)
                });
            }

            AddVertexPair(startPosition, 0);

            var previousPoint = startPosition;
            var previousPointDistance = 0;

            for (var currentDistance = 10; currentDistance < distance; currentDistance += 10)
            {
                var position = startPosition + direction * currentDistance;
                var actualHeight = heightMap.GetHeight(position.X, position.Y);
                var interpolatedHeight = MathUtility.Lerp(previousPoint.Z, endPosition.Z, (currentDistance - previousPointDistance) / distance);

                if (Math.Abs(actualHeight - interpolatedHeight) > createNewVerticesHeightDeltaThreshold)
                {
                    AddVertexPair(position, currentDistance);
                    previousPoint = position;
                    previousPointDistance = currentDistance;
                }
            }

            // Add last chunk.
            AddVertexPair(endPosition, distance);

            _boundingBox = BoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            _vertexBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                vertices.ToArray(),
                BufferUsage.VertexBuffer));

            var indices = new List<ushort>();

            for (var i = 0; i < vertices.Count - 2; i += 2)
            {
                indices.Add((ushort) (i + 0));
                indices.Add((ushort) (i + 1));
                indices.Add((ushort) (i + 2));

                indices.Add((ushort) (i + 1));
                indices.Add((ushort) (i + 2));
                indices.Add((ushort) (i + 3));
            }

            _numIndices = (uint) indices.Count;

            _indexBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                indices.ToArray(),
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
