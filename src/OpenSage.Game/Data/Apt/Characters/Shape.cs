using System;
using System.IO;
using System.Numerics;
using MoonSharp.Interpreter.Execution.VM;
using OpenSage.FileFormats;

namespace OpenSage.Data.Apt.Characters
{
    public sealed class Shape : Character
    {
        public Vector4 Bounds { get; private set; }
        public uint Geometry { get; private set; }

        public static Shape Parse(BinaryReader reader)
        {
            var shape = new Shape();
            shape.Bounds = reader.ReadVector4();
            shape.Geometry = reader.ReadUInt32();
            return shape;
        }

        public static int Create(AptFile container, uint geometryId)
        {
            if (!container.GeometryMap.TryGetValue(geometryId, out var geometry))
            {
                throw new ArgumentException(null, nameof(geometryId));
            }
            var box = geometry.BoundingBox;
            var bounds = new Vector4
            {
                X = box.Left,
                Y = box.Top,
                Z = box.Right,
                W = box.Bottom
            };
            var characters = container.Movie.Characters;
            var shapeIndex = characters.Count;
            characters.Add(new Shape
            {
                Container = container,
                Bounds = bounds,
                Geometry = geometryId
            });
            return shapeIndex;
        }

        public void Modify(uint newGeometryId, bool modifyBounds = false, Vector4? newBounds = null)
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
                    newBounds = new Vector4
                    {
                        X = box.Left,
                        Y = box.Top,
                        Z = box.Right,
                        W = box.Bottom
                    };
                }
                Bounds = newBounds.Value;
            }
            Geometry = newGeometryId;
        }
    }
}
