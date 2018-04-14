using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    public sealed class WndControlBar
    {
        private enum ControlBarSize
        {
            Maximized,
            Minimized
        }

        // How much the control bar should be moved down when minimized?
        private const int MinimizeOffset = 120;

        private readonly ContentManager _contentManager;

        private readonly ControlBarScheme _scheme;

        private ControlBarState _state;

        private ControlBarState State
        {
            get => _state;
            set
            {
                _state = value;
                _state.OnEnterState(this);
            }
        }

        private readonly Window _background;
        private readonly Window _window;

        private readonly Control _center;
        private readonly Control _right;

        private readonly Label _moneyDisplay;
        // TODO: Change this to a ProgressBar when they are implemented.
        private readonly Control _powerBar;
        // TODO: Change this to a ProgressBar when they are implemented.
        private readonly Control _expBar;

        private readonly Button _resize;

        private readonly Image _resizeDownBackground;
        private readonly Image _resizeDownHover;
        private readonly Image _resizeDownPushed;

        private readonly Image _resizeUpBackground;
        private readonly Image _resizeUpHover;
        private readonly Image _resizeUpPushed;

        private ControlBarSize _size = ControlBarSize.Maximized;

        private Control FindControl(string name) => _window.Controls.FindControl($"ControlBar.wnd:{name}");
        private Image LoadImage(string path) => _contentManager.WndImageLoader.CreateNormalImage(path);

        private WndControlBar(Window background, Window window, ControlBarScheme scheme, ContentManager contentManager)
        {
            _background = background;
            _window = window;
            _scheme = scheme;
            _contentManager = contentManager;

            _center = FindControl("CenterBackground");
            _right = FindControl("RightHUD");

            _moneyDisplay = FindControl("MoneyDisplay") as Label;
            _moneyDisplay.Text = "$ 0";
            _powerBar = FindControl("PowerWindow");
            _expBar = FindControl("GeneralsExp");

            _resize = FindControl("ButtonLarge") as Button;

            _resizeDownBackground = LoadImage(_scheme.ToggleButtonDownOn);
            _resizeDownHover = LoadImage(_scheme.ToggleButtonDownIn);
            _resizeDownPushed = LoadImage(_scheme.ToggleButtonDownPushed);

            _resizeUpBackground = LoadImage(_scheme.ToggleButtonUpOn);
            _resizeUpHover = LoadImage(_scheme.ToggleButtonUpIn);
            _resizeUpPushed = LoadImage(_scheme.ToggleButtonUpPushed);

            UpdateResizeButtonStyle();

            State = ControlBarState.Default;
        }

        // TODO: This should be called at every logic tick.
        // TODO: This takes a player as the state information. Do we need any other state?
        public void Update(Player player)
        {
            _moneyDisplay.Text = $"$ {player.Money}";
            State.Update(player, this);
        }

        public void ToggleSize()
        {
            if (_size == ControlBarSize.Maximized)
            {
                _window.Top += MinimizeOffset;
                _background.Top += MinimizeOffset;
                _size = ControlBarSize.Minimized;
            }
            else
            {
                _window.Top -= MinimizeOffset;
                _background.Top -= MinimizeOffset;
                _size = ControlBarSize.Maximized;
            }

            UpdateResizeButtonStyle();
        }

        public void UpdateResizeButtonStyle()
        {
            if (_size == ControlBarSize.Maximized)
            {
                _resize.BackgroundImage = _resizeDownBackground;
                _resize.HoverBackgroundImage = _resizeDownHover;
                _resize.PushedBackgroundImage = _resizeDownPushed;
            }
            else
            {
                _resize.BackgroundImage = _resizeUpBackground;
                _resize.HoverBackgroundImage = _resizeUpHover;
                _resize.PushedBackgroundImage = _resizeUpPushed;
            }
        }

        public void PushWindows(WndWindowManager windowManager)
        {
            windowManager.PushWindow(_background);
            windowManager.PushWindow(_window);
        }

        public static WndControlBar Create(string side, ContentManager contentManager)
        {
            var scheme = contentManager.IniDataContext.ControlBarSchemes.FindBySide(side);

            // TODO: Support multiple image parts?
            // Generals always uses exactly one image part.
            var imagePart = scheme.ImageParts[0];

            var background = new Control
            {
                Name = "OpenSAGE:ControlBarBackground",
                Bounds = new Rectangle(imagePart.Position, imagePart.Size),
                BackgroundImage = CreateImage(imagePart.ImageName)
            };

            var backgroundWindow = new Window(scheme.ScreenCreationRes, background, contentManager);
            var controlBarWindow = contentManager.Load<Window>("Window/ControlBar.wnd", new LoadOptions { CacheAsset = false});

            Control FindControl(string name) => controlBarWindow.Controls.FindControl($"ControlBar.wnd:{name}");
            Image CreateImage(string path) => contentManager.WndImageLoader.CreateNormalImage(path);

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

            void ApplyButtonScheme(string name, string coordPrefix, string texturePrefix)
            {
                var button = ApplyBounds(name, coordPrefix) as Button;

                Image LoadImage(string state) =>
                    CreateImage(
                        (string) schemeType.GetProperty($"{texturePrefix}{state}")?.GetValue(scheme));

                button.BackgroundImage = LoadImage("Enable");
                button.DisabledBackgroundImage = LoadImage("Disabled");
                button.HoverBackgroundImage = LoadImage("Highlighted");
                button.PushedBackgroundImage = LoadImage("Pushed");
            }

            ApplyBounds("MoneyDisplay", "Money");
            ApplyBounds("PowerWindow", "PowerBar");

            ApplyButtonScheme("ButtonOptions", "Options", "OptionsButton");
            ApplyButtonScheme("ButtonPlaceBeacon", "Beacon", "BeaconButton");
            ApplyButtonScheme("PopupCommunicator", "Chat", "BuddyButton");
            ApplyButtonScheme("ButtonIdleWorker", "Worker", "IdleWorkerButton");

            ApplyButtonScheme("ButtonGeneral", "General", "GeneralButton");
            // Textures are set by ControlBar
            ApplyBounds("ButtonLarge", "MinMax");

            // TODO: Hide left HUD until we implement the minimap.
            FindControl("LeftHUD").Hide();

            var rightHud = FindControl("RightHUD");
            rightHud.BorderWidth = 0;
            rightHud.BackgroundColor = ColorRgbaF.Transparent;
            rightHud.BackgroundImage = CreateImage(scheme.RightHudImage);

            FindControl("ExpBarForeground").BackgroundImage = CreateImage(scheme.ExpBarForegroundImage);

            return new WndControlBar(backgroundWindow, controlBarWindow, scheme, contentManager);
        }

        private abstract class ControlBarState
        {
            public abstract void OnEnterState(WndControlBar controlBar);
            public abstract void Update(Player player, WndControlBar controlBar);

            public static ControlBarState Default { get; } = new DefaultControlBarState();
        }

        private sealed class DefaultControlBarState : ControlBarState
        {
            public override void OnEnterState(WndControlBar controlBar)
            {
                foreach (var control in controlBar._center.Controls)
                {
                    control.Hide();
                }

                foreach (var control in controlBar._right.Controls)
                {
                    control.Hide();
                }
            }

            public override void Update(Player player, WndControlBar controlBar)
            {

            }
        }

        private sealed class SelectedControlBarState : ControlBarState
        {
            public override void OnEnterState(WndControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }

            public override void Update(Player player, WndControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }
        }

        private sealed class UnderConstructionControlBarState : ControlBarState
        {
            public override void OnEnterState(WndControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }

            public override void Update(Player player, WndControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
