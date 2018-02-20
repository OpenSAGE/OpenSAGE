using System.IO;
using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Rep;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Network;

namespace OpenSage.Mods.Generals
{
    internal static class WndCallbacks
    {
        private static bool _doneMainMenuFadeIn;

        public static void W3DMainMenuInit(WndTopLevelWindow window, Game game)
        {
            // We'll show these later via window transitions.
            window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Hide();
            window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Opacity = 0;

            var initiallyHiddenSections = new[]
            {
                "MainMenu.wnd:MapBorder",
                "MainMenu.wnd:MapBorder1",
                "MainMenu.wnd:MapBorder2",
                "MainMenu.wnd:MapBorder3",
                "MainMenu.wnd:MapBorder4"
            };

            foreach (var controlName in initiallyHiddenSections)
            {
                var control = window.Root.FindChild(controlName);
                control.Opacity = 0;

                foreach (var button in control.Children.First().Children)
                {
                    button.Opacity = 0;
                    button.TextOpacity = 0;
                }
            }

            window.Root.FindChild("MainMenu.wnd:ButtonUSARecentSave").Hide();
            window.Root.FindChild("MainMenu.wnd:ButtonUSALoadGame").Hide();

            window.Root.FindChild("MainMenu.wnd:ButtonGLARecentSave").Hide();
            window.Root.FindChild("MainMenu.wnd:ButtonGLALoadGame").Hide();

            window.Root.FindChild("MainMenu.wnd:ButtonChinaRecentSave").Hide();
            window.Root.FindChild("MainMenu.wnd:ButtonChinaLoadGame").Hide();

            // TODO: Show faction icons when WinScaleUpTransition is implemented.

            _doneMainMenuFadeIn = false;
        }

        public static void W3DNoDraw(WndWindow element, Game game)
        {
            
        }

        public static void MainMenuSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            void QueueTransition(string transition)
            {
                context.WindowManager.TransitionManager.QueueTransition(null, element.Window, transition);
            }

            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "MainMenu.wnd:ButtonSinglePlayer":
                            QueueTransition("MainMenuDefaultMenuBack");
                            QueueTransition("MainMenuSinglePlayerMenu");
                            break;

                        case "MainMenu.wnd:ButtonSingleBack":
                            QueueTransition("MainMenuSinglePlayerMenuBack");
                            QueueTransition("MainMenuDefaultMenu");
                            break;

                        case "MainMenu.wnd:ButtonMultiplayer":
                            QueueTransition("MainMenuDefaultMenuBack");
                            QueueTransition("MainMenuMultiPlayerMenu");
                            break;

                        case "MainMenu.wnd:ButtonMultiBack":
                            QueueTransition("MainMenuMultiPlayerMenuReverse");
                            QueueTransition("MainMenuDefaultMenu");
                            break;

                        case "MainMenu.wnd:ButtonLoadReplay":
                            QueueTransition("MainMenuDefaultMenuBack");
                            QueueTransition("MainMenuLoadReplayMenu");
                            break;

                        case "MainMenu.wnd:ButtonLoadReplayBack":
                            QueueTransition("MainMenuLoadReplayMenuBack");
                            QueueTransition("MainMenuDefaultMenu");
                            break;

                        case "MainMenu.wnd:ButtonReplay":
                            context.WindowManager.SetWindow(@"Menus\ReplayMenu.wnd");
                            break;

                        case "MainMenu.wnd:ButtonOptions":
                            context.WindowManager.PushWindow(@"Menus\OptionsMenu.wnd");
                            break;

                        case "MainMenu.wnd:ButtonExit":
                            var exitWindow = context.WindowManager.PushWindow(@"Menus\QuitMessageBox.wnd");
                            exitWindow.Root.FindChild("QuitMessageBox.wnd:StaticTextTitle").Text = "EXIT?";
                            exitWindow.Root.FindChild("QuitMessageBox.wnd:StaticTextTitle").TextAlignment = TextAlignment.Leading;
                            exitWindow.Root.FindChild("QuitMessageBox.wnd:StaticTextMessage").Text = "Are you sure you want to exit?";
                            exitWindow.Root.FindChild("QuitMessageBox.wnd:ButtonOk").Show();
                            exitWindow.Root.FindChild("QuitMessageBox.wnd:ButtonOk").Text = "YES";
                            exitWindow.Root.FindChild("QuitMessageBox.wnd:ButtonCancel").Show();
                            exitWindow.Root.FindChild("QuitMessageBox.wnd:ButtonCancel").Text = "NO";
                            break;
                    }
                    break;
            }
        }

        public static void MainMenuInput(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            // Any input at all (mouse, keyboard) will trigger the main menu fade-in.
            if (!_doneMainMenuFadeIn)
            {
                context.WindowManager.TransitionManager.QueueTransition(null, element.Window, "MainMenuFade");
                context.WindowManager.TransitionManager.QueueTransition(null, element.Window, "MainMenuDefaultMenu");
                element.Window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Show();
                _doneMainMenuFadeIn = true;
            }
        }

        public static void QuitMessageBoxSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "QuitMessageBox.wnd:ButtonCancel":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }

        public static void OptionsMenuSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "OptionsMenu.wnd:ButtonBack":
                            context.WindowManager.PopWindow();
                            break;
                    }
                    break;
            }
        }

        private static FileSystem GetReplaysFileSystem(Game game) => new FileSystem(Path.Combine(game.UserDataFolder, "Replays"));

        public static void ReplayMenuInit(WndTopLevelWindow window, Game game)
        {
            var listBox = (WndWindowListBox) window.Root.FindChild("ReplayMenu.wnd:ListboxReplayFiles");

            using (var fileSystem = GetReplaysFileSystem(game))
            {
                listBox.ListBoxItems.Clear();

                foreach (var file in fileSystem.Files)
                {
                    var replayFile = ReplayFile.FromFileSystemEntry(file, onlyHeader: true);

                    listBox.ListBoxItems.Add(new WndListBoxItem
                    {
                        DataItem = file.FilePath,
                        ColumnData = new[]
                        {
                            replayFile.Header.Filename, // Path.GetFileNameWithoutExtension(file.FilePath),
                            $"{replayFile.Header.Timestamp.Hour.ToString("D2")}:{replayFile.Header.Timestamp.Minute.ToString("D2")}",
                            replayFile.Header.Version,
                            replayFile.Header.Metadata.MapFile.Replace("maps/", string.Empty)
                        }
                    });
                }
            }
        }

        public static void ReplayMenuShutdown(WndTopLevelWindow window, Game game)
        {
            // TODO
        }

        public static void ReplayMenuSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "ReplayMenu.wnd:ButtonLoadReplay":
                            // TODO: Handle no selected item.
                            var listBox = (WndWindowListBox) element.Window.Root.FindChild("ReplayMenu.wnd:ListboxReplayFiles");
                            ReplayFile replayFile;
                            using (var fileSystem = GetReplaysFileSystem(context.Game))
                            {
                                var replayFileEntry = fileSystem.GetFile((string) listBox.ListBoxItems[listBox.SelectedIndex].DataItem);
                                replayFile = ReplayFile.FromFileSystemEntry(replayFileEntry);
                            }

                            // TODO: This probably isn't right.
                            var mapFilenameParts = replayFile.Header.Metadata.MapFile.Split('/');
                            var mapFilename = $"Maps\\{mapFilenameParts[1]}\\{mapFilenameParts[1]}.map";

                            // TODO: Loading screen.
                            context.Game.Scene3D = context.Game.ContentManager.Load<Scene3D>(mapFilename);
                            context.Game.NetworkMessageBuffer = new NetworkMessageBuffer(
                                context.Game,
                                new ReplayConnection(replayFile));

                            context.Game.Scene2D.WndWindowManager.PopWindow();

                            break;

                        case "ReplayMenu.wnd:ButtonBack":
                            context.WindowManager.SetWindow(@"Menus\MainMenu.wnd");
                            // TODO: Go back to Replay sub-menu
                            break;
                    }
                    break;
            }
        }

        public static void PassSelectedButtonsToParentSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            if (message.MessageType != WndWindowMessageType.SelectedButton)
            {
                return;
            }

            element.Parent.SystemCallback.Invoke(element.Parent, message, context);
        }

        public static void PassMessagesToParentSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            element.Parent.SystemCallback.Invoke(element.Parent, message, context);
        }
    }
}
