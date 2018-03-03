using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class SkirmishMapSelectMenuCallbacks
    {
        public static void SkirmishMapSelectMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishMapSelectMenu.wnd:ButtonBack":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }
    }
}
