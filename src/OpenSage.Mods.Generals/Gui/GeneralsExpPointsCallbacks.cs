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

        private static void ApplyRankCommandSet(CommandSet commandSet, int rank, Player player, GeneralsControlBar controlBar)
        {
            for (int i = 0; i < commandSet.Buttons.Count; i++)
            {
                var buttonControl = _window.Controls.FindControl($"GeneralsExpPoints.wnd:ButtonRank{rank}Number" + i) as Button;
                if (commandSet != null && commandSet.Buttons.TryGetValue(i + 1, out var commandButtonReference))
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

        private static void ApplyCommandSets(Player player, GeneralsControlBar controlBar)
        {
            var rank1 = player.Template.PurchaseScienceCommandSetRank1;
            ApplyRankCommandSet(rank1.Value, 1, player, controlBar);

            var rank3 = player.Template.PurchaseScienceCommandSetRank3;
            ApplyRankCommandSet(rank3.Value, 3, player, controlBar);

            var rank8 = player.Template.PurchaseScienceCommandSetRank8;
            ApplyRankCommandSet(rank8.Value, 8, player, controlBar);
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

            ApplyCommandSets(player, controlBar);
        }

        public static void SetWindow(Window window)
        {
            _window = window;
        }
    }
}
