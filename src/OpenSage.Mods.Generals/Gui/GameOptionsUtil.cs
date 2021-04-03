using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using OpenSage.Content;
using OpenSage.Content.Translation;
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
        protected static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public const string ComboBoxTeamPrefix = ":ComboBoxTeam";
        public const string ComboBoxPlayerTemplatePrefix = ":ComboBoxPlayerTemplate";
        public const string ComboBoxColorPrefix = ":ComboBoxColor";
        public const string ComboBoxPlayerPrefix = ":ComboBoxPlayer";
        public const string ButtonAcceptPrefix = ":ButtonAccept";
        public const string ButtonStart = ":ButtonStart";

        private readonly string _optionsPath;
        private readonly string _mapSelectPath;
        private readonly List<PlayerTemplate> _playableSides;
        private readonly Window _window;
        private readonly Game _game;
        private readonly Regex _mapPositionButtonRegex;
        private readonly Dictionary<int, (int Position, Control Control)> _playerToMapPosition = new();
        private readonly Dictionary<int, int> _mapPositionToPlayer = new();

        public MapCache CurrentMap { get; private set; }

        public GameOptionsUtil(Window window, Game game, string basePrefix)
        {
            _optionsPath = basePrefix + "GameOptionsMenu.wnd";
            _mapSelectPath = basePrefix + "MapSelectMenu.wnd";
            _mapPositionButtonRegex = new Regex($"^{_optionsPath}:ButtonMapStartPosition(\\d+)$");

            _window = window;
            _game = game;

            var mapCaches = _game.AssetStore.MapCaches;

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

            _playableSides = _game.GetPlayableSides().ToList();
            if (_playableSides.Count > 0)
            {
                var sideList = _playableSides.Select(i => i.DisplayName).ToList();
                sideList.Insert(0, "GUI:RandomSide");

                FillComboBoxOptions(_optionsPath + ComboBoxPlayerTemplatePrefix, sideList.ToArray());
            }

            if (game.AssetStore.MultiplayerColors.Count > 0)
            {
                var colors = game.AssetStore.MultiplayerColors.Select(i => new Tuple<string, ColorRgbaF>(i.TooltipName, i.RgbColor.ToColorRgbaF())).ToList();
                var randomColor = new Tuple<string, ColorRgbaF>("GUI:???", ColorRgbaF.White);
                colors.Insert(0, randomColor);

                FillColorComboBoxOptions(colors.ToArray());
            }

            FillComboBoxOptions(_optionsPath + ComboBoxPlayerPrefix, new[]
            {
                "GUI:Open", "GUI:Closed", "GUI:EasyAI", "GUI:MediumAI", "GUI:HardAI"
            });

            foreach (var prefix in new string[]{
                ComboBoxTeamPrefix,
                ComboBoxPlayerTemplatePrefix,
                ComboBoxColorPrefix,
                ComboBoxPlayerPrefix
            })
            {
                for (var j = 0; j < SkirmishGameSettings.MaxNumberOfPlayers; j++)
                {
                    var i = j;
                    var key = _optionsPath + prefix + i;

                    var comboBox = Control.GetSelfAndDescendants(_window).OfType<ComboBox>().FirstOrDefault(x => x.Name == key);

                    if (comboBox != null)
                    {
                        var listBox = (ListBox) comboBox.Controls[2];
                        listBox.SelectedIndexChanged += (sender, e) => OnSlotIndexChanged(i, prefix, comboBox.SelectedIndex);
                    }
                    else
                    {
                        Logger.Error($"Did not find control {key}");
                        continue;
                    }
                }
            }
        }

        private void OnSlotIndexChanged(int index, string name, int value)
        {
            var slot = _game.SkirmishManager.Settings.Slots[index];

            switch (name)
            {
                case ComboBoxColorPrefix:
                    Logger.Trace($"Changed the color box to {value}");
                    slot.ColorIndex = (byte) value;
                    break;
                case ComboBoxPlayerPrefix:
                    Logger.Trace($"Changed the player type box to {value}");
                    slot.State = value switch
                    {
                        0 => SkirmishSlotState.Open,
                        1 => SkirmishSlotState.Closed,
                        2 => SkirmishSlotState.EasyArmy,
                        3 => SkirmishSlotState.MediumArmy,
                        4 => SkirmishSlotState.HardArmy,
                        _ => throw new ArgumentException("invalid player type: " + value)
                    };

                    if (slot.State == SkirmishSlotState.Open || slot.State == SkirmishSlotState.Closed)
                    {
                        RemovePlayerFromMap(index);
                    }                    

                    break;
                case ComboBoxPlayerTemplatePrefix:
                    Logger.Trace($"Changed the faction box to {value}");
                    slot.FactionIndex = (byte) value;
                    break;
                case ComboBoxTeamPrefix:
                    Logger.Trace($"Changed the team box to {value}");
                    slot.Team = (byte) value;
                    break;
            }
        }

        private void UpdateMapPositionForPlayer(int player, int position)
        {
            var slot = _game.SkirmishManager.Settings.Slots[player];
            slot.StartPosition = position;
        }

        private void RemovePlayerFromMap(int player)
        {
            if (_playerToMapPosition.TryGetValue(player, out var p))
            {
                // we need to make sure this player doesn't have a map position set, and remove it if they do
                _playerToMapPosition.Remove(player);
                _mapPositionToPlayer.Remove(p.Position);
                p.Control.Text = string.Empty;
                UpdateMapPositionForPlayer(player, 0);
            }
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

                        context.Game.SkirmishManager.HandleStartButtonClickAsync();
                    }
                    else
                    {
                        Match match;
                        if ((match = _mapPositionButtonRegex.Match(message.Element.Name ?? "")).Success)
                        {
                            // TODO: get the index of the player making the request, so players can't change other players in LAN games?
                            const int playerIndex = 0;
                            // Positions are 1-indexed in game but this comes to us as 0-indexed
                            var position = int.Parse(match.Groups[1].Value) + 1;

                            var slotWasOccupied = _mapPositionToPlayer.TryGetValue(position, out var existingPlayer);
                            var startIndex = slotWasOccupied ? existingPlayer : 0;
                            var placedPlayer = false;

                            // for the purposes of AI control, this assumes that the host will always be player 0
                            foreach (var i in GetPlayersInLobby().OrderBy(_ => _).SkipWhile(x => x < startIndex))
                            {
                                if (!_playerToMapPosition.ContainsKey(i))
                                {
                                    // just remove whoever was in the spot previously
                                    if (_mapPositionToPlayer.TryGetValue(position, out var previousPlayer))
                                    {
                                        _playerToMapPosition.Remove(previousPlayer);
                                        UpdateMapPositionForPlayer(previousPlayer, 0);
                                    }

                                    _mapPositionToPlayer[position] = i;
                                    _playerToMapPosition[i] = (position, message.Element);
                                    message.Element.Text = (i + 1).ToString();
                                    UpdateMapPositionForPlayer(i, position);
                                    placedPlayer = true;
                                    Logger.Info($"Selected position {position} for player {i}");
                                    break;
                                }
                            }

                            if (!placedPlayer)
                            {
                                if (slotWasOccupied)
                                {
                                    // remove whoever was there
                                    if (_mapPositionToPlayer.TryGetValue(position, out var previousPlayer))
                                    {
                                        _playerToMapPosition.Remove(previousPlayer);
                                        _mapPositionToPlayer.Remove(position);
                                        message.Element.Text = string.Empty;
                                        UpdateMapPositionForPlayer(previousPlayer, 0);
                                    }
                                }
                                else
                                {
                                    // move ourselves there
                                    if (_playerToMapPosition.TryGetValue(playerIndex, out var p))
                                    {
                                        // first remove our existing position from the map
                                        _mapPositionToPlayer.Remove(p.Position);
                                        p.Control.Text = "";
                                        _playerToMapPosition.Remove(playerIndex);
                                    }

                                    _mapPositionToPlayer[position] = playerIndex;
                                    _playerToMapPosition[playerIndex] = (position, message.Element);
                                    message.Element.Text = (playerIndex + 1).ToString();
                                    UpdateMapPositionForPlayer(playerIndex, position);
                                    Logger.Info($"Selected position {position} for player {playerIndex}");
                                }
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }

                    break;
                default:
                    return false;
            }

            return true;
        }

        public void UpdateUI(Window window)
        {
            var mapName = _game.SkirmishManager.Settings.MapName;
            if (mapName != CurrentMap.Name && mapName != null)
            {
                var mapCache = _game.AssetStore.MapCaches.GetByName(mapName);
                if (mapCache == null)
                {
                    Logger.Warn($"Map {mapName} not found");
                }
                else
                {
                    SetCurrentMap(mapCache);
                }
            }

            foreach (var slot in _game.SkirmishManager.Settings.Slots)
            {
                var colorCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxColorPrefix}{slot.Index}");
                if (colorCombo.SelectedIndex != slot.ColorIndex)
                    colorCombo.SelectedIndex = slot.ColorIndex;

                var teamCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxTeamPrefix}{slot.Index}");
                if (teamCombo.SelectedIndex != slot.Team)
                    teamCombo.SelectedIndex = slot.Team;

                var playerTemplateCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxPlayerTemplatePrefix}{slot.Index}");
                if (playerTemplateCombo.SelectedIndex != slot.FactionIndex)
                    playerTemplateCombo.SelectedIndex = slot.FactionIndex;

                var buttonAccepted = (Button) window.Controls.FindControl($"{_optionsPath}{ButtonAcceptPrefix}{slot.Index}");
                var playerCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxPlayerPrefix}{slot.Index}");

                var isLocalSlot = slot == _game.SkirmishManager.Settings.LocalSlot;
                var editable = isLocalSlot || (_game.SkirmishManager.IsHosting && slot.State != SkirmishSlotState.Human);

                playerCombo.Enabled = !isLocalSlot && _game.SkirmishManager.IsHosting;

                buttonAccepted.Visible = slot.State == SkirmishSlotState.Human;

                if (slot.State == SkirmishSlotState.Human)
                {
                    if (buttonAccepted.Enabled != slot.Ready)
                        buttonAccepted.Enabled = slot.Ready;

                    playerCombo.Controls[0].Text = slot.PlayerName;
                }
                else
                {
                    playerCombo.Controls[0].Text = slot.State.ToString();
                }

                colorCombo.Enabled = editable;
                teamCombo.Enabled = editable;
                playerTemplateCombo.Enabled = editable;
            };

            var buttonStart = (Button) window.Controls.FindControl($"{_optionsPath}{ButtonStart}");
            buttonStart.Enabled = _game.SkirmishManager.IsStartButtonEnabled();
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

            for (int i = 0; i < SkirmishGameSettings.MaxNumberOfPlayers; ++i)
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

            for (int i = 0; i < SkirmishGameSettings.MaxNumberOfPlayers; ++i)
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

        /// <summary>
        /// Gets the indices of the players currently in the lobby
        /// </summary>
        private IEnumerable<int> GetPlayersInLobby()
        {
            yield return 0;
            for (var i = 1; i < SkirmishGameSettings.MaxNumberOfPlayers; i++)
            {
                var selected = GetSelectedComboBoxIndex(_optionsPath + ComboBoxPlayerPrefix + i);

                if (selected >= 2)
                {
                    yield return i;
                }
            }
        }

        private void ParsePlayerSettings(Game game, out PlayerSetting?[] settings)
        {
            var settingsList = new List<PlayerSetting?>();
            var rnd = new Random();
            int selected = 0;

            for (int i = 0; i < SkirmishGameSettings.MaxNumberOfPlayers; i++)
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

                var mpColors = game.AssetStore.MultiplayerColors;

                // Get the selected player color
                selected = GetSelectedComboBoxIndex(_optionsPath + ComboBoxColorPrefix + i);
                if (selected > 0)
                {
                    setting.Color = mpColors.GetByIndex(selected - 1).RgbColor;
                }
                else
                {
                    // TODO: make sure the color isn't already used
                    var r = rnd.Next(mpColors.Count);
                    setting.Color = mpColors.GetByIndex(r).RgbColor;
                }

                // Get the selected player faction
                selected = GetSelectedComboBoxIndex(_optionsPath + ComboBoxPlayerTemplatePrefix + i);

                if (selected > 0)
                {
                    setting.Template = _playableSides[selected - 1];
                }
                else
                {
                    // TODO: make sure the color isn't already used
                    int r = rnd.Next(_playableSides.Count);
                    setting.Template = _playableSides[r];
                }

                // Get the selected player team
                selected = GetSelectedComboBoxIndex(_optionsPath + ComboBoxTeamPrefix + i);

                setting.Team = selected;

                setting.StartPosition =
                    _playerToMapPosition.TryGetValue(i, out var startPosition) ? startPosition.Position : null;

                settingsList.Add(setting);
            }

            settings = settingsList.ToArray();
        }

        public void SetCurrentMap(MapCache mapCache)
        {
            Logger.Info("Set current map to " + mapCache.Name);

            CurrentMap = mapCache;

            var mapWindow = _window.Controls.FindControl(_optionsPath + ":MapWindow");

            MapUtils.SetMapPreview(mapCache, mapWindow, _game);

            // Set map text
            var textEntryMap = _window.Controls.FindControl(_optionsPath + ":TextEntryMapDisplay");
            var mapKey = mapCache.GetNameKey();

            textEntryMap.Text = mapKey.Translate();
        }
    }
}
