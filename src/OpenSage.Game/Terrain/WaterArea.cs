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

        public WaterArea(
            ContentManager contentManager,
            PolygonTrigger trigger)
        {
            var triggerPoints = trigger.Points
                .Select(x => new Vector2(x.X, x.Y))
                .ToList();

            if (!Triangulator.Process(triggerPoints, out var trianglePoints))
            {
                throw new InvalidOperationException();
            }

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

            var indices = Enumerable
                .Range(0, vertices.Length)
                .Select(x => (ushort) x)
                .ToArray();

            _numIndices = (uint) indices.Length;

            _indexBuffer = AddDisposable(contentManager.GraphicsDevice.CreateStaticBuffer(
                indices,
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
            renderList.Opaque.AddRenderItemDrawIndexed(
                _materials[timeOfDay],
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
    internal struct WaterVertex
    {
        public Vector3 Position;

        public static readonly VertexLayoutDescription VertexDescriptor = new VertexLayoutDescription(
            new VertexElementDescription("POSITION", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float3));
    }
}
