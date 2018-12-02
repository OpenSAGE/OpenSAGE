using System.Collections.Generic;
using System.IO;
using OpenSage.Data;
using OpenSage.Data.Rep;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;
using OpenSage.Network;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class ReplayMenuCallbacks
    {
        private static FileSystem GetReplaysFileSystem(Game game) => new FileSystem(Path.Combine(game.UserDataFolder, "Replays"));

        public static void ReplayMenuInit(Window window, Game game)
        {
            var listBox = (ListBox) window.Controls.FindControl("ReplayMenu.wnd:ListboxReplayFiles");

            using (var fileSystem = GetReplaysFileSystem(game))
            {
                var newItems = new List<ListBoxDataItem>();

                foreach (var file in fileSystem.Files)
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
                        },
                        ColorRgbaF.White));
                }

                listBox.Items = newItems.ToArray();
            }
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
                            ReplayFile replayFile;
                            using (var fileSystem = GetReplaysFileSystem(context.Game))
                            {
                                var replayFileEntry = fileSystem.GetFile((string) listBox.Items[listBox.SelectedIndex].DataItem);
                                replayFile = ReplayFile.FromFileSystemEntry(replayFileEntry);
                            }

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
