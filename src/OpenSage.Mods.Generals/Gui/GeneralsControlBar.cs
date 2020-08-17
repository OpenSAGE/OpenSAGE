using System;
using System.Linq;
using OpenSage.Content;
using OpenSage.Core;
using OpenSage.Gui;
using OpenSage.Gui.ControlBar;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Gui.Wnd.Images;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders;
using OpenSage.Logic.Object.Production;
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
                if (_state != value)
                {
                    _state = value;
                    _state.OnEnterState(this);
                }
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

        private Control FindControl(string name) => _window.Controls.FindControl($"ControlBar.wnd:{name}");

        private void ApplyProgress(string name, string coordPrefix, float progress = 1.0f)
        {
            var control = FindControl(name);
            if (control == null)
                return;
            var schemeType = _scheme.GetType();

            var ul = (Point2D) schemeType.GetProperty($"{coordPrefix}UL").GetValue(_scheme);
            var lr = (Point2D) schemeType.GetProperty($"{coordPrefix}LR").GetValue(_scheme);
            var width = (int) (progress * (lr.X - ul.X));
            lr = new Point2D(ul.X + width, lr.Y);

            control.Bounds = Rectangle.FromCorners(ul - _window.Bounds.Location, lr - _window.Bounds.Location);
        }

        public GeneralsControlBar(Window background, Window window, ControlBarScheme scheme, ContentManager contentManager, AssetStore assetStore)
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

            _resizeDownBackground = window.ImageLoader.CreateFromMappedImageReference(_scheme.ToggleButtonDownOn);
            _resizeDownHover = window.ImageLoader.CreateFromMappedImageReference(_scheme.ToggleButtonDownIn);
            _resizeDownPushed = window.ImageLoader.CreateFromMappedImageReference(_scheme.ToggleButtonDownPushed);

            _resizeUpBackground = window.ImageLoader.CreateFromMappedImageReference(_scheme.ToggleButtonUpOn);
            _resizeUpHover = window.ImageLoader.CreateFromMappedImageReference(_scheme.ToggleButtonUpIn);
            _resizeUpPushed = window.ImageLoader.CreateFromMappedImageReference(_scheme.ToggleButtonUpPushed);

            _commandButtonHover = window.ImageLoader.CreateFromMappedImageReference(assetStore.MappedImages.GetLazyAssetReferenceByName("Cameo_hilited"));
            _commandButtonPushed = window.ImageLoader.CreateFromMappedImageReference(assetStore.MappedImages.GetLazyAssetReferenceByName("Cameo_push"));

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

            float powerBarProgress = player.GetEnergy(this._window.Game.Scene3D.GameObjects) / 100.0f;
            ApplyProgress("PowerWindow", "PowerBar", Math.Clamp(powerBarProgress, 0.0f, 1.0f));

            if (player.SelectedUnits.Count > 0 && player.SelectedUnits.First().Owner == player)
            {
                var unit = player.SelectedUnits.First();
                if (player.SelectedUnits.Count == 1 && unit.IsBeingConstructed())
                {
                    State = ControlBarState.Construction;
                }
                else
                {
                    State = ControlBarState.Selected;
                }
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
            public static ControlBarState Selected { get; } = new SelectedControlBarState();
            public static ControlBarState Construction { get; } = new UnderConstructionControlBarState();

            private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            protected void ClearControls(GeneralsControlBar controlBar)
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

            protected void ApplyCommandSet(GameObject selectedUnit, GeneralsControlBar controlBar, CommandSet commandSet)
            {
                for (var i = 1; i <= 12; i++)
                {
                    var buttonControl = controlBar._commandWindow.Controls.FindControl($"ControlBar.wnd:ButtonCommand{i:D2}") as Button;

                    if (commandSet != null && commandSet.Buttons.TryGetValue(i, out var commandButtonReference))
                    {
                        var commandButton = commandButtonReference.Value;

                        buttonControl.BackgroundImage = controlBar._window.ImageLoader.CreateFromMappedImageReference(commandButton.ButtonImage);

                        buttonControl.DisabledBackgroundImage = buttonControl.BackgroundImage?.WithGrayscale(true);

                        buttonControl.BorderColor = GetBorderColor(commandButton.ButtonBorderType, controlBar._scheme).ToColorRgbaF();
                        buttonControl.BorderWidth = 1;

                        buttonControl.HoverOverlayImage = controlBar._commandButtonHover;
                        buttonControl.PushedOverlayImage = controlBar._commandButtonPushed;

                        var objectDefinition = commandButton.Object?.Value;

                        switch (commandButton.Command)
                        {
                            // Disable the button when the unit is not produceable
                            case CommandType.DozerConstruct:
                            case CommandType.UnitBuild:
                                buttonControl.Enabled = objectDefinition == null || selectedUnit.Owner.CanProduceObject(selectedUnit.Parent, objectDefinition);
                                break;
                            // Disable the button when the object already has it etc.
                            case CommandType.PlayerUpgrade:
                            case CommandType.ObjectUpgrade:
                                var upgrade = commandButton.Upgrade.Value;
                                var hasQueuedUpgrade = selectedUnit.ProductionUpdate.ProductionQueue.Any(x => x.UpgradeDefinition == upgrade);
                                var canEnqueue = selectedUnit.ProductionUpdate.CanEnque();
                                var hasUpgrade = selectedUnit.UpgradeAvailable(upgrade);
                                var upgradeIsInvalid = selectedUnit.ConflictingUpgradeAvailable(upgrade);

                                buttonControl.Enabled = canEnqueue && !hasQueuedUpgrade && !hasUpgrade && !upgradeIsInvalid;
                                break;
                        }

                        buttonControl.SystemCallback = (control, message, context) =>
                        {
                            logger.Debug($"Button callback: {control.Name}, {commandButton.Command}");

                            var playerIndex = context.Game.Scene3D.GetPlayerIndex(context.Game.Scene3D.LocalPlayer);
                            Order CreateOrder(OrderType type) => new Order(playerIndex, type);

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
                                    order.AddIntegerArgument(objectDefinition.InternalId);
                                    order.AddIntegerArgument(1);
                                    break;

                                case CommandType.SetRallyPoint:
                                    context.Game.OrderGenerator.SetRallyPoint();
                                    break;

                                case CommandType.PlayerUpgrade:
                                case CommandType.ObjectUpgrade:
                                    {
                                        order = CreateOrder(OrderType.BeginUpgrade);
                                        //TODO: figure this out correctly
                                        var selection = context.Game.Scene3D.LocalPlayer.SelectedUnits;
                                        var objId = context.Game.Scene3D.GameObjects.GetObjectId(selection.First());
                                        order.AddIntegerArgument(objId);
                                        var upgrade = commandButton.Upgrade.Value;
                                        order.AddIntegerArgument(upgrade.InternalId);
                                    }
                                    break;

                                case CommandType.Stop:
                                    // TODO: Also stop construction?
                                    order = CreateOrder(OrderType.StopMoving);
                                    break;

                                case CommandType.SpecialPower:
                                    {
                                        var specialPower = commandButton.SpecialPower.Value;
                                        if (commandButton.Options != null)
                                        {
                                            var needsPos = commandButton.Options.Get(CommandButtonOption.NeedTargetPos);
                                            var needsObject = commandButton.Options.Get(CommandButtonOption.NeedTargetAllyObject)
                                                             || commandButton.Options.Get(CommandButtonOption.NeedTargetEnemyObject)
                                                             || commandButton.Options.Get(CommandButtonOption.NeedTargetNeutralObject);

                                            if (needsPos)
                                            {
                                                context.Game.OrderGenerator.StartSpecialPowerAtLocation(specialPower);
                                            }
                                            else if (needsObject)
                                            {
                                                context.Game.OrderGenerator.StartSpecialPowerAtObject(specialPower);
                                            }
                                        }
                                        else
                                        {
                                            order = CreateOrder(OrderType.SpecialPower);
                                            order.AddIntegerArgument(specialPower.InternalId);
                                            order.AddIntegerArgument(0);
                                            order.AddObjectIdArgument(0);
                                        }
                                    }
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
                ClearControls(controlBar);
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {

            }
        }

        private sealed class SelectedControlBarState : ControlBarState
        {
            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                ClearControls(controlBar);
            }

            private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

            const int PRODUCTION_QUEUE_SIZE = 9;

            public override void Update(Player player, GeneralsControlBar controlBar)
            {
                // TODO: Handle multiple selection.
                var unit = player.SelectedUnits.First();

                if (unit.Definition.CommandSet == null) return;

                var commandSet = unit.Definition.CommandSet.Value;

                // TODO: Only do this when command set changes.
                ApplyCommandSet(unit, controlBar, commandSet);

                var unitSelectedControl = controlBar._right.Controls.FindControl("ControlBar.wnd:WinUnitSelected");

                var isProducing = unit.ProductionUpdate?.IsProducing ?? false;

                var productionQueueWindow = controlBar._right.Controls.FindControl("ControlBar.wnd:ProductionQueueWindow");

                unitSelectedControl.Visible = !isProducing;
                productionQueueWindow.Visible = isProducing;

                if (isProducing)
                {
                    var queue = unit.ProductionUpdate.ProductionQueue;

                    for (var pos = 0; pos < PRODUCTION_QUEUE_SIZE; pos++)
                    {
                        var queueButton = productionQueueWindow.Controls.FindControl($"ControlBar.wnd:ButtonQueue0{pos + 1}");

                        if (queueButton == null)
                        {
                            logger.Warn($"Could not find the right control (ControlBar.wnd:ButtonQueue0{pos + 1})");
                            continue;
                        }

                        Image img = null;
                        if (queue.Count > pos)
                        {
                            var job = queue[pos];
                            if (job != null)
                            {
                                queueButton.DrawCallback = (control, drawingContext) =>
                                {
                                    queueButton.DefaultDraw(control, drawingContext);

                                    // Draw radial progress indicator.
                                    drawingContext.FillRectangleRadial360(
                                        control.ClientRectangle,
                                        controlBar._scheme.BuildUpClockColor.ToColorRgbaF(),
                                        job.Progress);
                                };

                                if (job.Type == ProductionJobType.Unit)
                                {
                                    img = controlBar._window.ImageLoader.CreateFromMappedImageReference(job.ObjectDefinition.SelectPortrait);
                                }
                                else if (job.Type == ProductionJobType.Upgrade)
                                {
                                    img = controlBar._window.ImageLoader.CreateFromMappedImageReference(job.UpgradeDefinition.ButtonImage);
                                }
                                var posCopy = pos;

                                queueButton.SystemCallback = (control, message, context) =>
                                {
                                    var playerIndex = context.Game.Scene3D.GetPlayerIndex(context.Game.Scene3D.LocalPlayer);
                                    if (job.Type == ProductionJobType.Unit)
                                    {
                                        var order = new Order(playerIndex, OrderType.CancelUnit);
                                        order.AddIntegerArgument(posCopy);
                                        context.Game.NetworkMessageBuffer.AddLocalOrder(order);
                                    }
                                    else if (job.Type == ProductionJobType.Upgrade)
                                    {
                                        var order = new Order(playerIndex, OrderType.CancelUpgrade);
                                        order.AddIntegerArgument(job.UpgradeDefinition.InternalId);
                                        context.Game.NetworkMessageBuffer.AddLocalOrder(order);
                                    }
                                };
                            }
                        }
                        queueButton.BackgroundImage = img;

                        if (img == null)
                        {
                            queueButton.DrawCallback = queueButton.DefaultDraw;
                            queueButton.SystemCallback = null;
                        }
                    }
                }

                var iconControl = unitSelectedControl.Controls.FindControl("ControlBar.wnd:CameoWindow");
                var cameoImg = controlBar._window.ImageLoader.CreateFromMappedImageReference(unit.Definition.SelectPortrait);
                iconControl.BackgroundImage = cameoImg;
                iconControl.Visible = !isProducing;

                void ApplyUpgradeImage(GameObject unit, string upgradeControlName, LazyAssetReference<UpgradeTemplate> upgradeReference)
                {
                    var upgrade = upgradeReference?.Value;
                    var upgradeControl = unitSelectedControl.Controls.FindControl($"ControlBar.wnd:{upgradeControlName}");

                    upgradeControl.BackgroundImage = upgrade != null
                        ? controlBar._window.ImageLoader.CreateFromMappedImageReference(upgrade.ButtonImage)
                        : null;
                    upgradeControl.DisabledBackgroundImage = upgradeControl.BackgroundImage?.WithGrayscale(true);

                    upgradeControl.Enabled = unit.UpgradeAvailable(upgrade);
                }

                ApplyUpgradeImage(unit, "UnitUpgrade1", unit.Definition.UpgradeCameo1);
                ApplyUpgradeImage(unit, "UnitUpgrade2", unit.Definition.UpgradeCameo2);
                ApplyUpgradeImage(unit, "UnitUpgrade3", unit.Definition.UpgradeCameo3);
                ApplyUpgradeImage(unit, "UnitUpgrade4", unit.Definition.UpgradeCameo4);
                ApplyUpgradeImage(unit, "UnitUpgrade5", unit.Definition.UpgradeCameo5);
            }
        }

        private sealed class UnderConstructionControlBarState : ControlBarState
        {
            Control _window;
            Control _progressText;
            string _baseText;


            public override void OnEnterState(GeneralsControlBar controlBar)
            {
                ClearControls(controlBar);

                _window = controlBar._center.Controls.FindControl("ControlBar.wnd:UnderConstructionWindow");
                _window.Show();
                _progressText = _window.Controls.FindControl("ControlBar.wnd:UnderConstructionDesc");

                if (string.IsNullOrEmpty(_baseText))
                {
                    _baseText = StringConverter.FromPrintf(_progressText.Text);
                }

                //TODO: connect cancel button to some order.
            }

            public override void Update(Player player, GeneralsControlBar controlBar)
            {
                var unit = player.SelectedUnits.First();
                var percent = unit.BuildProgress * 100.0f;
                //TODO: the formatting should be taken from the printf string
                var text = string.Format(_baseText, percent.ToString("0.00"));
                _progressText.Text = text;
            }
        }
    }

    public sealed class GeneralsControlBarSource : IControlBarSource
    {
        public IControlBar Create(string side, Game game)
        {
            var scheme = game.AssetStore.ControlBarSchemes.FindBySide(side);

            // TODO: Support multiple image parts?
            // Generals always uses exactly one image part.
            var imagePart = scheme.ImageParts[0];

            var background = new Control
            {
                Name = "OpenSAGE:ControlBarBackground",
                Bounds = new Rectangle(imagePart.Position, imagePart.Size),
            };

            var backgroundWindow = new Window(scheme.ScreenCreationRes, background, game);
            var controlBarWindow = game.LoadWindow("ControlBar.wnd");

            background.BackgroundImage = backgroundWindow.ImageLoader.CreateFromMappedImageReference(imagePart.ImageName);

            Control FindControl(string name) => controlBarWindow.Controls.FindControl($"ControlBar.wnd:{name}");

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
                    controlBarWindow.ImageLoader.CreateFromMappedImageReference(
                        (LazyAssetReference<MappedImage>) schemeType.GetProperty($"{texturePrefix}{state}")?.GetValue(scheme));

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

            var rightHud = FindControl("RightHUD");
            rightHud.BorderWidth = 0;
            rightHud.BackgroundColor = ColorRgbaF.Transparent;
            rightHud.BackgroundImage = controlBarWindow.ImageLoader.CreateFromMappedImageReference(scheme.RightHudImage);

            FindControl("ExpBarForeground").BackgroundImage = controlBarWindow.ImageLoader.CreateFromMappedImageReference(scheme.ExpBarForegroundImage);

            return new GeneralsControlBar(backgroundWindow, controlBarWindow, scheme, game.ContentManager, game.AssetStore);
        }
    }
}
