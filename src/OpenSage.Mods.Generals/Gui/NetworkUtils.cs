using System.Net;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    internal static class NetworkUtils
    {
        public static void HostGame(ControlCallbackContext context)
        {
            context.Game.SkirmishManager = new SkirmishManager.Host(context.Game);
            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
        }

        public static void JoinGame(ControlCallbackContext context, IPEndPoint endPoint)
        {
            context.Game.SkirmishManager = new SkirmishManager.Client(context.Game, endPoint);
            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
        }
    }
}
