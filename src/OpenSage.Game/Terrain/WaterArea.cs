using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenSage.Content;
using OpenSage.Data.Map;
using OpenSage.Graphics.Rendering;
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

        private readonly Dictionary<TimeOfDay, WaterMaterial> _materials;

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
                    new WaterVertex
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

            _materials = new Dictionary<TimeOfDay, WaterMaterial>();

            foreach (var waterSet in contentManager.IniDataContext.WaterSets)
            {
                var material = AddDisposable(new WaterMaterial(
                    contentManager,
                    contentManager.EffectLibrary.Water));
                
                var waterTexture = contentManager.Load<Texture>(Path.Combine("Art", "Textures", waterSet.WaterTexture));
                material.SetWaterTexture(waterTexture);

                _materials.Add(waterSet.TimeOfDay, material);
            }
        }

        internal void BuildRenderList(RenderList renderList, TimeOfDay timeOfDay)
        {
            renderList.Opaque.RenderItems.Add(new RenderItem(
                _materials[timeOfDay],
                _vertexBuffer,
                null,
                CullFlags.None,
                _boundingBox,
                Matrix4x4.Identity,
                0,
                _numIndices,
                _indexBuffer));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WaterVertex
    {
        public Vector3 Position;

        public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
            new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
    }
}
