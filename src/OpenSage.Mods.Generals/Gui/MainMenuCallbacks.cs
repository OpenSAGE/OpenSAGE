using System.Linq;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Mods.Generals.Gui
{
    [WndCallbacks]
    public static class MainMenuCallbacks
    {
        private static bool _doneMainMenuFadeIn;

        public static void W3DMainMenuInit(Window window, Game game)
        {
            if (!game.Configuration.LoadShellMap)
            {
                // Draw the main menu background if no map is loaded.
                window.Root.DrawCallback = window.Root.DefaultDraw;
            }

            // We'll show these later via window transitions.
            window.Controls.FindControl("MainMenu.wnd:MainMenuRuler").Hide();
            window.Controls.FindControl("MainMenu.wnd:MainMenuRuler").Opacity = 0;

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
                var control = window.Controls.FindControl(controlName);
                control.Opacity = 0;

                foreach (var button in control.Controls.First().Controls)
                {
                    button.Opacity = 0;
                    button.TextOpacity = 0;
                }
            }

            window.Controls.FindControl("MainMenu.wnd:ButtonUSARecentSave").Hide();
            window.Controls.FindControl("MainMenu.wnd:ButtonUSALoadGame").Hide();

            window.Controls.FindControl("MainMenu.wnd:ButtonGLARecentSave").Hide();
            window.Controls.FindControl("MainMenu.wnd:ButtonGLALoadGame").Hide();

            window.Controls.FindControl("MainMenu.wnd:ButtonChinaRecentSave").Hide();
            window.Controls.FindControl("MainMenu.wnd:ButtonChinaLoadGame").Hide();

            // TODO: Show faction icons when WinScaleUpTransition is implemented.

            _doneMainMenuFadeIn = false;
        }

        public static void MainMenuSystem(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            void QueueTransition(string transition)
            {
                context.WindowManager.TransitionManager.QueueTransition(null, control.Window, transition);
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

                        case "MainMenu.wnd:ButtonSkirmish":
                            context.WindowManager.SetWindow(@"Menus\SkirmishGameOptionsMenu.wnd");
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

                        case "MainMenu.wnd:ButtonCredits":
                            context.WindowManager.SetWindow(@"Menus\CreditsMenu.wnd");
                            break;

                        case "MainMenu.wnd:ButtonExit":
                            var exitWindow = context.WindowManager.PushWindow(@"Menus\QuitMessageBox.wnd");
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextTitle").Text = "EXIT?";
                            ((Label) exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextTitle")).TextAlignment = TextAlignment.Leading;
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextMessage").Text = "Are you sure you want to exit?";
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonOk").Show();
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonOk").Text = "YES";
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonCancel").Show();
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonCancel").Text = "NO";
                            break;
                    }
                    break;
            }
        }

        public static void MainMenuInput(Control control, WndWindowMessage message, ControlCallbackContext context)
        {
            // Any input at all (mouse, keyboard) will trigger the main menu fade-in.
            if (!_doneMainMenuFadeIn)
            {
                context.WindowManager.TransitionManager.QueueTransition(null, control.Window, "MainMenuFade");
                context.WindowManager.TransitionManager.QueueTransition(null, control.Window, "MainMenuDefaultMenu");
                control.Window.Controls.FindControl("MainMenu.wnd:MainMenuRuler").Show();
                _doneMainMenuFadeIn = true;
            }
        }
    }
}
