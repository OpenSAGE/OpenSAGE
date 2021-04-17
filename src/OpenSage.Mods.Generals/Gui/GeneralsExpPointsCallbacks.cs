using OpenSage.Content.Translation;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Logic;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class GeneralsExpPointsCallbacks
    {
        private static Window _window;

        public static void GeneralsExpPointsSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "GeneralsExpPoints.wnd:ButtonExit":
                            context.WindowManager.PopWindow();
                            _window = null;
                            break;

                    }
                    break;
            }
        }

        public static void Update(Player player, GeneralsControlBar controlBar)
        {
            if (_window == null)
            {
                return;
            }

            var currentRank = player.Rank.CurrentRank;
            //Update title
            var lblTitle = _window.Controls.FindControl("GeneralsExpPoints.wnd:StaticTextTitle") as Label;
            lblTitle.Text = ("SCIENCE:Rank" + currentRank).Translate();

            var lblLevel = _window.Controls.FindControl("GeneralsExpPoints.wnd:StaticTextLevel") as Label;
            lblLevel.Text = ("SCIENCE:Rank").TranslateFormatted(currentRank); // TODO: this doesn't replace %d correctly yet

            var rank1 = player.Template.PurchaseScienceCommandSetRank1;
            for (int i = 0; i < 3; i++)
            {
                var buttonControl = _window.Controls.FindControl("GeneralsExpPoints.wnd:ButtonRank1Number" + i) as Button;
                if (rank1 != null && rank1.Value.Buttons.TryGetValue(i + 1, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    CommandButtonUtils.SetCommandButton(buttonControl, commandButton, controlBar);

                    switch (commandButton.Command)
                    {
                        case CommandType.PurchaseScience:
                            buttonControl.Enabled = player.ScienceAvailable(commandButton.Science[0].Value);
                            break;
                    }
                }
            }

            var rank3 = player.Template.PurchaseScienceCommandSetRank3;
            for (int i = 0; i < 9; i++)
            {
                var buttonControl = _window.Controls.FindControl("GeneralsExpPoints.wnd:ButtonRank3Number" + i) as Button;
                if (rank1 != null && rank3.Value.Buttons.TryGetValue(i + 1, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    CommandButtonUtils.SetCommandButton(buttonControl, commandButton, controlBar);

                    switch (commandButton.Command)
                    {
                        case CommandType.PurchaseScience:
                            buttonControl.Enabled = player.ScienceAvailable(commandButton.Science[0].Value);
                            break;
                    }
                }
            }

            var rank8 = player.Template.PurchaseScienceCommandSetRank8;
            for (int i = 0; i < 1; i++)
            {
                var buttonControl = _window.Controls.FindControl("GeneralsExpPoints.wnd:ButtonRank8Number" + i) as Button;
                if (rank1 != null && rank8.Value.Buttons.TryGetValue(i + 1, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    CommandButtonUtils.SetCommandButton(buttonControl, commandButton, controlBar);

                    switch (commandButton.Command)
                    {
                        case CommandType.PurchaseScience:
                            buttonControl.Enabled = player.ScienceAvailable(commandButton.Science[0].Value);
                            break;
                    }
                }
            }
        }

        public static void SetWindow(Window window)
        {
            _window = window;
        }
    }
}
