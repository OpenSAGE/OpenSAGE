using System;
using System.Linq;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Gui;
using OpenSage.Gui.Wnd;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;

namespace OpenSage.Mods.Generals.Gui
{
    public sealed class GeneralsControlBar : IControlBar
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

        private readonly Control _commandWindow;

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

        private readonly Image _commandButtonHover;
        private readonly Image _commandButtonPushed;

        private ControlBarSize _size = ControlBarSize.Maximized;

        private Image LoadImage(string name) => _contentManager.WndImageLoader.CreateNormalImage(name);
        private Control FindControl(string name) => _window.Controls.FindControl($"ControlBar.wnd:{name}");

        public GeneralsControlBar(Window background, Window window, ControlBarScheme scheme, ContentManager contentManager)
        {
            _background = background;
            _window = window;
            _scheme = scheme;
            _contentManager = contentManager;

            _center = FindControl("CenterBackground");
            _right = FindControl("RightHUD");

            _commandWindow = FindControl("CommandWindow");

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

            _commandButtonHover = LoadImage("Cameo_hilited");
            _commandButtonPushed = LoadImage("Cameo_push");

            UpdateResizeButtonStyle();

            State = ControlBarState.Default;
        }

        // TODO: This should be called at every logic tick.
        // TODO: This takes a player as the state information. Do we need any other state?
        public void Update(Player player)
        {
            if (player == null)
            {
                return;
            }

            _moneyDisplay.Text = $"$ {player.Money}";

            if (player.SelectedUnits.Count > 0)
            {
                State = new SelectedControlBarState();
            }
            else
            {
                State = ControlBarState.Default;
            }

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

        private void UpdateResizeButtonStyle()
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

        public void AddToScene(Scene2D scene2D)
        {
            scene2D.WndWindowManager.PushWindow(_background);
            scene2D.WndWindowManager.PushWindow(_window);
        }

        private abstract class ControlBarState
        {
            public abstract void OnEnterState(GeneralsControlBar controlBar);
            public abstract void Update(Player player, GeneralsControlBar controlBar);

            public static ControlBarState Default { get; } = new DefaultControlBarState();

            private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            protected void ApplyCommandSet(GeneralsControlBar controlBar, CommandSet commandSet)
            {
                for (var i = 1; i <= 12; i++)
                {
                    var buttonControl = controlBar._commandWindow.Controls.FindControl($"ControlBar.wnd:ButtonCommand{i:D2}") as Button;

                    if (commandSet != null && commandSet.Buttons.TryGetValue(i, out var commandButtonName))
                    {
                        var commandButton = controlBar._contentManager.IniDataContext.CommandButtons.Find(x => x.Name == commandButtonName);

                        buttonControl.BackgroundImage = controlBar._contentManager.WndImageLoader.CreateNormalImage(commandButton.ButtonImage);

                        buttonControl.BorderColor = GetBorderColor(commandButton.ButtonBorderType, controlBar._scheme).ToColorRgbaF();
                        buttonControl.BorderWidth = 1;

                        buttonControl.HoverOverlayImage = controlBar._commandButtonHover;
                        buttonControl.PushedOverlayImage = controlBar._commandButtonPushed;

                        buttonControl.SystemCallback = (control, message, context) =>
                        {
                            logger.Debug($"Button callback: {control.Name}, {commandButton.Command.ToString()}");

                            var playerIndex = context.Game.Scene3D.GetPlayerIndex(context.Game.Scene3D.LocalPlayer);
                            Order CreateOrder(OrderType type) => new Order(playerIndex, type);

                            var objectDefinition = context.Game.ContentManager.IniDataContext.Objects.Find(x => x.Name == commandButton.Object);

                            logger.Debug($"Relevant object: {objectDefinition?.Name}");

                            Order order = null;
                            switch (commandButton.Command)
                            {
                                case CommandType.DozerConstruct:
                                    context.Game.OrderGenerator.StartConstructBuilding(objectDefinition);
                                    break;

                                case CommandType.ToggleOvercharge:
                                    order = CreateOrder(OrderType.ToggleOvercharge);
                                    break;

                                case CommandType.Sell:
                                    order = CreateOrder(OrderType.Sell);
                                    break;

                                case CommandType.UnitBuild:
                                    order = CreateOrder(OrderType.CreateUnit);
                                    order.AddIntegerArgument(context.Game.ContentManager.IniDataContext.Objects.IndexOf(objectDefinition)+1);
                                    order.AddIntegerArgument(1);
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }

                            if (order != null)
                            {
                                context.Game.NetworkMessageBuffer.AddLocalOrder(order);
                            }
                        };

                        buttonControl.Show();
                    }
                    else
                    {
                        buttonControl.Hide();
                    }
                }
            }

            private static ColorRgba GetBorderColor(CommandButtonBorderType borderType, ControlBarScheme scheme)
            {
                switch (borderType)
                {
                    case CommandButtonBorderType.None:
                        return ColorRgba.Transparent;

                    case CommandButtonBorderType.Action:
                        return scheme.ButtonBorderActionColor;

                    case CommandButtonBorderType.Build:
                        return scheme.ButtonBorderBuildColor;

                    case CommandButtonBorderType.Upgrade:
                        return scheme.ButtonBorderUpgradeColor;
                        
                    case CommandButtonBorderType.System:
                        return scheme.ButtonBorderSystemColor;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(borderType));
                }
            }
        }

        private sealed class DefaultControlBarState : ControlBarState
        {
            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                foreach (var control in controlBar._center.Controls)
                {
                    if (control.Name == "ControlBar.wnd:CommandWindow")
                    {
                        foreach (var child in control.Controls)
                        {
                            child.Hide();
                        }
                        control.Show();
                    }
                    else
                    {
                        control.Hide();
                    }
                }

                foreach (var control in controlBar._right.Controls)
                {
                    control.Hide();
                }
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {

            }
        }

        private sealed class SelectedControlBarState : ControlBarState
        {
            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {
                // TODO: Handle multiple selection.
                var unit = player.SelectedUnits.First();
                var commandSet = controlBar._contentManager.IniDataContext.CommandSets.Find(x => x.Name == unit.Definition.CommandSet);
                ApplyCommandSet(controlBar, commandSet);

                var unitSelectedControl = controlBar._right.Controls.FindControl("ControlBar.wnd:WinUnitSelected");

                var productionQueueWindow = controlBar._right.Controls.FindControl("ControlBar.wnd:ProductionQueueWindow");
                productionQueueWindow.Visible = unit.IsProducing;

                var queue = unit.ProductionQueue;

                for (var pos = 0; pos < 9; pos++)
                {
                    var queueButton = productionQueueWindow.Controls.FindControl($"ControlBar.wnd:ButtonQueue0{pos+1}");
                    Image img = null;
                    if (queue.Count > pos)
                    {
                        var job = queue[pos];
                        if (queueButton != null && job != null)
                        {
                            // quick and dirty progress indicator. needs to be remade to show the clock-like overlay
                            queueButton.Opacity = (1.0f - job.Progress);

                            img = controlBar._contentManager.WndImageLoader.CreateNormalImage(job.objectDefinition.SelectPortrait);
                            queueButton.SystemCallback = (Control control, WndWindowMessage message, ControlCallbackContext context) =>
                            {
                                unit.CancelProduction(pos);
                            };
                        }
                     
                    }
                    queueButton.BackgroundImage = img;
                }

                var iconControl = unitSelectedControl.Controls.FindControl("ControlBar.wnd:CameoWindow");
                var cameoImg = controlBar._contentManager.WndImageLoader.CreateNormalImage(unit.Definition.SelectPortrait);
                iconControl.BackgroundImage = cameoImg;
                iconControl.Visible = !unit.IsProducing;

                void ApplyUpgradeImage(string upgradeControlName, string upgradeName)
                {
                    var upgrade = upgradeName != null
                        ? controlBar._contentManager.IniDataContext.Upgrades.Find(x => x.Name == upgradeName)
                        : null;
                    var upgradeControl = unitSelectedControl.Controls.FindControl($"ControlBar.wnd:{upgradeControlName}");
                    upgradeControl.BackgroundImage = upgrade != null
                        ? controlBar._contentManager.WndImageLoader.CreateNormalImage(upgrade.ButtonImage)
                        : null;
                }

                ApplyUpgradeImage("UnitUpgrade1", unit.Definition.UpgradeCameo1);
                ApplyUpgradeImage("UnitUpgrade2", unit.Definition.UpgradeCameo2);
                ApplyUpgradeImage("UnitUpgrade3", unit.Definition.UpgradeCameo3);
                ApplyUpgradeImage("UnitUpgrade4", unit.Definition.UpgradeCameo4);
                ApplyUpgradeImage("UnitUpgrade5", unit.Definition.UpgradeCameo5);

                unitSelectedControl.Show();
            }
        }

        private sealed class UnderConstructionControlBarState : ControlBarState
        {
            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {
                throw new System.NotImplementedException();
            }
        }
    }

    public sealed class GeneralsControlBarSource : IControlBarSource
    {
        public IControlBar Create(string side, ContentManager contentManager)
        {
            // TODO: This is not the best place for this.
            contentManager.IniDataContext.LoadIniFile(@"Data\INI\ControlBarScheme.ini");
            contentManager.IniDataContext.LoadIniFile(@"Data\INI\CommandSet.ini");
            contentManager.IniDataContext.LoadIniFile(@"Data\INI\CommandButton.ini");
            contentManager.IniDataContext.LoadIniFile(@"Data\INI\Upgrade.ini");

            var scheme = contentManager.IniDataContext.ControlBarSchemes.FindBySide(side);

            // TODO: Support multiple image parts?
            // Generals always uses exactly one image part.
            var imagePart = scheme.ImageParts[0];

            var background = new Control
            {
                Name = "OpenSAGE:ControlBarBackground",
                Bounds = new Rectangle(imagePart.Position, imagePart.Size),
                BackgroundImage = LoadImage(imagePart.ImageName)
            };

            var backgroundWindow = new Window(scheme.ScreenCreationRes, background, contentManager);
            var controlBarWindow = contentManager.Load<Window>("Window/ControlBar.wnd", new LoadOptions { CacheAsset = false });

            Control FindControl(string name) => controlBarWindow.Controls.FindControl($"ControlBar.wnd:{name}");
            Image LoadImage(string path) => contentManager.WndImageLoader.CreateNormalImage(path);

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

                Image LoadImageForState(string state) =>
                    LoadImage(
                        (string) schemeType.GetProperty($"{texturePrefix}{state}")?.GetValue(scheme));

                button.BackgroundImage = LoadImageForState("Enable");
                button.DisabledBackgroundImage = LoadImageForState("Disabled");
                button.HoverBackgroundImage = LoadImageForState("Highlighted");
                button.PushedBackgroundImage = LoadImageForState("Pushed");
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
            rightHud.BackgroundImage = LoadImage(scheme.RightHudImage);

            FindControl("ExpBarForeground").BackgroundImage = LoadImage(scheme.ExpBarForegroundImage);

            return new GeneralsControlBar(backgroundWindow, controlBarWindow, scheme, contentManager);
        }
    }
}
