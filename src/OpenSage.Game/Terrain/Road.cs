using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
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

        private readonly ShaderSet _shaderSet;
        private readonly Pipeline _pipeline;
        private readonly ResourceSet _resourceSet;

        internal Road(
            ContentManager contentManager,
            HeightMap heightMap,
            RoadTemplate template,
            in Vector3 startPosition,
            in Vector3 endPosition)
        {
            const float heightBias = 1f;
            const float createNewVerticesHeightDeltaThreshold = 0.002f;

            var distance = Vector3.Distance(startPosition, endPosition);
            var direction = Vector3.Normalize(endPosition - startPosition);
            var centerToEdgeDirection = Vector3.Cross(Vector3.UnitZ, direction);
            var up = Vector3.Cross(direction, centerToEdgeDirection);

            var halfWidth = template.RoadWidth / 2;

            var textureAtlasSplit = 1 / 3f;

            var vertices = new List<RoadTypes.RoadVertex>();

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

                vertices.Add(new RoadTypes.RoadVertex
                {
                    Position = p0,
                    Normal = up,
                    UV = new Vector2(u, 0)
                });

                var p1 = position + centerToEdgeDirection * halfWidth;
                p1.Z += heightBias;

                vertices.Add(new RoadTypes.RoadVertex
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

            _shaderSet = contentManager.ShaderLibrary.Road;
            _pipeline = contentManager.RoadResourceCache.Pipeline;

            var texture = contentManager.Load<Texture>(Path.Combine("Art", "Textures", template.Texture));
            _resourceSet = contentManager.RoadResourceCache.GetResourceSet(texture);
        }

        internal void BuildRenderList(RenderList renderList)
        {
            renderList.Opaque.RenderItems.Add(new RenderItem(
                _shaderSet,
                _pipeline,
                _boundingBox,
                Matrix4x4.Identity,
                0,
                _numIndices,
                _indexBuffer,
                cl =>
                {
                    cl.SetGraphicsResourceSet(4, _resourceSet);
                    cl.SetVertexBuffer(0, _vertexBuffer);
                }));
        }
    }
}
