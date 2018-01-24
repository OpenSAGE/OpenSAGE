using System.Numerics;
using OpenSage.Data.Apt.Characters;
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
            return new ItemTransform(a.ColorTransform * b.ColorTransform,
                                     a.GeometryTransform * b.GeometryTransform);
        }
    }

    public interface IDisplayItem
    {
        AptContext Context { get; }
        SpriteItem Parent { get; }
        Character Character { get; }
        ItemTransform Transform { get; set; }

        /// <summary>
        /// Create a new DisplayItem
        /// </summary>
        /// <param name="chararacter"></param>
        /// The template character that is used for this Item
        /// <param name="context"></param>
        /// Contains information about the AptFile where this is part of
        /// <param name="parent"></param>
        /// The parent displayitem (which must be a SpriteItem)
        void Create(Character chararacter, AptContext context, SpriteItem parent = null);
        void Update(ItemTransform pTransform, GameTime gt, DrawingContext2D dc);
    }
}
