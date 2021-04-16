using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class LanMapSelectMenuCallbacks
    {
        private const string ListBoxMapPrefix = "LanMapSelectMenu.wnd:ListboxMap";

        private static Window _window;
        private static Game _game;
        private static MapCache _previewMap;

        public static void LanMapSelectMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "LanMapSelectMenu.wnd:ButtonBack":
                            LanGameOptionsMenuCallbacks.GameOptions.CloseMapSelection(context);
                            break;
                        case "LanMapSelectMenu.wnd:ButtonOK":
                            LanGameOptionsMenuCallbacks.GameOptions.SetCurrentMap(_previewMap);
                            LanGameOptionsMenuCallbacks.GameOptions.CloseMapSelection(context);
                            break;
                    }
                    break;
            }
        }

        public static void LanMapSelectMenuInit(Window window, Game game)
        {
            _window = window;
            _game = game;
            SetPreviewMap(LanGameOptionsMenuCallbacks.GameOptions.CurrentMap);

            // Official maps
            var mapCaches = _game.AssetStore.MapCaches;
            var listBoxMaps = (ListBox) _window.Controls.FindControl(ListBoxMapPrefix);
            var items = new List<ListBoxDataItem>();

            foreach (var mapCache in mapCaches)
            {
                if (mapCache.IsMultiplayer)
                {
                    items.Add(new ListBoxDataItem(mapCache, new[] { mapCache.GetNameKey().Translate() }, listBoxMaps.TextColor));
                }
            }

            listBoxMaps.Items = items.ToArray();

            listBoxMaps.SelectedIndexChanged += OnItemChanged;
        }

        private static void OnItemChanged(object sender, EventArgs e)
        {
            var listBoxMaps = (ListBox) _window.Controls.FindControl(ListBoxMapPrefix);
            var selectedItem = listBoxMaps.Items[listBoxMaps.SelectedIndex];

            var mapCache = selectedItem.DataItem as MapCache;

            SetPreviewMap(mapCache);
        }

        internal static void SetPreviewMap(MapCache mapCache)
        {
            _previewMap = mapCache;

            var mapPreview = _window.Controls.FindControl("LanMapSelectMenu.wnd:WinMapPreview");

            MapUtils.SetMapPreview(mapCache, mapPreview, _game);
        }
    }
}
