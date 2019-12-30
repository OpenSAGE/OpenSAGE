using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Content.Loaders;
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
        private DeviceBuffer _vertexBuffer;
        private BoundingBox _boundingBox;

        private DeviceBuffer _indexBuffer;
        private uint _numIndices;

        private readonly ShaderSet _shaderSet;
        private readonly Pipeline _pipeline;

        private readonly BeforeRenderDelegate _beforeRender;
        private Matrix4x4 _world;

        internal static bool TryCreate(
            AssetLoadContext loadContext,
            PolygonTrigger trigger,
            out WaterArea result)
        {
            if (trigger.Points.Length < 3)
            {
                // Some maps (such as Training01) have water areas with fewer than 3 points.
                result = null;
                return false;
            }

            result = new WaterArea(loadContext, trigger);
            return true;
        }

        internal static bool TryCreate(
            AssetLoadContext loadContext,
            StandingWaterArea area,
            out WaterArea result)
        {
            if (area.Points.Length < 3)
            {
                // Some maps (such as Training01) have water areas with fewer than 3 points.
                result = null;
                return false;
            }

            result = new WaterArea(loadContext, area);
            return true;
        }

        internal static bool TryCreate(
            AssetLoadContext loadContext,
            StandingWaveArea area,
            out WaterArea result)
        {
            if (area.Points.Length < 3)
            {
                // Some maps (such as Training01) have water areas with fewer than 3 points.
                result = null;
                return false;
            }

            result = new WaterArea(loadContext, area);
            return true;
        }

        private void CreateGeometry(AssetLoadContext loadContext,
                                Vector2[] points, uint height)
        {
            Triangulator.Triangulate(
                points,
                WindingOrder.CounterClockwise,
                out var trianglePoints,
                out var triangleIndices);

            var vertices = trianglePoints
                .Select(x =>
                    new WaterShaderResources.WaterVertex
                    {
                        Position = new Vector3(x.X, x.Y, height)
                    })
                .ToArray();

            _boundingBox = BoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            _vertexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                vertices,
                BufferUsage.VertexBuffer));

            _numIndices = (uint)triangleIndices.Length;

            _indexBuffer = AddDisposable(loadContext.GraphicsDevice.CreateStaticBuffer(
                triangleIndices,
                BufferUsage.IndexBuffer));

            _world = Matrix4x4.Identity;
            _world.Translation = new Vector3(0, height, 0);

        }

        private WaterArea(AssetLoadContext loadContext)
        {
            _shaderSet = loadContext.ShaderResources.Water.ShaderSet;
            _pipeline = loadContext.ShaderResources.Water.Pipeline;

            _beforeRender = (cl, context) =>
            {
                cl.SetVertexBuffer(0, _vertexBuffer);
            };
        }

        private WaterArea(
            AssetLoadContext loadContext,
            StandingWaveArea area) : this(loadContext)
        {
            CreateGeometry(loadContext, area.Points, area.FinalHeight);
            //TODO: add waves
        }

        private WaterArea(
            AssetLoadContext loadContext,
            StandingWaterArea area) : this(loadContext)
        {
            CreateGeometry(loadContext, area.Points, area.WaterHeight);
            //TODO: use depthcolors
        }

        private WaterArea(
            AssetLoadContext loadContext,
            PolygonTrigger trigger) : this(loadContext)
        {
            var triggerPoints = trigger.Points
                .Select(x => new Vector2(x.X, x.Y))
                .ToArray();

            CreateGeometry(loadContext, triggerPoints, (uint)trigger.Points[0].Z);
        }

        internal void BuildRenderList(RenderList renderList)
        {
            renderList.Water.RenderItems.Add(new RenderItem(
                _shaderSet,
                _pipeline,
                _boundingBox,
                _world,
                0,
                _numIndices,
                _indexBuffer,
                _beforeRender));
        }
    }
}
