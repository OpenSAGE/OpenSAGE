using OpenSage.Content;
using OpenSage.Data.Wnd;
using Veldrid;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowGenericWindow : WndWindow
    {
        private readonly Texture _imageTexture;

        internal WndWindowGenericWindow(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
            : base(wndWindow, contentManager, callbackResolver)
        {
            if (wndWindow.Status.HasFlag(WndWindowStatusFlags.Image))
            {
                _imageTexture = contentManager.WndImageTextureCache.GetNormalTexture(
                    wndWindow,
                    wndWindow.EnabledDrawData,
                    0);
            }
        }

        protected override void DefaultDrawOverride(Game game)
        {
            if (_imageTexture != null)
            {
                PrimitiveBatch.DrawImage(_imageTexture, null, Bounds);
            }
        }
    }
}
