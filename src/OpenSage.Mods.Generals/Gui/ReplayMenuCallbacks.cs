using System.Collections.Generic;
using System.IO;
using OpenSage.Content.Translation;
using OpenSage.Data.Rep;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.IO;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class ReplayMenuCallbacks
    {
        private static DiskFileSystem GetReplaysFileSystem(Game game) => new(Path.Combine(game.UserDataFolder, "Replays"));

        public static void ReplayMenuInit(Window window, Game game)
        {
            var listBox = GetListBox(window);

            using var fileSystem = GetReplaysFileSystem(game);
            var newItems = new List<ListBoxDataItem>();

            foreach (var file in fileSystem.GetFilesInDirectory("", "*.rep"))
            {
                var replayFile = ReplayFile.FromFileSystemEntry(file, onlyHeader: true);

                newItems.Add(new ListBoxDataItem(
                    file.FilePath,
                    new[]
                    {
                        Path.GetFileNameWithoutExtension(file.FilePath),
                        $"{replayFile.Header.Timestamp.Hour:D2}:{replayFile.Header.Timestamp.Minute:D2}",
                        replayFile.Header.Version,
                        replayFile.Header.Metadata.MapFile.Replace("maps/", string.Empty)
                    },
                    ColorRgbaF.White));
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
                    var listBox = GetListBox(control.Window);
                    Control yesButton, noButton;
                    switch (message.Element.Name)
                    {
                        case "ReplayMenu.wnd:ButtonLoadReplay":
                            if (HandleNoFileSelected(listBox, context))
                            {
                                break;
                            }

                            using (var fileSystem = GetReplaysFileSystem(context.Game))
                            {
                                var replayFileEntry = fileSystem.GetFile((string) listBox.Items[listBox.SelectedIndex].DataItem);

                                context.Game.Scene2D.WndWindowManager.PopWindow();

                                context.Game.LoadReplayFile(replayFileEntry);
                            }

                            break;

                        case "ReplayMenu.wnd:ButtonDeleteReplay":
                            if (HandleNoFileSelected(listBox, context))
                            {
                                break;
                            }

                            context.Game.Scene2D.WndWindowManager.ShowDialogBox("GUI:DeleteFile".Translate(), "GUI:AreYouSureDelete".Translate(), out yesButton, out noButton);

                            yesButton.SystemCallback = (_, _, _) => {
                                using (var fileSystem = GetReplaysFileSystem(context.Game))
                                {
                                    var replayFileEntry = fileSystem.GetFile((string)listBox.Items[listBox.SelectedIndex].DataItem);
                                    File.Delete(fileSystem.GetFullPath(replayFileEntry));
                                    ReplayMenuInit(control.Window, control.Window.Game);
                                }

                                context.Game.Scene2D.WndWindowManager.PopWindow();
                            };

                            noButton.SystemCallback = (_, _, _) =>
                            {
                                context.Game.Scene2D.WndWindowManager.PopWindow();
                            };
                            break;

                        case "ReplayMenu.wnd:ButtonCopyReplay":
                            if (HandleNoFileSelected(listBox, context))
                            {
                                break;
                            }

                            context.Game.Scene2D.WndWindowManager.ShowDialogBox("GUI:CopyReplay".Translate(), "GUI:AreYouSureCopy".Translate(), out yesButton, out noButton);
                            yesButton.SystemCallback = (_, _, _) => {
                                using (var fileSystem = GetReplaysFileSystem(context.Game))
                                {
                                    var replayFileEntry = fileSystem.GetFile((string)listBox.Items[listBox.SelectedIndex].DataItem);
                                    using var outStream =
                                        File.OpenWrite(
                                            Path.Combine(
                                                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), replayFileEntry.FilePath));
                                    replayFileEntry.Open().CopyTo(outStream);
                                }

                                context.Game.Scene2D.WndWindowManager.PopWindow();
                            };

                            noButton.SystemCallback = (_, _, _) =>
                            {
                                context.Game.Scene2D.WndWindowManager.PopWindow();
                            };

                            break;

                        case "ReplayMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Replay sub-menu
                            //QueueTransition("MainMenuDefaultMenuBack");
                            //QueueTransition("MainMenuLoadReplayMenu");
                            break;
                    }
                    break;
            }
        }

        private static ListBox GetListBox(Window window) => (ListBox)window.Controls.FindControl("ReplayMenu.wnd:ListboxReplayFiles");

        private static bool HandleNoFileSelected(ListBox listBox, ControlCallbackContext context)
        {
            if (listBox.SelectedIndex != -1)
            {
                return false;
            }

            var title = "GUI:NoFileSelected";
            var text = "GUI:PleaseSelectAFile";

            context.Game.Scene2D.WndWindowManager.ShowMessageBox(title.Translate(), text.Translate());
            return true;
        }
    }
}
