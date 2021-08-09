using System;
using System.IO;
using System.Numerics;
using OpenSage.FileFormats;
using OpenSage.Mathematics;

namespace OpenSage.FileFormats.Apt.Characters
{
    public sealed class Shape : Character
    {
        public RectangleF Bounds { get; private set; }
        public uint GeometryId { get; private set; }
        public Geometry Geometry { get; private set; }

        public static Shape Parse(BinaryReader reader)
        {
            var shape = new Shape();
            var bounds = reader.ReadVector4();
            shape.Bounds = new RectangleF(bounds.X, bounds.Y, bounds.Z - bounds.X, bounds.W - bounds.Y);
            shape.GeometryId = reader.ReadUInt32();
            return shape;
        }

        public static Shape Create(AptFile container, uint geometryId)
        {
            if (!container.GeometryMap.TryGetValue(geometryId, out var geometry))
            {
                throw new ArgumentException(null, nameof(geometryId));
            }
            var box = geometry.BoundingBox;
            var bounds = box;
            return new Shape
            {
                Container = container,
                Bounds = bounds,
                GeometryId = geometryId,
                Geometry = geometry,
            };
        }

        public void Modify(uint newGeometryId, bool modifyBounds = false, RectangleF? newBounds = null)
        {
            if (!Container.GeometryMap.TryGetValue(newGeometryId, out var geometry))
            {
                throw new ArgumentException(null, nameof(newGeometryId));
            }
            if (modifyBounds)
            {
                if (!newBounds.HasValue)
                {
                    var box = geometry.BoundingBox;
                    newBounds = box;
                }
                Bounds = newBounds.Value;
            }
            GeometryId = newGeometryId;
            Geometry = geometry;
        }
    }
}
