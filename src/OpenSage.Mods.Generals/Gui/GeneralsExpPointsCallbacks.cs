using OpenSage.Gui.ControlBar;
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
                var buttonControl = window.Controls.FindControl("GeneralsExpPoints.wnd:ButtonRank1Number" + i) as Button;
                if (rank1 != null && rank1.Value.Buttons.TryGetValue(i + 1, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    CommandButtonUtils.SetCommandButton(buttonControl, commandButton, (GeneralsControlBar) game.Scene2D.ControlBar);

                    switch (commandButton.Command)
                    {
                        case CommandType.PurchaseScience:
                            buttonControl.Enabled = game.Scene3D.LocalPlayer.ScienceAvailable(commandButton.Science[0].Value);
                            break;
                    }
                }
            }

            var rank3 = game.Scene3D.LocalPlayer.Template.PurchaseScienceCommandSetRank3;
            for (int i = 0; i < 9; i++)
            {
                var buttonControl = window.Controls.FindControl("GeneralsExpPoints.wnd:ButtonRank3Number" + i) as Button;
                if (rank1 != null && rank3.Value.Buttons.TryGetValue(i + 1, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    CommandButtonUtils.SetCommandButton(buttonControl, commandButton, (GeneralsControlBar) game.Scene2D.ControlBar);

                    switch (commandButton.Command)
                    {
                        case CommandType.PurchaseScience:
                            buttonControl.Enabled = game.Scene3D.LocalPlayer.ScienceAvailable(commandButton.Science[0].Value);
                            break;
                    }
                }
            }

            var rank8 = game.Scene3D.LocalPlayer.Template.PurchaseScienceCommandSetRank8;
            for (int i = 0; i < 1; i++)
            {
                var buttonControl = window.Controls.FindControl("GeneralsExpPoints.wnd:ButtonRank8Number" + i) as Button;
                if (rank1 != null && rank8.Value.Buttons.TryGetValue(i + 1, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    CommandButtonUtils.SetCommandButton(buttonControl, commandButton, (GeneralsControlBar) game.Scene2D.ControlBar);

                    switch (commandButton.Command)
                    {
                        case CommandType.PurchaseScience:
                            buttonControl.Enabled = game.Scene3D.LocalPlayer.ScienceAvailable(commandButton.Science[0].Value);
                            break;
                    }
                }
            }
        }
    }
}
