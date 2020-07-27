using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    class LanGameOptionsMenuCallbacks
    {
        private const string TextEntryChatPrefix = "LanGameOptionsMenu.wnd:TextEntryChat";


        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static GameOptionsUtil GameOptions { get; private set; }

        public static void LanGameOptionsMenuInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            logger.Info($"Have message {message.MessageType} for control {message.Element.DisplayName}");
        }

        public static void LanGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            logger.Info($"Have message {message.MessageType} for control {control.Name}");
            if (!GameOptions.HandleSystem(control, message, context))
            {
                switch (message.MessageType)
                {
                    case WndWindowMessageType.SelectedButton:
                        switch (message.Element.Name)
                        {
                            case "LanGameOptionsMenu.wnd:ButtonBack":
                                context.Game.SkirmishManager.Quit();

                                //this should be called by the OnStop callback
                                context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
                                break;

                            default:
                                logger.Warn($"No callback for {message.Element.Name}");
                                break;
                        }
                        break;
                }
            }
        }

        public static void LanGameOptionsMenuInit(Window window, Game game)
        {
            GameOptions = new GameOptionsUtil(window, game, "Lan");

            // Clear chat field
            var textChat = (TextBox)window.Controls.FindControl(TextEntryChatPrefix);
            textChat.Text = string.Empty;

            //game.SkirmishManager.OnStop += () =>
            //{
            //    //TODO: somehow make this work
            //    game.Scene2D.WndWindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
            //};
        }

        public static void LanGameOptionsMenuUpdate(Window window, Game game)
        {
            //TODO: update manager state to slots
            foreach (var slot in game.SkirmishManager.SkirmishGame.Slots)
            {
                var colorCombo = (ComboBox)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxColor{slot.Index}");
                if (colorCombo.SelectedIndex != slot.ColorIndex)
                    colorCombo.SelectedIndex = slot.ColorIndex;

                var teamCombo = (ComboBox)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxTeam{slot.Index}");
                if (teamCombo.SelectedIndex != slot.Team)
                    teamCombo.SelectedIndex = slot.Team;

                var playerTemplateCombo = (ComboBox)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxPlayerTemplate{slot.Index}");
                if (playerTemplateCombo.SelectedIndex != slot.FactionIndex)
                    playerTemplateCombo.SelectedIndex = slot.FactionIndex;

                var playerCombo = (ComboBox)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxPlayer{slot.Index}");
                if (slot.State == Network.SkirmishSlotState.Human)
                {
                    playerCombo.Controls[0].Text = slot.PlayerName;
                }
                else
                {
                    playerCombo.Controls[0].Text = slot.State.ToString();
                }

                var buttonAccepted = (Button)window.Controls.FindControl($"LanGameOptionsMenu.wnd:ButtonAccept{slot.Index}");
                if (buttonAccepted.Visible != slot.Ready)
                    buttonAccepted.Visible = slot.Ready;
            };
        }

    }
}
