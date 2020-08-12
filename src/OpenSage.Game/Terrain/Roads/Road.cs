using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Gui;
using OpenSage.Gui.DebugUI;
using OpenSage.Mathematics;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Terrain.Roads
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

        private readonly BeforeRenderDelegate _beforeRender;

#if DEBUG
        private readonly List<(Vector3 start, Vector3 end)> _debugLines;
#endif

        internal Road(
            AssetLoadContext loadContext,
            HeightMap heightMap,
            RoadNetwork network,
            ResourceSet radiusCursorDecalsResourceSet)
        {
            var vertices = new List<RoadShaderResources.RoadVertex>();
            var indices = new List<ushort>();

            foreach (var segment in network.Segments.OrderBy(s => s.Type))
            {
                var mesher = segment.CreateMesher(network.Template);
                mesher.GenerateMesh(heightMap, vertices, indices);
            }

            _boundingBox = BoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            _vertexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                vertices.ToArray(),
                BufferUsage.VertexBuffer));

            _numIndices = (uint) indices.Count;

            _indexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                indices.ToArray(),
                BufferUsage.IndexBuffer));
            
            _shaderSet = loadContext.ShaderResources.Road.ShaderSet;
            _pipeline = loadContext.ShaderResources.Road.Pipeline;

            // TODO: Cache these resource sets in some sort of scoped data context.
            _resourceSet = AddDisposable(loadContext.ShaderResources.Road.CreateMaterialResourceSet(network.Template.Texture.Value));

            _beforeRender = (cl, context) =>
            {
                cl.SetGraphicsResourceSet(4, _resourceSet);
                cl.SetGraphicsResourceSet(5, radiusCursorDecalsResourceSet);
                cl.SetVertexBuffer(0, _vertexBuffer);
            };

#if DEBUG
            _debugLines = new List<(Vector3 start, Vector3 end)>();
            for (int i = 0; i < _numIndices; i += 3)
            {
                var a = vertices[indices[i]].Position;
                var b = vertices[indices[i + 1]].Position;
                var c = vertices[indices[i + 2]].Position;

                _debugLines.Add((a, b));
                _debugLines.Add((b, c));
                _debugLines.Add((c, a));
            }
#endif
        }

        internal void BuildRenderList(RenderList renderList)
        {
            renderList.Road.RenderItems.Add(new RenderItem(
                _shaderSet,
                _pipeline,
                _boundingBox,
                Matrix4x4.Identity,
                0,
                _numIndices,
                _indexBuffer,
                _beforeRender));
        }

        internal void DebugDraw(DrawingContext2D context, Camera camera)
        {            
#if DEBUG            
            var strokeColor = new ColorRgbaF(180, 176, 165, 255);

            foreach (var line in _debugLines)
            {
                DebugDrawingUtils.DrawLine(context, camera, line.start, line.end, strokeColor);
            }
#endif
        }
    }
}
