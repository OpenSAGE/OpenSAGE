using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Sav;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.IO;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class SaveLoadCallbacks
    {
        private static FileSystem GetSavesFileSystem(Game game) => new DiskFileSystem(Path.Combine(game.UserDataFolder, "Save"));

        public static void SaveLoadMenuFullScreenInit(Window window, Game game)
        {
            var listBox = (ListBox) window.Controls.FindControl("SaveLoad.wnd:ListboxGames");

            using (var fileSystem = GetSavesFileSystem(game))
            {
                var newItems = new List<ListBoxDataItem>();

                foreach (var file in fileSystem.GetFilesInDirectory("", "*.sav"))
                {
                    var saveGameState = SaveFile.GetGameState(file, game);

                    newItems.Add(new ListBoxDataItem(
                        file.FilePath,
                        new[]
                        {
                            saveGameState.DisplayName,
                            saveGameState.Timestamp.ToShortTimeString(),
                            saveGameState.Timestamp.ToShortDateString()
                        },
                        ColorRgbaF.White));
                }

                listBox.Items = newItems.ToArray();
            }
        }

        public static void SaveLoadMenuShutdown(Window window, Game game)
        {
            // TODO
        }

        public static void SaveLoadMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "SaveLoad.wnd:ButtonLoad":
                            // TODO: Handle no selected item.

                            var listBox = (ListBox) control.Window.Controls.FindControl("SaveLoad.wnd:ListboxGames");
                            using (var fileSystem = GetSavesFileSystem(context.Game))
                            {
                                var saveFileEntry = fileSystem.GetFile((string) listBox.Items[listBox.SelectedIndex].DataItem);

                                context.Game.Scene2D.WndWindowManager.PopWindow();

                                context.Game.LoadSaveFile(saveFileEntry);
                            }

                            break;

                        case "SaveLoad.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Load sub-menu
                            break;
                    }
                    break;
            }
        }
    }
}
