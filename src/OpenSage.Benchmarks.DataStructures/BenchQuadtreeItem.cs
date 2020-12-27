using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.DataStructures;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Benchmarks.DataStructures
{
    public class BenchQuadtreeItem : ICollidable
    {
        public readonly int Id;

        public Collider RoughCollider { get; set; }

        public List<Collider> Colliders { get; set; }

        public Vector3 Translation => throw new NotImplementedException();

        public BenchQuadtreeItem(int id, RectangleF bounds)
        {
            Id = id;
            var collider = new BoxCollider(bounds);
            RoughCollider = collider;
            Colliders = new List<Collider> { collider };
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

        public bool CollidesWith(ICollidable other, bool twoDimensional)
        {
            throw new NotImplementedException();
        }
    }
}
