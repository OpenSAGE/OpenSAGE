using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Logic;
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
                            ParsePlayerSettings(context.Game, out PlayerSetting[] settings);

                            context.Game.Scene2D.WndWindowManager.PopWindow();
                            context.Game.StartGame(
                                @"maps\Alpine Assault\Alpine Assault.map", // TODO
                                new EchoConnection(),
                                settings,
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
        private static int GetSelectedComboBoxIndex(string control)
        {
            var playerOwnerBox = (ComboBox) _window.Controls.FindControl(control);
            var playerOwnerList = (ListBox) playerOwnerBox.Controls[2];

            return playerOwnerList.SelectedIndex;
        }

        //TODO: Parse player count and set stuff
        private static void ParsePlayerSettings(Game game, out PlayerSetting[] settings)
        {
            List<PlayerSetting> settingsList = new List<PlayerSetting>();
            Random rnd = new Random();
            int selected = 0;

            for (int i = 0; i < 8; i++)
            {
                var setting = new PlayerSetting();
                setting.Owner = PlayerOwner.Player;

                //Get the selected player owner
                if (i >= 1)
                {
                    selected = GetSelectedComboBoxIndex("SkirmishGameOptionsMenu.wnd:ComboBoxPlayer" + i);

                    if (selected >= 2)
                    {
                        setting.Owner = PlayerOwner.EasyAi + (selected - 2);
                    }
                    else
                    {
                        //TODO: make sure the color isn't already used
                        setting.Owner = PlayerOwner.None;
                    }
                }

                if (setting.Owner == PlayerOwner.None)
                {
                    continue;
                }

                //Get the selected player color
                selected = GetSelectedComboBoxIndex("SkirmishGameOptionsMenu.wnd:ComboBoxColor" + i);
                if (selected > 0)
                {
                    setting.Color = game.ContentManager.IniDataContext.MultiplayerColors[selected - 1].RgbColor.ToColorRgb();
                }
                else
                {
                    //TODO: make sure the color isn't already used
                    int r = rnd.Next(game.ContentManager.IniDataContext.MultiplayerColors.Count);
                    setting.Color = game.ContentManager.IniDataContext.MultiplayerColors[r].RgbColor.ToColorRgb();
                }

                //Get the selected player faction
                selected = GetSelectedComboBoxIndex("SkirmishGameOptionsMenu.wnd:ComboBoxPlayerTemplate" + i);
                var playableSides = game.ContentManager.IniDataContext.PlayerTemplates.FindAll(x => x.PlayableSide);

                if (selected > 0)
                {
                    setting.Side = playableSides[selected - 1].Side;
                }
                else
                {
                    //TODO: make sure the color isn't already used
                    int r = rnd.Next(playableSides.Count);
                    setting.Side = playableSides[r].Side;
                }

                settingsList.Add(setting);
            }

            settings = settingsList.ToArray();
        }

    }
}
