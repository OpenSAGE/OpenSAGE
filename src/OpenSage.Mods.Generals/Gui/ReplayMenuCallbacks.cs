using System.Collections.Generic;
using OpenSage.Data.Rep;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class ReplayMenuCallbacks
    {
        public static void ReplayMenuInit(Window window, Game game)
        {
            var listBox = (ListBox) window.Controls.FindControl("ReplayMenu.wnd:ListboxReplayFiles");

            var newItems = new List<ListBoxDataItem>();

            var fileSystem = game.ContentManager.FileSystem;
            foreach (var file in fileSystem.GetFiles("Replays"))
            {
                var replayFile = ReplayFile.FromFileSystemEntry(file, onlyHeader: true);

                newItems.Add(new ListBoxDataItem(
                    file.FilePath,
                    new[]
                    {
                        replayFile.Header.Filename, // Path.GetFileNameWithoutExtension(file.FilePath),
                        $"{replayFile.Header.Timestamp.Hour.ToString("D2")}:{replayFile.Header.Timestamp.Minute.ToString("D2")}",
                        replayFile.Header.Version,
                        replayFile.Header.Metadata.MapFile.Replace("maps/", string.Empty)
                    }));
            }

            listBox.Items = newItems.ToArray();
        }

        public static void ReplayMenuShutdown(Window window, Game game)
        {
            // TODO
        }

        public static void ReplayMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "ReplayMenu.wnd:ButtonLoadReplay":
                            // TODO: Handle no selected item.

                            var listBox = (ListBox) control.Window.Controls.FindControl("ReplayMenu.wnd:ListboxReplayFiles");

                            var replayFile =
                                context.Game.ContentManager.Load<ReplayFile>(
                                    (string) listBox.Items[listBox.SelectedIndex].DataItem);

                            // TODO: This probably isn't right.
                            var mapFilenameParts = replayFile.Header.Metadata.MapFile.Split('/');
                            var mapFilename = $"Maps\\{mapFilenameParts[1]}\\{mapFilenameParts[1]}.map";

                            context.Game.Scene2D.WndWindowManager.PopWindow();

                            context.Game.StartGame(
                                mapFilename,
                                new ReplayConnection(replayFile),
                                new[] { "America", "Observer" }, // TODO
                                0);

                            break;

                        case "ReplayMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Replay sub-menu
                            break;
                    }
                    break;
            }
        }
    }
}
