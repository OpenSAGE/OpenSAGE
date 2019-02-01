using System;
using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public struct ItemTransform : ICloneable
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

        public static ItemTransform operator *(in ItemTransform a, in ItemTransform b)
        {
            return new ItemTransform(a.ColorTransform * b.ColorTransform,
                                     a.GeometryRotation * b.GeometryRotation,
                                     a.GeometryTranslation + b.GeometryTranslation);
        }

        public ItemTransform WithColorTransform(in ColorRgbaF color)
        {
            return new ItemTransform(color,
                         GeometryRotation,
                         GeometryTranslation);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public abstract class DisplayItem
    {
        public AptContext Context { get; protected set; }
        public SpriteItem Parent { get; protected set; }
        public Character Character { get; protected set; }
        public ItemTransform Transform { get; set; }
        public ObjectContext ScriptObject { get; protected set; }
        public string Name { get; set; }
        public bool Visible { get; set; }

        /// <summary>
        /// Create a new DisplayItem
        /// </summary>
        /// <param name="character"></param>
        /// The template character that is used for this Item
        /// <param name="context"></param>
        /// Contains information about the AptFile where this is part of
        /// <param name="parent"></param>
        /// The parent displayItem (which must be a SpriteItem)
        public abstract void Create(Character character, AptContext context, SpriteItem parent = null);

        public virtual void Update(TimeInterval gt) { }

        public virtual void Render(AptRenderer renderer, ItemTransform pTransform, DrawingContext2D dc) { }

        public virtual void RunActions(TimeInterval gt) { }
    }
}
