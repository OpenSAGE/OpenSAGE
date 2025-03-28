using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui;

[WndCallbacks]
public static class LanMapSelectMenuCallbacks
{
    private const string ListBoxMapPrefix = "LanMapSelectMenu.wnd:ListboxMap";

    private static Window Window;
    private static IGame Game;
    private static MapCache PreviewMap;

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
                        LanGameOptionsMenuCallbacks.GameOptions.SetCurrentMap(PreviewMap);
                        LanGameOptionsMenuCallbacks.GameOptions.CloseMapSelection(context);
                        break;
                }
                break;
        }
    }

    public static void LanMapSelectMenuInit(Window window, IGame game)
    {
        Window = window;
        Game = game;
        SetPreviewMap(LanGameOptionsMenuCallbacks.GameOptions.CurrentMap);

        // Official maps
        var mapCaches = Game.AssetStore.MapCaches;
        var listBoxMaps = (ListBox)Window.Controls.FindControl(ListBoxMapPrefix);
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
        var listBoxMaps = (ListBox)Window.Controls.FindControl(ListBoxMapPrefix);
        var selectedItem = listBoxMaps.Items[listBoxMaps.SelectedIndex];

        var mapCache = selectedItem.DataItem as MapCache;

        SetPreviewMap(mapCache);
    }

    internal static void SetPreviewMap(MapCache mapCache)
    {
        PreviewMap = mapCache;

        var mapPreview = Window.Controls.FindControl("LanMapSelectMenu.wnd:WinMapPreview");

        MapUtils.SetMapPreview(mapCache, mapPreview, Game);
    }
}
