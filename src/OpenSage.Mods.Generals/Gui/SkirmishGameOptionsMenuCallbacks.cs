using System.Linq;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class SkirmishGameOptionsMenuCallbacks
    {
        private const string ComboBoxTeamPrefix = "SkirmishGameOptionsMenu.wnd:ComboBoxTeam";
        private const string ComboBoxPlayerTemplatePrefix = "SkirmishGameOptionsMenu.wnd:ComboBoxPlayerTemplate";
        private const string ComboBoxColorPrefix = "SkirmishGameOptionsMenu.wnd:ComboBoxColor";
        private const string ComboBoxPlayerPrefix = "SkirmishGameOptionsMenu.wnd:ComboBoxPlayer";
        private static Window _window;
        private static Game _game;

        public static void SkirmishGameOptionsMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishGameOptionsMenu.wnd:ButtonSelectMap":
                            context.WindowManager.PushWindow(@"Menus\SkirmishMapSelectMenu.wnd");
                            break;

                        case "SkirmishGameOptionsMenu.wnd:ButtonStart":
                            context.Game.Scene2D.WndWindowManager.PopWindow();
                            context.Game.StartGame(
                                @"maps\Alpine Assault\Alpine Assault.map", // TODO
                                new EchoConnection(),
                                new[] { "America" }, // TODO: We need to receive the player list from UI.
                                0); 
                            break;

                        case "SkirmishGameOptionsMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Single Player sub-menu
                            break;
                    }
                    break;
            }
        }

        public static void SkirmishGameOptionsMenuInit(Window window, Game game)
        {

            _window = window;
            _game = game;

            var playerNameTextBox =  (TextBox)_window.Controls.FindControl("SkirmishGameOptionsMenu.wnd:TextEntryPlayerName");
            playerNameTextBox.IsReadOnly = false;
            

            FillComboBoxOptions(ComboBoxTeamPrefix, new[]
            {
                "Team:0", "Team:1", "Team:2", "Team:3", "Team:4"
            });

            //Maybe we can selcet the data by linq with prefix SIDE:
            FillComboBoxOptions(ComboBoxPlayerTemplatePrefix, new[]
            {
                "GUI:RandomSide", "SIDE:America", "SIDE:China", "SIDE:GLA"
            });

            //Maybe we can selcet the data by linq with prefix Color:
            FillComboBoxOptions(ComboBoxColorPrefix, new[]
            {
                "GUI:???", "Color:Gold", "Color:Red", "Color:Green", "Color:Orange", "Color:SkyBlue", "Color:Purple", "Color:Pink"
            });

            FillComboBoxOptions(ComboBoxPlayerPrefix, new[]
            {
                "GUI:Open", "GUI:Closed", "GUI:EasyAI", "GUI:MediumAI", "GUI:HardAI"
            });

        }

        /// <summary>
        /// Fill all related ComboBoxs with the same data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="options"></param>
        /// <param name="selectedIndex"></param>
        private static void FillComboBoxOptions(string key, string[] options, int selectedIndex = 0)
        {
            var comboBoxs = _window.Controls.FindControlsStratsWith<ComboBox>(key);
            foreach (ComboBox comboBox in comboBoxs)
            {
                if(comboBox.Name.Length -1 != key.Length) continue;
                ListBoxDataItem[] items = options.Select(i =>
                    new ListBoxDataItem(comboBox, new[] {_game.ContentManager.TranslationManager.Lookup(i)})).ToArray();
                comboBox.Items = items;
                comboBox.SelectedIndex = selectedIndex;
            }
        }
    }
}
