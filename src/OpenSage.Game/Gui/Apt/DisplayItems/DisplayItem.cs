using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenSage.Graphics;
using OpenSage.Gui.Apt.Script;
using OpenSage.Mathematics;
using OpenAS2.Base;
using OpenSage.FileFormats.Apt;

namespace OpenSage.Gui.Apt
{
    public struct ItemTransform : ICloneable
    {
        public static readonly ItemTransform None = new ItemTransform(ColorRgbaF.White, Matrix3x2.Identity);

        public ColorRgbaF ColorTransform;
        public Matrix3x2 GeometryTransform; // should be right product
        public Matrix2x2 GeometryRotationScale // TODO determine scale first or rotation first
        {
            get { return new Matrix2x2(GeometryTransform.M11, GeometryTransform.M12, GeometryTransform.M21, GeometryTransform.M22); }
            set { GeometryTransform.M11 = value.M11; GeometryTransform.M12 = value.M12; GeometryTransform.M21 = value.M21; GeometryTransform.M22 = value.M22; }
        }
        public Vector2 GeometryTranslation { get { return GeometryTransform.Translation; } set { GeometryTransform.Translation = value; } }

        public ItemTransform(in ColorRgbaF color, in Matrix3x2 transform)
        {
            ColorTransform = color;
            GeometryTransform = transform;
        }

        public ItemTransform(in ColorRgbaF color, in Matrix2x2 rotAndScale, in Vector2 translation)
        {
            ColorTransform = color;
            GeometryTransform = Matrix3x2.Identity; 
            GeometryRotationScale = rotAndScale;
            GeometryTranslation = translation;
        }

        public static ItemTransform operator *(in ItemTransform a, in ItemTransform b)
        {
            return new ItemTransform(a.ColorTransform * b.ColorTransform,
                                     a.GeometryTransform * b.GeometryTransform);
        }

        public void Scale(float x, float y)
        {
            GeometryTransform = Matrix3x2.Multiply(Matrix3x2.CreateScale(x, y), GeometryTransform);
        }

        public ItemTransform WithColorTransform(in ColorRgbaF color)
        {
            return new ItemTransform(color,
                         GeometryTransform);
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public struct ItemShape : ICloneable
    {
        public Vector2 TopLeft;
        public Vector2 BottomRight;

        public ItemShape(float top, float left, float bottom, float right)
        {
            TopLeft = new Vector2(left, top);
            BottomRight = new Vector2(right, bottom);
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
        public Dictionary<ClipEventFlags, List<(byte, InstructionStorage)>> ClipEvents { get; protected set; } = new ();
        public AptContext ClipEventDefinedContext { get; set; }
        public List<ConstantEntry> Constants => Context.AptFile.Constants.Entries;
        public ItemTransform Transform { get; set; } = ItemTransform.None;
        public StageObject ScriptObject { get; protected set; }
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
        public abstract void CreateFrom(Character character, AptContext context, SpriteItem parent = null);

        public virtual void Update(TimeInterval gt) { }

        public void RegisterClipEvents(List<ClipEvent> ce)
        {
            ClipEvents = new();
            foreach (var ev in ce) {
                foreach (ClipEventFlags f in Enum.GetValues(typeof(ClipEventFlags)).Cast<Enum>().Where(ev.Flags.HasFlag))
                {
                    if (!ClipEvents.TryGetValue(f, out var lst))
                    {
                        ClipEvents[f] = new();
                        ClipEvents.TryGetValue(f, out lst);
                    }
                    lst.Add((ev.KeyCode, ev.Instructions));
                }
            }
        }

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

        public virtual bool CallClipEventLocal(ClipEventFlags flags)
        {
            if (ClipEvents == null)
                return false;
            foreach (ClipEventFlags f in Enum.GetValues(typeof(ClipEventFlags)).Cast<Enum>().Where(flags.HasFlag))
            { // TODO KeyCode special jdudge
                if (ClipEvents.TryGetValue(f, out var lst))
                    foreach (var (cid, insts) in lst)
                    {
                        Context.VM.EnqueueContext(insts, ClipEventDefinedContext, ScriptObject, Name + "." + flags.ToString());
                    }
                        
                return true;
            }

            return false;
        }

        public virtual void CallClipEvent(ClipEventFlags flags)
        {
            var thisobj = this;
            while (thisobj != null)
            {
                var call_flag = thisobj.CallClipEventLocal(flags);
                if (call_flag) return;
                thisobj = thisobj.Parent;
            }
        }

        public virtual void EnqueueActions(TimeInterval gt) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mousePos"> the mouse position for the local 2d homogeneous coordinate.</param>
        /// <returns></returns>
        public abstract DisplayItem GetMouseFocus(Vector2 mousePos);

        public virtual bool HandleInput(Point2D mousePos, bool mouseDown) { return false; }

        public abstract bool HandleEvent(ClipEventFlags flags); // { return false; }
    }
}
