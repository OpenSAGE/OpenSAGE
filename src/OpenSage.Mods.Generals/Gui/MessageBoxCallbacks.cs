using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class MessageBoxCallbacks
    {
        private static Window _window;
        private static Game _game;
        private static MapCache _currentMap;

        public static void MessageBoxSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "MessageBox.wnd:ButtonOk":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }
    }
}
