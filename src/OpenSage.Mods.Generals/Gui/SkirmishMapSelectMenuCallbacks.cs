using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

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

            var mapPath = mapCache.Name;
            var basePath = Path.GetDirectoryName(mapPath) + "\\" + Path.GetFileNameWithoutExtension(mapPath);
            var thumbPath = basePath + ".tga";

            // Set thumbnail
            mapPreview.BackgroundImage = _game.ContentManager.WndImageLoader.CreateFileImage(thumbPath);

            // Hide all start positions
            for (int i = 0; i < mapCache.NumPlayers; ++i)
            {
                _window.Controls.FindControl("SkirmishMapSelectMenu.wnd:ButtonMapStartPosition" + i.ToString());
            }

            // Set starting positions
            for (int i = 0; i < mapCache.NumPlayers; ++i)
            {
                var startPosCtrl = _window.Controls.FindControl("SkirmishMapSelectMenu.wnd:ButtonMapStartPosition" + i.ToString());
                startPosCtrl.BackgroundImage = _game.ContentManager.WndImageLoader.CreateNormalImage("PlayerStart");
                startPosCtrl.HoverBackgroundImage = _game.ContentManager.WndImageLoader.CreateNormalImage("PlayerStartHilite");
                startPosCtrl.DisabledBackgroundImage = _game.ContentManager.WndImageLoader.CreateNormalImage("PlayerStartDisabled");
                startPosCtrl.Enabled = false;
                startPosCtrl.Show();

                var relPos = MapUtils.GetRelativePosition(mapCache, i);

                var newPos = new Point2D((int) (relPos.X * mapPreview.Width) - 8, (int) ((1.0 - relPos.Y) * mapPreview.Height) - 8);

                startPosCtrl.Bounds = new Rectangle(newPos, new Size(16));
            }
        }
    }
}
