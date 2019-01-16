using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class WaterArea : DisposableBase
    {
        private readonly DeviceBuffer _vertexBuffer;
        private readonly BoundingBox _boundingBox;

        private readonly DeviceBuffer _indexBuffer;
        private readonly uint _numIndices;

        private readonly ShaderSet _shaderSet;
        private readonly Pipeline _pipeline;
        private readonly Dictionary<TimeOfDay, ResourceSet> _resourceSets;

        public static bool TryCreate(
            ContentManager contentManager,
            PolygonTrigger trigger,
            out WaterArea result)
        {
            if (trigger.Points.Length < 3)
            {
                // Some maps (such as Training01) have water areas with fewer than 3 points.
                result = null;
                return false;
            }

            result = new WaterArea(contentManager, trigger);
            return true;
        }

        private WaterArea(
            ContentManager contentManager,
            PolygonTrigger trigger)
        {
            var triggerPoints = trigger.Points
                .Select(x => new Vector2(x.X, x.Y))
                .ToArray();

            Triangulator.Triangulate(
                triggerPoints,
                WindingOrder.CounterClockwise,
                out var trianglePoints,
                out var triangleIndices);

            var vertices = trianglePoints
                .Select(x =>
                    new WaterTypes.WaterVertex
                    {
                        Position = new Vector3(x.X, x.Y, trigger.Points[0].Z)
                    })
                .ToArray();

            _boundingBox = BoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            _vertexBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                vertices,
                BufferUsage.VertexBuffer));

            _numIndices = (uint) triangleIndices.Length;

            _indexBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                triangleIndices,
                BufferUsage.IndexBuffer));

            _shaderSet = contentManager.ShaderLibrary.Water;
            _pipeline = contentManager.WaterResourceCache.Pipeline;

            _resourceSets = new Dictionary<TimeOfDay, ResourceSet>();

            foreach (var waterSet in contentManager.IniDataContext.WaterSets)
            {
                var waterTexture = contentManager.Load<Texture>(Path.Combine("Art", "Textures", waterSet.WaterTexture));
                var resourceSet = contentManager.WaterResourceCache.GetResourceSet(waterTexture);

                _resourceSets.Add(waterSet.TimeOfDay, resourceSet);
            }
        }

        internal void BuildRenderList(RenderList renderList, TimeOfDay timeOfDay)
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
                    cl.SetGraphicsResourceSet(4, _resourceSets[timeOfDay]);
                    cl.SetVertexBuffer(0, _vertexBuffer);
                }));
        }
    }
}
