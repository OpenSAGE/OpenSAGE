#nullable enable

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
        private static Window? _window;

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
                var buttonControl = (Button)_window!.Controls.FindControl($"GeneralsExpPoints.wnd:ButtonRank{rank}Number" + i);
                if (commandSet.Buttons.TryGetValue(i + 1, out var commandButtonReference))
                {
                    var commandButton = commandButtonReference.Value;

                    if (commandButton.Options.Get(CommandButtonOption.ScriptOnly))
                    {
                        buttonControl.Hide();
                        continue;
                    }

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
            var rank1 = player.Template!.PurchaseScienceCommandSetRank1;
            ApplyRankCommandSet(rank1.Value, 1, player, controlBar);

            var rank3 = player.Template.PurchaseScienceCommandSetRank3;
            ApplyRankCommandSet(rank3.Value, 3, player, controlBar);

            var rank8 = player.Template.PurchaseScienceCommandSetRank8;
            ApplyRankCommandSet(rank8.Value, 8, player, controlBar);
        }

        private static int CurrentRankStringRank = int.MinValue;
        private static LocalizedString? BaseRankText;
        private static readonly LocalizedString BaseStaticLevelText = new("SCIENCE:Rank");

        public static void Update(Player player, GeneralsControlBar controlBar)
        {
            if (_window == null)
            {
                return;
            }

            var currentRank = player.Rank.CurrentRank;
            //Update title
            var lblTitle = _window.Controls.FindControl("GeneralsExpPoints.wnd:StaticTextTitle") as Label;

            if (CurrentRankStringRank != currentRank)
            {
                BaseRankText = new LocalizedString($"SCIENCE:Rank{currentRank}");
                CurrentRankStringRank = currentRank;
            }

            lblTitle!.Text = BaseRankText!.Localize();

            var lblPoints = _window.Controls.FindControl("GeneralsExpPoints.wnd:StaticTextRankPointsAvailable") as Label;
            lblPoints!.Text = player.SciencePurchasePoints.ToString();

            var lblLevel = _window.Controls.FindControl("GeneralsExpPoints.wnd:StaticTextLevel") as Label;
            lblLevel!.Text = BaseStaticLevelText.Localize(currentRank);

            ApplyCommandSets(player, controlBar);
        }

        public static void SetWindow(Window window)
        {
            _window = window;
        }
    }
}
