using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class GeneralsExpPointsCallbacks
    {
        public static void GeneralsExpPointsSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "GeneralsExpPoints.wnd:ButtonExit":
                            context.WindowManager.PopWindow();
                            break;

                    }
                    break;
            }
        }

        public static void GeneralsExpPointsInit(Window window, Game game)
        {
            var rank1 = game.Scene3D.LocalPlayer.Template.PurchaseScienceCommandSetRank1;
            for (int i = 0; i < 3; i++)
            {
                var buttonControl = window.Controls.FindControl("ButtonRank1Number" + i);
                if (rank1 != null && rank1.Value.Buttons.TryGetValue(i, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    buttonControl.BackgroundImage = window.ImageLoader.CreateFromMappedImageReference(commandButton.ButtonImage);

                    buttonControl.DisabledBackgroundImage = buttonControl.BackgroundImage?.WithGrayscale(true);

                }
            }
        }
    }
}
