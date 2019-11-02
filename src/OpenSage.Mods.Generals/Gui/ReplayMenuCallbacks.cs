using System.Collections.Generic;
using System.IO;
using OpenSage.Data.IO;
using OpenSage.Data.Rep;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class ReplayMenuCallbacks
    {
        private static IFileProvider GetReplaysFileSystem(Game game) => new FileProvider("/replays", Path.Combine(game.UserDataFolder, "Replays"));

        public static void ReplayMenuInit(Window window, Game game)
        {
            var listBox = (ListBox) window.Controls.FindControl("ReplayMenu.wnd:ListboxReplayFiles");

            using (var fileSystem = GetReplaysFileSystem(game))
            {
                var newItems = new List<ListBoxDataItem>();

                foreach (var file in FileSystem.ListFiles("/replays/", "*", Data.IO.SearchOption.AllDirectories))
                {
                    var replayFile = ReplayFile.FromUrl(file, onlyHeader: true);

                    newItems.Add(new ListBoxDataItem(
                        file,
                        new[]
                        {
                            FileSystem.GetFileNameWithoutExtension(file),
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
                            using (var fileSystem = GetReplaysFileSystem(context.Game))
                            {
                                var replayFileEntry = (string) listBox.Items[listBox.SelectedIndex].DataItem;

                                context.Game.Scene2D.WndWindowManager.PopWindow();

                                context.Game.LoadReplayFile(replayFileEntry);
                            }

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
