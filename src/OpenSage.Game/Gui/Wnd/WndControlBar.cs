using OpenSage.Content;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndControlBar
    {
        private readonly Window _window;

        private readonly Label _moneyDisplay;
        private readonly Control _powerBar;
        private readonly Control _expBar;

        private WndControlBar(Window window, Label moneyDisplay, Control powerBar, Control expBar)
        {
            _window = window;
            _moneyDisplay = moneyDisplay;
            _powerBar = powerBar;
            _expBar = expBar;
        }

        public void UpdateState(Player player)
        {
            _moneyDisplay.Text = $"$ {player.Money}";
        }

        public static WndControlBar Create(string side, WndWindowManager windowManager, ContentManager contentManager)
        {
            var scheme = contentManager.IniDataContext.ControlBarSchemes.FindBySide(side);

            var background = new Control();
            background.Name = "OpenSAGE:ControlBarBackground";

            // TODO: Support multiple image parts?
            var imagePart = scheme.ImageParts[0];
            background.Bounds = new Rectangle(imagePart.Position, imagePart.Size);
            background.BackgroundImage = CreateImage(imagePart.ImageName);

            var backgroundWindow = new Window(scheme.ScreenCreationRes, background, contentManager);
            windowManager.PushWindow(backgroundWindow);

            var controlBarWindow = windowManager.PushWindow("ControlBar.wnd");

            Control FindControl(string name) => controlBarWindow.Controls.FindControl($"ControlBar.wnd:{name}");
            Image CreateImage(string path) => contentManager.WndImageLoader.CreateNormalImage(path);

            var center = FindControl("CenterBackground");

            foreach (var control in center.Controls)
            {
                control.Hide();
            }

            // TODO: Implement under attack indicator.
            FindControl("WinUAttack").Hide();

            // TODO: What is this?
            FindControl("OnTopDraw").Hide();

            var windowOrigin = controlBarWindow.Bounds.Location;
            var schemeType = scheme.GetType();

            Control ApplyBounds(string name, string coordPrefix)
            {
                var control = FindControl(name);

                var ul = (Point2D) schemeType.GetProperty($"{coordPrefix}UL").GetValue(scheme);
                var lr = (Point2D) schemeType.GetProperty($"{coordPrefix}LR").GetValue(scheme);
                control.Bounds = Rectangle.FromCorners(ul - windowOrigin, lr - windowOrigin);

                return control;
            }

            Button ApplyButtonScheme(string name, string coordPrefix, string texturePrefix)
            {
                var button = ApplyBounds(name, coordPrefix) as Button;

                Image LoadImage(string state) =>
                    CreateImage(
                        (string) schemeType.GetProperty($"{texturePrefix}{state}")?.GetValue(scheme));

                button.BackgroundImage = LoadImage("Enable");
                button.DisabledBackgroundImage = LoadImage("Disabled");
                button.HoverBackgroundImage = LoadImage("Highlighted");
                button.PushedBackgroundImage = LoadImage("Pushed");

                return button;
            }

            var money = ApplyBounds("MoneyDisplay", "Money") as Label;
            money.Text = "$ 0";

            var power = ApplyBounds("PowerWindow", "PowerBar");

            ApplyButtonScheme("ButtonOptions", "Options", "OptionsButton");
            ApplyButtonScheme("ButtonGeneral", "General", "GeneralButton");
            ApplyButtonScheme("ButtonPlaceBeacon", "Beacon", "BeaconButton");
            ApplyButtonScheme("PopupCommunicator", "Chat", "BuddyButton");

            ApplyButtonScheme("ButtonIdleWorker", "Worker", "IdleWorkerButton");
            ApplyButtonScheme("ButtonLarge", "MinMax", "MinMaxButton");

            var rightHud = FindControl("RightHUD");
            rightHud.BorderWidth = 0;
            rightHud.BackgroundColor = ColorRgbaF.Transparent;
            rightHud.BackgroundImage = CreateImage(scheme.RightHudImage);

            foreach (var control in rightHud.Controls)
            {
                control.Hide();
            }

            var expBar = FindControl("GeneralsExp");
            var expBarForeground = FindControl("ExpBarForeground");
            expBarForeground.BackgroundImage = CreateImage(scheme.ExpBarForegroundImage);

            return new WndControlBar(controlBarWindow, money, power, expBar);
        }
    }
}
