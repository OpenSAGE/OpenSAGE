using System;
using OpenSage.DataStructures;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class BenchQuadtreeItem : ICollidable
    {
        public readonly int Id;
        public Collider Collider { get; set; }

        public BenchQuadtreeItem(int id, RectangleF bounds)
        {
            Id = id;
            Collider = new BoxCollider(bounds);
        }

        public static BenchQuadtreeItem Generate(int id, RectangleF bounds, SizeF maxSize, Random random)
        {
            var x = (float) random.NextDouble() * bounds.Width - bounds.Left;
            var y = (float) random.NextDouble() * bounds.Height - bounds.Top;

            var width = MathF.Min(bounds.Right - x, (float) (random.NextDouble() * maxSize.Width));
            var height = MathF.Min(bounds.Bottom - y, (float) (random.NextDouble() * maxSize.Height));

            return new BenchQuadtreeItem(id, new RectangleF(x, y, width, height));
        }

        public BenchQuadtreeItem Clone()
        {
            return (BenchQuadtreeItem) MemberwiseClone();
        }
    }
}
