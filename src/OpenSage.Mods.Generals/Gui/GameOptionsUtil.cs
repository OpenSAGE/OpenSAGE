using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Content.Translation;
using OpenSage.Data.Ini;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    public class GameOptionsUtil
    {
        private const string ComboBoxTeamPrefix = ":ComboBoxTeam";
        private const string ComboBoxPlayerTemplatePrefix = ":ComboBoxPlayerTemplate";
        private const string ComboBoxColorPrefix = ":ComboBoxColor";
        private const string ComboBoxPlayerPrefix = ":ComboBoxPlayer";

        private readonly string _optionsPath;
        private readonly string _mapSelectPath;
        private Window _window;
        private Game _game;

        public MapCache CurrentMap { get; private set; }

        public GameOptionsUtil(Window window, Game game, string basePrefix)
        {
            _optionsPath = basePrefix + "GameOptionsMenu.wnd";
            _mapSelectPath = basePrefix + "MapSelectMenu.wnd";

            _window = window;
            _game = game;

            var mapCaches = _game.ContentManager.IniDataContext.MapCaches;

            foreach (var cache in mapCaches)
            {
                if (cache.IsMultiplayer)
                {
                    SetCurrentMap(cache);
                    break;
                }
            }

            FillComboBoxOptions(_optionsPath + ComboBoxTeamPrefix, new[]
            {
                "Team:0", "Team:1", "Team:2", "Team:3", "Team:4"
            });

            var playableSides = _game.ContentManager.IniDataContext.PlayerTemplates.FindAll(i => i.PlayableSide);
            if (playableSides.Count > 0)
            {
                var sideList = playableSides.Select(i => i.DisplayName).ToList();
                sideList.Insert(0, "GUI:RandomSide");

                FillComboBoxOptions(_optionsPath + ComboBoxPlayerTemplatePrefix, sideList.ToArray());
            }

            if (game.ContentManager.IniDataContext.MultiplayerColors.Count > 0)
            {
                var colors = game.ContentManager.IniDataContext.MultiplayerColors.Select(i => new Tuple<string, ColorRgbaF>(i.TooltipName, i.RgbColor.ToColorRgbaF())).ToList();
                var randomColor = new Tuple<string, ColorRgbaF>("GUI:???", ColorRgbaF.White);
                colors.Insert(0, randomColor);

                FillColorComboBoxOptions(colors.ToArray());
            }

            FillComboBoxOptions(_optionsPath + ComboBoxPlayerPrefix, new[]
            {
                "GUI:Open", "GUI:Closed", "GUI:EasyAI", "GUI:MediumAI", "GUI:HardAI"
            });
        }

        public bool HandleSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    if (message.Element.Name == _optionsPath + ":ButtonSelectMap")
                    {
                        OpenMapSelection(context);
                    }
                    else if (message.Element.Name == _optionsPath + ":ButtonStart")
                    {
                        ParsePlayerSettings(context.Game, out PlayerSetting?[] settings);

                        if (!ValidateSettings(settings, context.WindowManager))
                        {
                            return true;
                        }

                        context.Game.Scene2D.WndWindowManager.PopWindow();
                        context.Game.StartMultiPlayerGame(
                            CurrentMap.Name,
                            new EchoConnection(),
                            settings,
                            0);
                    }
                    else
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }

        private bool ValidateSettings(PlayerSetting?[] settings, WndWindowManager manager)
        {
            if (settings.Length > CurrentMap.NumPlayers)
            {
                var translation = _game.ContentManager.TranslationManager;
                var messageBox = manager.PushWindow(@"Menus\MessageBox.wnd");
                messageBox.Controls.FindControl("MessageBox.wnd:StaticTextTitle").Text = "GUI:ErrorStartingGame".Translate();
                var staticTextTitle = messageBox.Controls.FindControl("MessageBox.wnd:StaticTextTitle") as Label;
                staticTextTitle.TextAlignment = TextAlignment.Leading;
                messageBox.Controls.FindControl("MessageBox.wnd:StaticTextMessage").Text = "GUI:TooManyPlayers".TranslateFormatted(CurrentMap.NumPlayers);
                messageBox.Controls.FindControl("MessageBox.wnd:ButtonOk").Show();
                return false;
            }

            return true;
        }

        private void OpenMapSelection(ControlCallbackContext context)
        {
            // Hide controls
            _window.Controls.FindControl(_optionsPath + ":ButtonSelectMap").Hide();
            _window.Controls.FindControl(_optionsPath + ":MapWindow").Hide();
            _window.Controls.FindControl(_optionsPath + ":TextEntryMapDisplay").Hide();
            _window.Controls.FindControl(_optionsPath + ":StaticTextMapPreview").Hide();
            _window.Controls.FindControl(_optionsPath + ":StaticTextTeam").Hide();
            _window.Controls.FindControl(_optionsPath + ":StaticTextFaction").Hide();
            _window.Controls.FindControl(_optionsPath + ":StaticTextColor").Hide();

            for (int i = 0; i < 8; ++i)
            {
                _window.Controls.FindControl(_optionsPath + ":ComboBoxTeam" + i.ToString()).Hide();
                _window.Controls.FindControl(_optionsPath + ":ComboBoxPlayerTemplate" + i.ToString()).Hide();
                _window.Controls.FindControl(_optionsPath + ":ComboBoxColor" + i.ToString()).Hide();
            }

            context.WindowManager.PushWindow(@"Menus\" + _mapSelectPath);
        }

        public void CloseMapSelection(ControlCallbackContext context)
        {
            // Reshow controls
            _window.Controls.FindControl(_optionsPath + ":ButtonSelectMap").Show();
            _window.Controls.FindControl(_optionsPath + ":MapWindow").Show();
            _window.Controls.FindControl(_optionsPath + ":TextEntryMapDisplay").Show();
            _window.Controls.FindControl(_optionsPath + ":StaticTextMapPreview").Show();
            _window.Controls.FindControl(_optionsPath + ":StaticTextTeam").Show();
            _window.Controls.FindControl(_optionsPath + ":StaticTextFaction").Show();
            _window.Controls.FindControl(_optionsPath + ":StaticTextColor").Show();

            for (int i = 0; i < 8; ++i)
            {
                _window.Controls.FindControl(_optionsPath + ":ComboBoxTeam" + i.ToString()).Show();
                _window.Controls.FindControl(_optionsPath + ":ComboBoxPlayerTemplate" + i.ToString()).Show();
                _window.Controls.FindControl(_optionsPath + ":ComboBoxColor" + i.ToString()).Show();
            }

            context.WindowManager.PopWindow();
        }

        private void FillComboBoxOptions(string key, string[] options, int selectedIndex = 0)
        {
            var comboBoxes = Control.GetSelfAndDescendants(_window).OfType<ComboBox>().Where(i => i.Name.StartsWith(key));
            foreach (var comboBox in comboBoxes)
            {
                if (comboBox.Name.Length - 1 != key.Length)
                {
                    continue;
                }
                var items = options.Select(i => new ListBoxDataItem(null, new[] { i.Translate() }, comboBox.TextColor)).ToArray();
                comboBox.Items = items;
                comboBox.SelectedIndex = selectedIndex;
            }
        }

        private void FillColorComboBoxOptions(Tuple<string, ColorRgbaF>[] options, int selectedIndex = 0)
        {
            var comboBoxes = Control.GetSelfAndDescendants(_window).OfType<ComboBox>().Where(i => i.Name.StartsWith(_optionsPath + ComboBoxColorPrefix));
            foreach (var comboBox in comboBoxes)
            {
                var items = options.Select(i =>
                    new ListBoxDataItem(comboBox, new[] { i.Item1.Translate() }, i.Item2)).ToArray();
                comboBox.Items = items;
                comboBox.SelectedIndex = selectedIndex;
            }
        }

        private int GetSelectedComboBoxIndex(string control)
        {
            var playerOwnerBox = (ComboBox) _window.Controls.FindControl(control);
            var playerOwnerList = (ListBox) playerOwnerBox.Controls[2];

            return playerOwnerList.SelectedIndex;
        }

        private void ParsePlayerSettings(Game game, out PlayerSetting?[] settings)
        {
            var settingsList = new List<PlayerSetting?>();
            var rnd = new Random();
            int selected = 0;

            for (int i = 0; i < 8; i++)
            {
                var setting = new PlayerSetting();
                setting.Owner = PlayerOwner.Player;

                // Get the selected player owner
                if (i >= 1)
                {
                    selected = GetSelectedComboBoxIndex(_optionsPath + ComboBoxPlayerPrefix + i);

                    if (selected >= 2)
                    {
                        setting.Owner = PlayerOwner.EasyAi + (selected - 2);
                    }
                    else
                    {
                        // TODO: make sure the color isn't already used
                        setting.Owner = PlayerOwner.None;
                    }
                }

                if (setting.Owner == PlayerOwner.None)
                {
                    continue;
                }

                var mpColors = game.ContentManager.IniDataContext.MultiplayerColors;

                // Get the selected player color
                selected = GetSelectedComboBoxIndex(_optionsPath + ComboBoxColorPrefix + i);
                if (selected > 0)
                {
                    setting.Color = mpColors[selected - 1].RgbColor;
                }
                else
                {
                    // TODO: make sure the color isn't already used
                    var r = rnd.Next(mpColors.Count);
                    setting.Color = mpColors[r].RgbColor;
                }

                // Get the selected player faction
                selected = GetSelectedComboBoxIndex(_optionsPath + ComboBoxPlayerTemplatePrefix + i);
                var playableSides = game.ContentManager.IniDataContext.PlayerTemplates.FindAll(x => x.PlayableSide);

                if (selected > 0)
                {
                    setting.Side = playableSides[selected - 1].Side;
                }
                else
                {
                    // TODO: make sure the color isn't already used
                    int r = rnd.Next(playableSides.Count);
                    setting.Side = playableSides[r].Side;
                }

                settingsList.Add(setting);
            }

            settings = settingsList.ToArray();
        }

        public void SetCurrentMap(MapCache mapCache)
        {
            CurrentMap = mapCache;

            var mapWindow = _window.Controls.FindControl(_optionsPath + ":MapWindow");

            MapUtils.SetMapPreview(mapCache, mapWindow, _game);

            // Set map text
            var textEntryMap = _window.Controls.FindControl(_optionsPath + ":TextEntryMapDisplay");
            var translation = _game.ContentManager.TranslationManager;
            var mapKey = mapCache.GetNameKey();

            textEntryMap.Text = mapKey.Translate();
        }
    }
}
