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

        private static string _currentSide;
        private static string _currentSideWindowSuffix;

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

            void OpenSinglePlayerSideMenu(string side, string sideWindowSuffix)
            {
                _currentSide = side;
                _currentSideWindowSuffix = sideWindowSuffix;

                var selectDifficultyLabel = (Label) control.Window.Controls.FindControl("MainMenu.wnd:StaticTextSelectDifficulty");
                // TODO: This should be animated as part of the transition.
                selectDifficultyLabel.Opacity = 1;
                selectDifficultyLabel.TextOpacity = 1;
                selectDifficultyLabel.Show();

                QueueTransition("MainMenuSinglePlayerMenuBack");
                QueueTransition($"MainMenuDifficultyMenu{sideWindowSuffix}");
            }

            var translation = context.Game.ContentManager.TranslationManager;

            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "MainMenu.wnd:ButtonSinglePlayer":
                            QueueTransition("MainMenuDefaultMenuBack");
                            QueueTransition("MainMenuSinglePlayerMenu");
                            break;

                        case "MainMenu.wnd:ButtonTRAINING":
                            OpenSinglePlayerSideMenu("TRAINING", "Training");
                            break;

                        case "MainMenu.wnd:ButtonChina":
                            OpenSinglePlayerSideMenu("China", "China");
                            break;

                        case "MainMenu.wnd:ButtonGLA":
                            OpenSinglePlayerSideMenu("GLA", "GLA");
                            break;

                        case "MainMenu.wnd:ButtonUSA":
                            OpenSinglePlayerSideMenu("USA", "US");
                            break;

                        case "MainMenu.wnd:ButtonEasy":
                        case "MainMenu.wnd:ButtonMedium":
                        case "MainMenu.wnd:ButtonHard":
                            context.Game.StartCampaign(_currentSide);
                            break;

                        case "MainMenu.wnd:ButtonDiffBack":
                            QueueTransition($"MainMenuDifficultyMenu{_currentSideWindowSuffix}Back");
                            QueueTransition($"MainMenuSinglePlayer{_currentSideWindowSuffix}MenuFromDiff");
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

                        case "MainMenu.wnd:ButtonNetwork":
                            context.WindowManager.SetWindow(@"Menus\LanLobbyMenu.wnd");
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
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextTitle").Text = translation.Lookup("GUI:QuitPopupTitle");
                            ((Label) exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextTitle")).TextAlignment = TextAlignment.Leading;
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:StaticTextMessage").Text = translation.Lookup("GUI:QuitPopupMessage");
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonOk").Show();
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonOk").Text = translation.Lookup("GUI:Yes");
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonCancel").Show();
                            exitWindow.Controls.FindControl("QuitMessageBox.wnd:ButtonCancel").Text = translation.Lookup("GUI:No");
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
