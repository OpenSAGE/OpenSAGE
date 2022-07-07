using System;
using System.Diagnostics;
using System.Numerics;
using OpenSage.Data.Apt.Characters;
using OpenSage.Graphics;
using OpenSage.Gui.Apt.ActionScript;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Apt
{
    public struct ItemTransform : ICloneable
    {
        public static readonly ItemTransform None = new(ColorRgbaF.White, ColorRgbaF.Transparent, Matrix3x2.Identity, Vector2.Zero);

        public ColorRgbaF TintColorTransform;
        public ColorRgbaF AdditiveColorTransform;
        public Matrix3x2 GeometryRotation;
        public Vector2 GeometryTranslation;

        public ItemTransform(in ColorRgbaF tint, in ColorRgbaF add, in Matrix3x2 rotation, in Vector2 translation)
        {
            TintColorTransform = tint;
            AdditiveColorTransform = add;
            GeometryRotation = rotation;
            GeometryTranslation = translation;
        }

        public static ItemTransform operator *(in ItemTransform a, in ItemTransform b)
        {
            /*  Combining ColorTransform:
             *  f(x) = x * a + b
             *  g(x) = x * i + j
             *  g(f(x)) = (x * a + b) * i + j
             *      = x * (a * i) + (b * i + j)
             */
            var tint = a.TintColorTransform * b.TintColorTransform;
            var add = a.AdditiveColorTransform * b.TintColorTransform;
            add += b.AdditiveColorTransform;
            // FIXME: Adobe's specification claims that colors are clamped between 0 and 255
            // during every single step of transformation.
            // Probably We can't achieve that by using our current ItemTransform.
            return new(tint,
                       add,
                       a.GeometryRotation * b.GeometryRotation,
                       a.GeometryTranslation + b.GeometryTranslation);
        }

        public void Scale(float x, float y)
        {
            GeometryRotation = Matrix3x2.Multiply(Matrix3x2.CreateScale(x, y), GeometryRotation);
        }

        public ItemTransform WithColorTransform(in ColorRgbaF tint, in ColorRgbaF add)
        {
            return new(tint,
                       add,
                       GeometryRotation,
                       GeometryTranslation);
        }

        public ColorRgbaF TransformColor(in ColorRgbaF sourceColor)
        {
            return sourceColor * TintColorTransform + AdditiveColorTransform;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    [DebuggerDisplay("[DisplayItem:{Name}]")]
    public abstract class DisplayItem : DisposableBase
    {
        public AptContext Context { get; protected set; }
        public SpriteItem Parent { get; protected set; }
        public Character Character { get; protected set; }
        public ItemTransform Transform { get; set; }
        public ObjectContext ScriptObject { get; protected set; }
        public string Name { get; set; }
        public bool Visible { get; set; }
        public int? ClipDepth { get; set; }

        public bool Highlight { get; set; }

        internal RenderTarget ClipMask { get => _clipMask; set => DisposeAndAssign(ref _clipMask, value); }
        private RenderTarget _clipMask;

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

        public void Render(AptRenderingContext renderingContext)
        {
            if (ClipDepth.HasValue)
            {
                ClipMask.EnsureSize(renderingContext.WindowSize);
                renderingContext.SetRenderTarget(ClipMask);
            }

            RenderImpl(renderingContext);

            if (ClipDepth.HasValue)
            {
                renderingContext.SetRenderTarget(null);
            }
        }

        protected virtual void RenderImpl(AptRenderingContext renderingContext) { }

        public virtual void RunActions(TimeInterval gt) { }

        public virtual bool HandleInput(Point2D mousePos, bool mouseDown) { return false; }
    }
}
