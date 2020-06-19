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
                                context.Game.SkirmishManager.Stop();

                                //this should be called by the OnStop callback
                                //context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
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
            var textChat = (TextBox) window.Controls.FindControl(TextEntryChatPrefix);
            textChat.Text = string.Empty;

            game.SkirmishManager.OnStop += () =>
            {
                //TODO: somehow make this work
                game.Scene2D.WndWindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
            };
        }

        public static void LanGameOptionsMenuUpdate(Window window, Game game)
        {
            //TODO: update manager state to slots
            game.SkirmishManager.Slots.ForEach(slot =>
            {
                var colorCombo = (ComboBox) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxColor{slot.id}");
                colorCombo.SelectedIndex = slot.ColorIndex;

                var teamCombo = (ComboBox) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxTeam{slot.id}");
                teamCombo.SelectedIndex = slot.id;

                var playerTemplateCombo = (ComboBox) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxPlayerTemplate{slot.id}");
                playerTemplateCombo.SelectedIndex = slot.FactionIndex;

                var playerCombo = (ComboBox) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ComboBoxPlayer{slot.id}");
                //playerCombo.Text = slot.HumanName;
                playerCombo.SelectedIndex = (int) slot.Type;

                var buttonAccepted = (Button) window.Controls.FindControl($"LanGameOptionsMenu.wnd:ButtonAccept{slot.id}");
                buttonAccepted.Visible = slot.Ready;
            });

        }

    }
}
