using System.Numerics;
using OpenSage.Content.Translation;
using OpenSage.FileFormats.Apt;
using OpenSage.FileFormats.Apt.Characters;
using OpenSage.FileFormats.Apt.FrameItems;
using OpenAS2.FlashDom.Script;
using OpenAS2.FlashDom.Script;
using Veldrid;

namespace OpenAS2.FlashDom.HostObjects
{
    public sealed class RenderItem : TexturedItem
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private TimeInterval _lastUpdate;

        public LocalizedString TextValue { get; private set; }
        private bool IsHovered { get; set; }

        public delegate void CustomRenderCallback(AptRenderingContext context, Geometry geometry, Texture originalTexture);
        public CustomRenderCallback RenderCallback;

        public override void Create(Character character, AptContext context, SpriteItem parent = null)
        {
            Character = character;
            Context = context;
            Parent = parent;
            ScriptObject = character switch
            {
                Text _ => new TextField(this),
                _ => new StageObject(this),
            };
            Name = "";
            Visible = true;
            TextValue = character is Text text ? LocalizedString.CreateApt(text.Content) : null;
            IsHovered = false;
        }

        public override void Update(TimeInterval gt)
        {
            // Currently only Text needs to be updated
            if (Character is not Text t)
            {
                return;
            }

            if ((gt.TotalTime - _lastUpdate.TotalTime).TotalMilliseconds < Context.MsPerFrame)
            {
                return;
            }

            _lastUpdate = gt;
            if (t.Value.Length > 0)
            {
                string textValue = null;
                try
                {
                    var val = ScriptObject.ResolveValue(t.Value, ScriptObject);
                    if (val.Type != ValueType.Undefined)
                    {
                        textValue = val.ToString();
                    }
                }
                catch (System.Exception e)
                {
                    Logger.Warn($"Failed to resolve text value: {e}");
                }

                if (TextValue.Original != textValue)
                {
                    TextValue = LocalizedString.CreateApt(textValue);
                }
            }
        }

        public override DisplayItem GetMouseFocus(Vector2 mousePos)
        {
            var ret = false;
            if (Character is Text t)
                ret = t.Bounds.Contains(mousePos);
            else if (Character is Shape s)
                ret = s.Geometry == null ?
                    s.Bounds.Contains(mousePos) :
                    s.Geometry.Contains(mousePos);
                // TODO The geometry is not loaded by default, this will cause some problems
                // in some very special cases (like 2 circle in 2 sides of the screen while the
                // mouse points the center). Better try to load it when the context is created.
            return ret ? this : null;
        }

        public override bool HandleEvent(ClipEventFlags flags)
        {
            CallClipEvent(flags);
            return true;
        }

        protected override void RenderImpl(AptRenderingContext renderingContext)
        {
            if (!Visible)
            {
                return;
            }

            renderingContext.PushTransform(Transform);

            switch (Character)
            {
                case Shape s:
                    var geometry = Context.GetGeometry(s.GeometryId, Character);
                    if (RenderCallback != null)
                    {
                        RenderCallback(renderingContext, geometry, Texture);
                    }
                    else
                    {
                        renderingContext.RenderGeometry(geometry, Texture);
                    }

                    if (Highlight)
                    {
                        renderingContext.RenderOutline(geometry);
                    }
                    break;

                case Text t:
                    renderingContext.RenderText(t, TextValue.Localized);
                    break;
            }

            renderingContext.PopTransform();
        }
    }
}
