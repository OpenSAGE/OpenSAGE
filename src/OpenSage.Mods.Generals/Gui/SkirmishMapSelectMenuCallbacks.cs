using System.Collections.Generic;
using OpenSage.Data;
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

        public static void SkirmishMapSelectMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SkirmishMapSelectMenu.wnd:ButtonBack":
                            context.WindowManager.PopWindow();
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
                    items.Add(new ListBoxDataItem(listBoxMaps, new[] { _game.ContentManager.TranslationManager.Lookup(cache.DisplayName) }));
                }
            }

            listBoxMaps.Items = items.ToArray();
        }
    }
}
