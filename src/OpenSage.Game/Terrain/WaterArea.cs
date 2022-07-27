using System.Linq;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Core.Graphics;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Rendering.Water;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;
using OpenSage.Rendering;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;
using Veldrid;

namespace OpenSage.Terrain
{
    public sealed class WaterArea : DisposableBase
    {
        private readonly string _debugName;

        private DeviceBuffer _vertexBuffer;
        private AxisAlignedBoundingBox _boundingBox;

        private DeviceBuffer _indexBuffer;
        private uint _numIndices;

        private readonly WaterShaderResources _shaderSet;
        private readonly Pipeline _pipeline;

        private readonly Material _material;

        private readonly BeforeRenderDelegate _beforeRender;
        private Matrix4x4 _world;

        internal static bool TryCreate(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore,
            PolygonTrigger trigger,
            out WaterArea result)
        {
            if (trigger.Points.Length < 3)
            {
                // Some maps (such as Training01) have water areas with fewer than 3 points.
                result = null;
                return false;
            }

            result = new WaterArea(graphicsDeviceManager, shaderSetStore, trigger);
            return true;
        }

        internal static bool TryCreate(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore,
            StandingWaterArea area,
            out WaterArea result)
        {
            if (area.Points.Length < 3)
            {
                // Some maps (such as Training01) have water areas with fewer than 3 points.
                result = null;
                return false;
            }

            result = new WaterArea(graphicsDeviceManager, shaderSetStore, area);
            return true;
        }

        internal static bool TryCreate(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore,
            StandingWaveArea area,
            out WaterArea result)
        {
            if (area.Points.Length < 3)
            {
                // Some maps (such as Training01) have water areas with fewer than 3 points.
                result = null;
                return false;
            }

            result = new WaterArea(graphicsDeviceManager, shaderSetStore, area);
            return true;
        }

        private void CreateGeometry(
            GraphicsDeviceManager graphicsDeviceManager,
            Vector2[] points,
            uint height)
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

            _boundingBox = AxisAlignedBoundingBox.CreateFromPoints(vertices.Select(x => x.Position));

            _vertexBuffer = AddDisposable(graphicsDeviceManager.GraphicsDevice.CreateStaticBuffer(
                vertices,
                BufferUsage.VertexBuffer));

            _numIndices = (uint)triangleIndices.Length;

            _indexBuffer = AddDisposable(graphicsDeviceManager.GraphicsDevice.CreateStaticBuffer(
                triangleIndices,
                BufferUsage.IndexBuffer));

            _world = Matrix4x4.Identity;
            _world.Translation = new Vector3(0, height, 0);
        }

        private WaterArea(
            ShaderSetStore shaderSetStore,
            string debugName)
        {
            _shaderSet = shaderSetStore.GetWaterShaderResources();
            _pipeline = _shaderSet.Pipeline;

            _material = AddDisposable(
                new Material(
                    _shaderSet,
                    _pipeline,
                    null,
                    SurfaceType.Transparent)); // TODO: MaterialResourceSet

            _debugName = debugName;

            _beforeRender = (CommandList cl, in RenderItem renderItem) =>
            {
                cl.SetVertexBuffer(0, _vertexBuffer);
            };
        }

        private WaterArea(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore,
            StandingWaveArea area) : this(shaderSetStore, area.Name)
        {
            CreateGeometry(graphicsDeviceManager, area.Points, area.FinalHeight);
            //TODO: add waves
        }

        private WaterArea(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore,
            StandingWaterArea area) : this(shaderSetStore, area.Name)
        {
            CreateGeometry(graphicsDeviceManager, area.Points, area.WaterHeight);
            // TODO: use depthcolors
            // TODO: use FXShader?
        }

        private WaterArea(
            GraphicsDeviceManager graphicsDeviceManager,
            ShaderSetStore shaderSetStore,
            PolygonTrigger trigger) : this(shaderSetStore, trigger.Name)
        {
            var triggerPoints = trigger.Points
                .Select(x => new Vector2(x.X, x.Y))
                .ToArray();

            CreateGeometry(graphicsDeviceManager, triggerPoints, (uint)trigger.Points[0].Z);
        }

        internal void BuildRenderList(RenderList renderList)
        {
            renderList.Water.RenderItems.Add(new RenderItem(
                _debugName,
                _material,
                _boundingBox,
                _world,
                0,
                _numIndices,
                _indexBuffer,
                _beforeRender));
        }
    }
}
