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
            var x = (float)(bounds.X + random.NextDouble() * bounds.Width);
            var y = (float)(bounds.Y + random.NextDouble() * bounds.Height);
            var width = (float)(random.NextDouble() * Math.Min(bounds.Width - x, maxSize.Width));
            var height = (float)(random.NextDouble() * Math.Min(bounds.Height - y, maxSize.Height));

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
