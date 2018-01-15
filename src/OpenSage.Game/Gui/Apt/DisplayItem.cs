using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Gui.Apt
{
    public struct ItemTransform
    {
        public static readonly ItemTransform None = new ItemTransform(ColorRgbaF.White, Matrix3x2.Identity);

        public ColorRgbaF ColorTransform;
        public Matrix3x2 GeometryTransform;

        public ItemTransform(ColorRgbaF color, Matrix3x2 geometry)
        {
            ColorTransform = color;
            GeometryTransform = geometry;
        }

        public static ItemTransform operator *(ItemTransform a, ItemTransform b)
        {
            return new ItemTransform(a.ColorTransform.BlendMultiply(b.ColorTransform),
                                     a.GeometryTransform * b.GeometryTransform);
        }
    }

    public interface IDisplayItem
    {
        AptContext Context { get; }
        SpriteItem Parent { get; }
        //the underlying structure that will be used
        Character Character { get; }
        ItemTransform Transform { get; set; }

        void Create(Character chararacter, AptContext context, SpriteItem parent = null);
        void Update(ItemTransform pTransform, GameTime gt, DrawingContext dc);
    }
}
