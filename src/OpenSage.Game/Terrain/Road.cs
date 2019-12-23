using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Terrain.Roads;
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

        private readonly BeforeRenderDelegate _beforeRender;

        internal Road(
            AssetLoadContext loadContext,
            HeightMap heightMap,
            RoadNetwork network)
        {
            var vertices = new List<RoadShaderResources.RoadVertex>();
            var indices = new List<ushort>();

            foreach (var segment in network.Segments)
            {
                segment.GenerateMesh(network.Template, heightMap, vertices, indices);
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
                cl.SetVertexBuffer(0, _vertexBuffer);
            };
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
                _beforeRender));
        }
    }
}
