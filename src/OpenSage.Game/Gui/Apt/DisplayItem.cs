using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public struct ItemTransform
    {
        public static readonly ItemTransform None = new ItemTransform(ColorRgbaF.White, Matrix3x2.Identity, Vector2.Zero);

        public ColorRgbaF ColorTransform;
        public Matrix3x2 GeometryRotation;
        public Vector2 GeometryTranslation;

        public ItemTransform(in ColorRgbaF color, in Matrix3x2 rotation, in Vector2 translation)
        {
            ColorTransform = color;
            GeometryRotation = rotation;
            GeometryTranslation = translation;
        }

        public static ItemTransform operator *(ItemTransform a, ItemTransform b)
        {
            return new ItemTransform(a.ColorTransform * b.ColorTransform,
                                     a.GeometryRotation * b.GeometryRotation,
                                     a.GeometryTranslation + b.GeometryTranslation);
        }
    }

    public interface IDisplayItem
    {
        AptContext Context { get; }
        SpriteItem Parent { get; }
        Character Character { get; }
        ItemTransform Transform { get; set; }
        ObjectContext ScriptObject { get; }
        string Name { get; set; }
        bool Visible { get; set; }

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
        void Update(GameTime gt);
        void Render(AptRenderer renderer, ItemTransform pTransform, DrawingContext2D dc);
        void RunActions(GameTime gt);
    }
}
