using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
  [WndCallbacks]
  public static class SharedCallbacks
  {
    public static void GameWinBlockInput(Control control, WndWindowMessage message, ControlCallbackContext context) {
        // This intentionally does nothing.
        // The only purpose of this callback is to handle input messages on non-interactive parts of windows.
    }
  }
}
