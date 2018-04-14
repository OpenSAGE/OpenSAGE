using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class ControlBarCallbacks
    {
        public static void W3DCommandBarBackgroundDraw(Control control, DrawingContext2D drawingContext)
        {

        }

        public static void ControlBarSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "ControlBar.wnd:ButtonLarge":
                            ((GeneralsControlBar) context.Game.Scene2D.ControlBar).ToggleSize();
                            break;
                        case "ControlBar.wnd:ButtonOptions":
                            context.WindowManager.PushWindow("Menus/QuitMenu.wnd");
                            break;
                    }
                    break;
            }
        }
    }
}
