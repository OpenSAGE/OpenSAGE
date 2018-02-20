using OpenSage.Content;
using OpenSage.Data.Wnd;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndWindowStaticText : WndWindow
    {
        internal WndWindowStaticText(WndWindowDefinition wndWindow, ContentManager contentManager, WndCallbackResolver callbackResolver)
            : base(wndWindow, contentManager, callbackResolver)
        {
            if (!wndWindow.StaticTextData.Centered)
            {
                TextAlignment = TextAlignment.Leading;
            }
        }
    }
}
