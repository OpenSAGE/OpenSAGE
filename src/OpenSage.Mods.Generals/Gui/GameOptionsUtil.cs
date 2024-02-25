using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public const string ButtonSelectMap = ":ButtonSelectMap";
        public const string MapWindow = ":MapWindow";

        private readonly string _optionsPath;
        private readonly string _mapSelectPath;
        private readonly List<PlayerTemplate> _playableSides;
        private readonly Window _window;
        private readonly Game _game;

        public MapCache CurrentMap { get; private set; }

        public GameOptionsUtil(Window window, Game game, string basePrefix)
        {
            _optionsPath = basePrefix + "GameOptionsMenu.wnd";
            _mapSelectPath = basePrefix + "MapSelectMenu.wnd";

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

            FillComboBoxOptions(
                _optionsPath + ComboBoxTeamPrefix,
                new[]
                {
                    "Team:0", "Team:1", "Team:2", "Team:3", "Team:4"
                },
                new object[]
                {
                    (sbyte)-1, (sbyte)0, (sbyte)1,(sbyte)2,(sbyte)3,
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
                var colors = game.AssetStore.MultiplayerColors
                    .Select((c, i) => new Tuple<sbyte, string, ColorRgbaF>((sbyte) i, c.TooltipName, c.RgbColor.ToColorRgbaF()))
                    .ToList();
                var randomColor = new Tuple<sbyte, string, ColorRgbaF>(-1, "GUI:???", ColorRgbaF.White);
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
                        listBox.SelectedIndexChanged += (sender, e) => OnSlotIndexChanged(i, prefix, comboBox.SelectedIndex, comboBox.Items[comboBox.SelectedIndex].DataItem);
                    }
                    else
                    {
                        Logger.Error($"Did not find control {key}");
                        continue;
                    }
                }
            }

            var mapWindow = _window.Controls.FindControl(_optionsPath + MapWindow);
            for (int i = 0; i < SkirmishGameSettings.MaxNumberOfPlayers; i++)
            {
                var startPosition = (byte)(i + 1);
                ((Button) mapWindow.Controls[i]).Click += (s, e) => StartingPositionClicked(_game.SkirmishManager.Settings, startPosition);
            }

            _window.Controls.FindControl(_optionsPath + ButtonSelectMap).Enabled = _game.SkirmishManager.IsHosting;
        }

        public static void StartingPositionClicked(SkirmishGameSettings settings, byte clickedPosition)
        {
            var currentlyAssignedPlayer = settings.Slots.FirstOrDefault(s => s.StartPosition == clickedPosition)?.Index ?? -1;

            var assignablePlayers = settings.Slots
                                            .Where(CanAssignPosition)
                                            .Select(s => s.Index)
                                            .ToList();

            var indexOfCurrentlyAssignedPlayer = assignablePlayers.IndexOf(currentlyAssignedPlayer);
            if (currentlyAssignedPlayer >= 0)
            {
                // the clicked position is already assigned
                if (indexOfCurrentlyAssignedPlayer < 0)
                {
                    // the assigned player cannot be removed
                    return;
                }

                settings.Slots[currentlyAssignedPlayer].StartPosition = 0;
                Logger.Trace($"Removed player {currentlyAssignedPlayer} from position {clickedPosition}");
            }

            // find the next assignable player without a starting position
            for (int index = indexOfCurrentlyAssignedPlayer + 1; index < assignablePlayers.Count; index++)
            {
                var nextPlayer = assignablePlayers[index];
                var slot = settings.Slots[nextPlayer];
                if (slot.StartPosition == 0)
                {
                    slot.StartPosition = clickedPosition;
                    Logger.Trace($"Assigned player {slot.Index} to position {clickedPosition}");
                    return;
                }
            }

            // if the clicked position is free and all assignable players already have a position,
            // re-assigned the local player
            if (currentlyAssignedPlayer < 0)
            {
                settings.LocalSlot.StartPosition = clickedPosition;
                Logger.Trace($"Assigned local player {settings.LocalSlotIndex} to position {clickedPosition}");
            }

            bool CanAssignPosition(SkirmishSlot slot)
            {
                var isLocalSlot = slot.Index == settings.LocalSlotIndex;
                var isAI = slot.State == SkirmishSlotState.EasyArmy
                    || slot.State == SkirmishSlotState.MediumArmy
                    || slot.State == SkirmishSlotState.HardArmy;

                return isLocalSlot || (settings.IsHost && isAI);
            }
        }

        private void OnSlotIndexChanged(int index, string name, int value, object dataItem)
        {
            var slot = _game.SkirmishManager.Settings.Slots[index];

            switch (name)
            {
                case ComboBoxColorPrefix:
                    Logger.Trace($"Changed the color box to {value}");
                    slot.ColorIndex = (sbyte) dataItem;
                    break;
                case ComboBoxPlayerPrefix:
                    Logger.Trace($"Changed the player type box to {value}");

                    var wasHumanPlayer = slot.State == SkirmishSlotState.Human;

                    slot.State = value switch
                    {
                        0 => SkirmishSlotState.Open,
                        1 => SkirmishSlotState.Closed,
                        2 => SkirmishSlotState.EasyArmy,
                        3 => SkirmishSlotState.MediumArmy,
                        4 => SkirmishSlotState.HardArmy,
                        _ => throw new ArgumentException("invalid player type: " + value)
                    };

                    if (slot.State == SkirmishSlotState.Open || slot.State == SkirmishSlotState.Closed || wasHumanPlayer)
                    {
                        slot.ClientId = null;
                        slot.PlayerName = null;
                        slot.StartPosition = 0;
                        slot.ColorIndex = 0;
                        slot.FactionIndex = 0;
                        slot.Team = 0;
                        slot.Ready = false;
                        slot.ReadyUpdated = false;
                    }

                    if (wasHumanPlayer)
                    {
                        ((HostSkirmishManager) _game.SkirmishManager).Disconnect(slot);
                    }

                    break;
                case ComboBoxPlayerTemplatePrefix:
                    Logger.Trace($"Changed the faction box to {value}");
                    slot.FactionIndex = (byte) value;
                    break;
                case ComboBoxTeamPrefix:
                    Logger.Trace($"Changed the team box to {value}");
                    slot.Team = (sbyte) dataItem;
                    break;
            }
        }

        public async Task<bool> HandleSystemAsync(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    if (message.Element.Name == _optionsPath + ButtonSelectMap)
                    {
                        OpenMapSelection(context);
                    }
                    else if (message.Element.Name == _optionsPath + ButtonStart)
                    {
                        if (_game.SkirmishManager.Settings.Slots.Count(s => s.State != SkirmishSlotState.Open && s.State != SkirmishSlotState.Closed) > CurrentMap.NumPlayers)
                        {
                            context.WindowManager.ShowMessageBox(
                                "GUI:ErrorStartingGame".Translate(),
                                "LAN:TooManyPlayers".TranslateFormatted(CurrentMap.NumPlayers)); // TODO: this doesn't replace %d correctly yet

                            return true;
                        }
                        else
                        {
                            var availablePositions = new List<byte>(CurrentMap.NumPlayers);
                            for (byte a = 1; a <= CurrentMap.NumPlayers; a++)
                            {
                                availablePositions.Add(a);
                            }

                            foreach (var slot in _game.SkirmishManager.Settings.Slots)
                            {
                                if (slot.StartPosition != 0)
                                {
                                    availablePositions.Remove(slot.StartPosition);
                                }
                            }

                            var random = new Random(_game.SkirmishManager.Settings.Seed);

                            foreach (var slot in _game.SkirmishManager.Settings.Slots)
                            {
                                // Close open slots.
                                if (slot.State == SkirmishSlotState.Open)
                                {
                                    slot.State = SkirmishSlotState.Closed;
                                }

                                if (slot.State != SkirmishSlotState.Closed)
                                {
                                    if (slot.StartPosition == 0)
                                    {
                                        slot.StartPosition = availablePositions.Last();
                                        availablePositions.Remove(slot.StartPosition);
                                    }

                                    if (slot.FactionIndex == 0)
                                    {
                                        slot.FactionIndex = (byte) random.Next(1, _game.GetPlayableSides().Count() + 1);
                                    }

                                    if (slot.ColorIndex == -1)
                                    {
                                        slot.ColorIndex = (sbyte) random.Next(slot.ColorIndex, _game.AssetStore.MultiplayerColors.Count);
                                    }
                                }
                            }

                            await context.Game.SkirmishManager.HandleStartButtonClickAsync();
                        }
                    }

                    break;
            }

            return false;
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

            var mapWindow = _window.Controls.FindControl(_optionsPath+ MapWindow);
            for (int i = 0; i < SkirmishGameSettings.MaxNumberOfPlayers; i++)
            {
                var startPositionButton = (Button) mapWindow.Controls[i];
                startPositionButton.Text = string.Empty;
                startPositionButton.Enabled = _game.SkirmishManager.Settings.LocalSlot?.Ready != true;
            }

            foreach (var slot in _game.SkirmishManager.Settings.Slots)
            {
                if (slot.StartPosition > 0)
                {
                    ((Button) mapWindow.Controls[slot.StartPosition - 1]).Text = (slot.Index + 1).ToString();
                }

                var colorCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxColorPrefix}{slot.Index}");
                if ((sbyte) colorCombo.Items[colorCombo.SelectedIndex].DataItem != slot.ColorIndex)
                    colorCombo.SelectedIndex = Array.FindIndex(colorCombo.Items, x => (sbyte) x.DataItem == slot.ColorIndex - 1);

                var teamCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxTeamPrefix}{slot.Index}");
                if ((sbyte) teamCombo.Items[teamCombo.SelectedIndex].DataItem != slot.Team)
                    teamCombo.SelectedIndex = Array.FindIndex(teamCombo.Items, x => (sbyte) x.DataItem == slot.Team - 1);

                var playerTemplateCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxPlayerTemplatePrefix}{slot.Index}");
                if (playerTemplateCombo.SelectedIndex != slot.FactionIndex)
                    playerTemplateCombo.SelectedIndex = slot.FactionIndex;

                var buttonAccept = (Button) window.Controls.FindControl($"{_optionsPath}{ButtonAcceptPrefix}{slot.Index}");
                var playerCombo = (ComboBox) window.Controls.FindControl($"{_optionsPath}{ComboBoxPlayerPrefix}{slot.Index}");

                var isLocalSlot = slot == _game.SkirmishManager.Settings.LocalSlot;
                // if there is no player or AI in the slot, the other boxes should be disabled
                var editable = (isLocalSlot && !slot.Ready) || (_game.SkirmishManager.IsHosting && slot.State is not SkirmishSlotState.Human and not SkirmishSlotState.Open and not SkirmishSlotState.Closed);

                // is null in singleplayer games for the local player
                if (playerCombo != null)
                {
                    playerCombo.Enabled = _game.SkirmishManager.IsHosting && !isLocalSlot;
                    playerCombo.Controls[0].Text = slot.State switch
                    {
                        SkirmishSlotState.Open => "GUI:Open".Translate(),
                        SkirmishSlotState.Closed => "GUI:Closed".Translate(),
                        SkirmishSlotState.EasyArmy => "GUI:EasyAI".Translate(),
                        SkirmishSlotState.MediumArmy => "GUI:MediumAI".Translate(),
                        SkirmishSlotState.HardArmy => "GUI:HardAI".Translate(),
                        SkirmishSlotState.Human => slot.PlayerName,
                        _ => throw new ArgumentException("invalid slot state: " + slot.State)
                    };
                }

                // only exists in multiplayer games
                if (buttonAccept != null)
                {
                    buttonAccept.Visible = slot.State == SkirmishSlotState.Human;
                    buttonAccept.Enabled = slot.Ready;
                }

                colorCombo.Enabled = editable;
                teamCombo.Enabled = editable;
                playerTemplateCombo.Enabled = editable;
            };

            var buttonStart = (Button) window.Controls.FindControl($"{_optionsPath}{ButtonStart}");
            buttonStart.Enabled = _game.SkirmishManager.IsStartButtonEnabled();
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

        private void FillComboBoxOptions(string key, string[] options, object[] dataItems = null, int selectedIndex = 0)
        {
            var comboBoxes = Control.GetSelfAndDescendants(_window).OfType<ComboBox>().Where(i => i.Name.StartsWith(key));
            foreach (var comboBox in comboBoxes)
            {
                if (comboBox.Name.Length - 1 != key.Length)
                {
                    continue;
                }
                var items = options.Select((o, i) => new ListBoxDataItem(dataItems?[i], new[] { o.Translate() }, comboBox.TextColor)).ToArray();
                comboBox.Items = items;
                comboBox.SelectedIndex = selectedIndex;
            }
        }

        private void FillColorComboBoxOptions(Tuple<sbyte, string, ColorRgbaF>[] options, int selectedIndex = 0)
        {
            var comboBoxes = Control.GetSelfAndDescendants(_window).OfType<ComboBox>().Where(i => i.Name.StartsWith(_optionsPath + ComboBoxColorPrefix));
            foreach (var comboBox in comboBoxes)
            {
                var items = options
                    .Select(i => new ListBoxDataItem(i.Item1, new[] { i.Item2.Translate() }, i.Item3))
                    .ToArray();
                comboBox.Items = items;
                comboBox.SelectedIndex = selectedIndex;
            }
        }

        public void SetCurrentMap(MapCache mapCache)
        {
            _game.SkirmishManager.Settings.MapName = mapCache.Name;

            Logger.Info("Set current map to " + mapCache.Name);

            CurrentMap = mapCache;

            var mapWindow = _window.Controls.FindControl(_optionsPath + MapWindow);

            MapUtils.SetMapPreview(mapCache, mapWindow, _game);

            // Set map text
            var textEntryMap = _window.Controls.FindControl(_optionsPath + ":TextEntryMapDisplay");
            var mapKey = mapCache.GetNameKey();

            textEntryMap.Text = mapKey.Translate();
        }
    }
}
