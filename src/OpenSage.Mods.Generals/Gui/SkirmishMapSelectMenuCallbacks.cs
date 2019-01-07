using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class SkirmishMapSelectMenuCallbacks
    {
        private const string ListBoxMapPrefix = "SkirmishMapSelectMenu.wnd:ListboxMap";

        private static Window _window;
        private static Game _game;
        private static List<FileSystemEntry> _maps;
        private static List<FileSystemEntry> _thumbnails;
        private static MapCache _previewMap;

        public static void SkirmishMapSelectMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishMapSelectMenu.wnd:ButtonBack":
                            SkirmishGameOptionsMenuCallbacks.CloseMapSelection(context);
                            break;
                        case "SkirmishMapSelectMenu.wnd:ButtonOK":
                            SkirmishGameOptionsMenuCallbacks.SetCurrentMap(_previewMap);
                            SkirmishGameOptionsMenuCallbacks.CloseMapSelection(context);
                            break;
                    }
                    break;
            }
        }

        public static void SkirmishMapSelectMenuInit(Window window, Game game)
        {
            _window = window;
            _game = game;

            //Official maps
            var mapCaches = _game.ContentManager.IniDataContext.MapCaches;
            var listBoxMaps = (ListBox) _window.Controls.FindControl(ListBoxMapPrefix);
            var items = new List<ListBoxDataItem>();

            foreach (var cache in mapCaches)
            {
                if (cache.IsMultiplayer)
                {
                    items.Add(new ListBoxDataItem(listBoxMaps, new[] { "", _game.ContentManager.TranslationManager.Lookup(cache.DisplayName) }));
                }
            }

            listBoxMaps.Items = items.ToArray();

            listBoxMaps.SelectedIndexChanged += OnItemChanged;
        }

        private static void OnItemChanged(object sender, EventArgs e)
        {
            var listBoxMaps = (ListBox) _window.Controls.FindControl(ListBoxMapPrefix);
            var selectedItem = listBoxMaps.Items[listBoxMaps.SelectedIndex];
            var mapName = selectedItem.ColumnData[1];

            var mapCache = _game.ContentManager.IniDataContext.MapCaches.First(x => x.DisplayName == mapName);

            SetPreviewMap(mapCache);
        }

        public static void SetPreviewMap(MapCache mapCache)
        {
            _previewMap = mapCache;

            var mapPreview = _window.Controls.FindControl("SkirmishMapSelectMenu.wnd:WinMapPreview");

            MapUtils.SetMapPreview(mapCache, mapPreview, _game);
        }
    }
}
