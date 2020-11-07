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

        public static int Create(AptFile container, Vector4 bounds, uint geometry)
        {
            if (!container.GeometryMap.ContainsKey(geometry))
            {
                throw new ArgumentException(nameof(geometry));
            }
            var shape = new Shape
            {
                Container = container,
                Bounds = bounds,
                Geometry = geometry
            };
            var shapeIndex = container.Movie.Characters.Count;
            container.Movie.Characters.Add(shape);
            return shapeIndex;
        }
    }
}
