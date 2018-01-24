using OpenSage.Gui;
using OpenSage.Gui.Wnd;

namespace OpenSage.Mods.Generals
{
    internal static class WndCallbacks
    {
        private static bool _doneMainMenuFadeIn;

        public static void W3DMainMenuInit(WndTopLevelWindow window)
        {
            // We'll show these later via window transitions.
            window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Hide();
            window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Opacity = 0;

            window.Root.FindChild("MainMenu.wnd:MapBorder2").Opacity = 0;
            foreach (var button in window.Root.FindChild("MainMenu.wnd:EarthMap2").Children)
            {
                button.Opacity = 0;
                button.TextOpacity = 0;
            }

            window.Root.FindChild("MainMenu.wnd:MapBorder").Hide();
            window.Root.FindChild("MainMenu.wnd:MapBorder1").Hide();
            window.Root.FindChild("MainMenu.wnd:MapBorder3").Hide();
            window.Root.FindChild("MainMenu.wnd:MapBorder4").Hide();

            window.Root.FindChild("MainMenu.wnd:ButtonUSARecentSave").Hide();
            window.Root.FindChild("MainMenu.wnd:ButtonUSALoadGame").Hide();

            window.Root.FindChild("MainMenu.wnd:ButtonGLARecentSave").Hide();
            window.Root.FindChild("MainMenu.wnd:ButtonGLALoadGame").Hide();

            window.Root.FindChild("MainMenu.wnd:ButtonChinaRecentSave").Hide();
            window.Root.FindChild("MainMenu.wnd:ButtonChinaLoadGame").Hide();

            _doneMainMenuFadeIn = false;
        }

        public static void W3DNoDraw(WndWindow element, Game game)
        {
            
        }

        public static void MainMenuSystem(WndWindow element, WndWindowMessage message, UIElementCallbackContext context)
        {
            switch (message.MessageType)
            {
                case WndWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
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
