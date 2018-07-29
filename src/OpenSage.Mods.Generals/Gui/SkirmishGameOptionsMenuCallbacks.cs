using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;
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

            var playableSides = game.ContentManager.IniDataContext.PlayerTemplates.FindAll(i => i.PlayableSide);
            if(playableSides.Count > 0)
            {
                var sideList = playableSides.Select(i => i.DisplayName).ToList();
                sideList.Insert(0, "GUI:RandomSide");

                FillComboBoxOptions(ComboBoxPlayerTemplatePrefix, sideList.ToArray());
            }

            if (game.ContentManager.IniDataContext.MultiplayerColors.Count > 0)
            {
                var colors = game.ContentManager.IniDataContext.MultiplayerColors.Select(i => new KeyValuePair<string, ColorRgbaF>(i.TooltipName, i.RgbColor.ToColorRgbaF())).ToList();
                var randomColor = new KeyValuePair<string, ColorRgbaF>("GUI:???", ColorRgbaF.White);
                colors.Insert(0, randomColor);

                FillColorComboBoxOptions(colors.ToArray());
            }
            
            FillComboBoxOptions(ComboBoxPlayerPrefix, new[]
            {
                "GUI:Open", "GUI:Closed", "GUI:EasyAI", "GUI:MediumAI", "GUI:HardAI"
            });
        }

        private static void FillComboBoxOptions(string key, string[] options, int selectedIndex = 0)
        {
            var comboBoxs = Control.GetSelfAndDescendants(_window).OfType<ComboBox>().Where(i => i.Name.StartsWith(key));
            foreach (ComboBox comboBox in comboBoxs)
            {
                if(comboBox.Name.Length -1 != key.Length) continue;
                ListBoxDataItem[] items = options.Select(i =>
                    new ListBoxDataItem(comboBox, new[] {_game.ContentManager.TranslationManager.Lookup(i)})).ToArray();
                comboBox.Items = items;
                comboBox.SelectedIndex = selectedIndex;
            }
        }

        private static void FillColorComboBoxOptions(KeyValuePair<string, ColorRgbaF>[] options, int selectedIndex = 0)
        {
            var comboBoxs = Control.GetSelfAndDescendants(_window).OfType<ComboBox>().Where(i=>i.Name.StartsWith(ComboBoxColorPrefix));
            foreach (ComboBox comboBox in comboBoxs)
            {
                ListBoxDataItem[] items = options.Select(i =>
                    new ListBoxDataItem(comboBox, new[] { _game.ContentManager.TranslationManager.Lookup(i.Key) }, i.Value)).ToArray();
                comboBox.Items = items;
                comboBox.SelectedIndex = selectedIndex;
            }
        }
    }
}
