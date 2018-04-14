using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class QuitMenuCallbacks
    {
        public static void QuitMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton when message.Element.Name == "QuitMenu.wnd:ButtonReturn":
                    context.WindowManager.PopWindow();
                    break;
                case WndWindowMessageType.SelectedButton when message.Element.Name == "QuitMenu.wnd:ButtonExit":
                    context.WindowManager.PopWindow();
                    // TODO: Implement this when we can reset the game state and replace the Scene3D without crashing.
                    // context.Game.EndGame();
                    break;
            }
        }
    }
}
