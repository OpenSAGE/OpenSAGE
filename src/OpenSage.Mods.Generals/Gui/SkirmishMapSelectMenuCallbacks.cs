using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using Veldrid;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class SkirmishMapSelectMenuCallbacks
    {
        private const string ListBoxMapPrefix = "SkirmishMapSelectMenu.wnd:ListboxMap";

        private static MapCache _previewMap;
        private static Game _game;

        public static void SkirmishMapSelectMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishMapSelectMenu.wnd:ButtonBack":
                            SkirmishGameOptionsMenuCallbacks.GameOptions.CloseMapSelection(context);
                            break;
                        case "SkirmishMapSelectMenu.wnd:ButtonOK":
                            SkirmishGameOptionsMenuCallbacks.GameOptions.SetCurrentMap(_previewMap);
                            SkirmishGameOptionsMenuCallbacks.GameOptions.CloseMapSelection(context);
                            break;
                    }
                    break;
            }
        }

        public static void SkirmishMapSelectMenuInit(Window window, Game game)
        {
            _game = game;

            void SetPreviewMap(MapCache mapCache)
            {
                _previewMap = mapCache;

                var mapPreview = window.Controls.FindControl("SkirmishMapSelectMenu.wnd:WinMapPreview");

                MapUtils.SetMapPreview(mapCache, mapPreview, game);
            }

            SetPreviewMap(SkirmishGameOptionsMenuCallbacks.GameOptions.CurrentMap);

            // Official maps
            var mapCaches = game.AssetStore.MapCaches;
            var listBoxMaps = (ListBox) window.Controls.FindControl(ListBoxMapPrefix);
            var items = new List<ListBoxDataItem>();

            foreach (var mapCache in mapCaches)
            {
                if (mapCache.IsMultiplayer)
                {
                    items.Add(new ListBoxDataItem(mapCache, new[] { "", mapCache.GetNameKey().Translate() }, listBoxMaps.TextColor));
                }
            }

            listBoxMaps.Items = items.ToArray();

            listBoxMaps.SelectedIndexChanged += (sender, e) =>
            {
                var selectedItem = listBoxMaps.Items[listBoxMaps.SelectedIndex];

                var mapCache = selectedItem.DataItem as MapCache;

                SetPreviewMap(mapCache);
            };
        }

        // This is also used by LanMapSelectMenu.
        public static void W3DDrawMapPreview(Control control, DrawingContext2D drawingContext)
        {
            if (!control.Data.TryGetValue("MapPreview", out var mapPreviewTexture))
            {
                return;
            }

            drawingContext.DrawImage(
                (Texture) mapPreviewTexture,
                null,
                control.ClientRectangle.ToRectangleF());
        }
    }
}
