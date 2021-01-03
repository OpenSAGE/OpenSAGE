using System.Net;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    internal static class NetworkUtils
    {
        public static object OnlineTag = "Online";

        public static void HostGame(ControlCallbackContext context, object windowTag = null)
        {
            context.Game.SkirmishManager = new SkirmishManager.Host(context.Game);
            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd", windowTag);
        }

        public static void JoinGame(ControlCallbackContext context, IPEndPoint endPoint)
        {
            context.Game.SkirmishManager = new SkirmishManager.Client(context.Game, endPoint);
            context.WindowManager.SetWindow(@"Menus\LanGameOptionsMenu.wnd");
        }
    }
}
