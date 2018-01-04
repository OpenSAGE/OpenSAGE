using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd.Elements;
using OpenSage.LowLevel.Graphics2D;
using OpenSage.LowLevel.Graphics3D;
using OpenSage.Mathematics;

namespace OpenSage.Gui.Wnd
{
    // TODO: This class should move to a Generals-specific csproj.
    internal static class WndCallbacks
    {
        private static bool _doneMainMenuFadeIn;

        public static void W3DMainMenuInit(GuiWindow window)
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

        public static void W3DNoDraw(UIElement element, GraphicsDevice device)
        {
            // TODO: Render normally if shell map is not loaded.
        }

        public static void MainMenuSystem(UIElement element, GuiWindowMessage message, UIElementCallbackContext context)
        {
            switch (message.MessageType)
            {
                case GuiWindowMessageType.SelectedButton:
                    switch (message.Element.Name)
                    {
                        case "MainMenu.wnd:ButtonExit":
                            var exitWindow = context.GuiSystem.OpenWindow(@"Menus\QuitMessageBox.wnd");
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

        public static void MainMenuInput(UIElement element, GuiWindowMessage message, UIElementCallbackContext context)
        {
            // Any input at all (mouse, keyboard) will trigger the main menu fade-in.
            if (!_doneMainMenuFadeIn)
            {
                context.GuiSystem.TransitionManager.QueueTransition(null, element.Window, "MainMenuFade");
                context.GuiSystem.TransitionManager.QueueTransition(null, element.Window, "MainMenuDefaultMenu");
                element.Window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Show();
                _doneMainMenuFadeIn = true;
            }
        }

        public static void PassSelectedButtonsToParentSystem(UIElement element, GuiWindowMessage message, UIElementCallbackContext context)
        {
            if (message.MessageType != GuiWindowMessageType.SelectedButton)
            {
                return;
            }

            element.Parent.SystemCallback.Invoke(element.Parent, message, context);
        }

        public static void PassMessagesToParentSystem(UIElement element, GuiWindowMessage message, UIElementCallbackContext context)
        {
            element.Parent.SystemCallback.Invoke(element.Parent, message, context);
        }
    }

    internal sealed class GuiWindowMessage
    {
        public GuiWindowMessageType MessageType { get; }
        public UIElement Element { get; }

        public GuiWindowMessage(GuiWindowMessageType messageType, UIElement element)
        {
            MessageType = messageType;
            Element = element;
        }
    }

    public enum GuiWindowMessageType
    {
        MouseEnter,
        MouseExit,

        MouseMove,

        MouseDown,
        MouseUp,

        SelectedButton
    }

    internal sealed class WindowTransitionManager
    {
        private readonly Dictionary<string, WindowTransition> _transitions;

        private readonly Queue<WindowTransitionRequest> _transitionQueue;

        private sealed class WindowTransitionRequest
        {
            public GuiWindow From;
            public GuiWindow To;
            public WindowTransition Transition;
        }

        private WindowTransitionState _currentTransitionState;

        public WindowTransitionManager(List<WindowTransition> transitions)
        {
            _transitions = transitions.ToDictionary(x => x.Name);

            _transitionQueue = new Queue<WindowTransitionRequest>();
        }

        public void QueueTransition(
            GuiWindow from,
            GuiWindow to,
            string transitionName)
        {
            if (!_transitions.TryGetValue(transitionName, out var transition))
            {
                throw new ArgumentOutOfRangeException(nameof(transitionName));
            }

            _transitionQueue.Enqueue(new WindowTransitionRequest
            {
                From = from,
                To = to,
                Transition = transition
            });
        }

        public void Update(GameTime currentTime)
        {
            var transitionTime = currentTime.TotalGameTime;

            if (_currentTransitionState == null && _transitionQueue.Count > 0)
            {
                var nextTransition = _transitionQueue.Dequeue();

                _currentTransitionState = new WindowTransitionState(
                    nextTransition.To,
                    nextTransition.Transition,
                    transitionTime);
            }

            if (_currentTransitionState != null)
            {
                _currentTransitionState.Update(transitionTime);

                if (currentTime.TotalGameTime > _currentTransitionState.LastEndTime)
                {
                    _currentTransitionState.Finish();
                    _currentTransitionState = null;
                }
            }
        }
    }

    internal sealed class WindowTransitionState
    {
        private readonly List<WindowTransitionOperation> _operations;

        public TimeSpan LastEndTime { get; }

        public WindowTransitionState(GuiWindow window, WindowTransition transition, TimeSpan currentTime)
        {
            _operations = transition.Windows
                .Select(x => WindowTransitionOperation.Create(window, x, currentTime))
                .ToList();

            LastEndTime = _operations.Max(x => x.EndTime);
        }

        public void Update(TimeSpan currentTime)
        {
            foreach (var operation in _operations)
            {
                operation.Update(currentTime);
            }
        }

        public void Finish()
        {
            foreach (var operation in _operations)
            {
                operation.Finish();
            }
        }
    }

    internal abstract class WindowTransitionOperation
    {
        protected static readonly ColorRgba FlashColor = new ColorRgba(255, 187, 0, 255);

        public static WindowTransitionOperation Create(
            GuiWindow window,
            WindowTransitionWindow transitionWindow,
            TimeSpan currentTime)
        {
            var element = window.Root.FindChild(transitionWindow.WinName);
            var startTime = currentTime + TimeSpan.FromSeconds(transitionWindow.FrameDelay / 30.0f);

            switch (transitionWindow.Style)
            {
                case WindowTransitionStyle.WinFade:
                    return new WinFadeTransition(element, startTime);

                case WindowTransitionStyle.Flash:
                    return new FlashTransition(element, startTime);

                case WindowTransitionStyle.ButtonFlash:
                    return new ButtonFlashTransition(element, startTime);

                default:
                    throw new NotImplementedException();
            }
        }

        protected UIElement Element { get; }

        protected abstract int FrameDuration { get; }

        public TimeSpan StartTime { get; }

        public TimeSpan Duration { get; }

        public TimeSpan EndTime { get; }

        protected WindowTransitionOperation(UIElement element, TimeSpan startTime)
        {
            Element = element;

            StartTime = startTime;

            Duration = TimeSpan.FromSeconds(FrameDuration / 30.0);

            EndTime = startTime + Duration;
        }

        public void Update(TimeSpan currentTime)
        {
            var relativeTime = currentTime - StartTime;

            var unclampedProgress = (float) (relativeTime.TotalSeconds / Duration.TotalSeconds);

            var progress = MathUtility.Clamp(
                unclampedProgress,
                0,
                1);

            if (progress >= 0 && progress <= 1)
            {
                OnUpdate(progress);
            }
            else if (progress > 1)
            {
                OnFinish();
            }
        }

        protected abstract void OnUpdate(float progress);

        public void Finish()
        {
            OnFinish();
        }

        protected virtual void OnFinish() { }
    }

    internal sealed class WinFadeTransition : WindowTransitionOperation
    {
        private readonly float _startOpacity;
        private readonly float _endOpacity;

        protected override int FrameDuration => 12;

        public WinFadeTransition(UIElement element, TimeSpan startTime)
            : base(element, startTime)
        {
            _startOpacity = element.Opacity;
            _endOpacity = element.Opacity == 1 ? 0 : 1;
        }

        protected override void OnUpdate(float progress)
        {
            Element.Opacity = MathUtility.Lerp(
                _startOpacity,
                _endOpacity,
                progress);
        }
    }

    internal sealed class FlashTransition : WindowTransitionOperation
    {
        // 3 frames to yellow (border colour?), 0 opacity to half final opacity,
        // 3 frames to final colour, half final opacity to final opacity.

        private readonly float _startOpacity;
        private readonly float _endOpacity;

        protected override int FrameDuration => 6;

        public FlashTransition(UIElement element, TimeSpan startTime)
            : base(element, startTime)
        {
            _startOpacity = element.Opacity;
            _endOpacity = element.Opacity == 1 ? 0 : 1;
        }

        protected override void OnUpdate(float progress)
        {
            Element.Opacity = MathUtility.Lerp(
                _startOpacity,
                _endOpacity,
                progress);

            Element.BackgroundColorOverride = progress < 0.5f
                ? FlashColor
                : Element.Definition.EnabledDrawData.Items[0].Color;
        }

        protected override void OnFinish()
        {
            Element.Opacity = _endOpacity;
            Element.BackgroundColorOverride = null;
        }
    }

    internal sealed class ButtonFlashTransition : WindowTransitionOperation
    {
        // 3 frames to yellow, 5 frames to black, 2 frames nothing, then 2 frames to show text.

        private readonly float _startOpacity;
        private readonly float _endOpacity;

        protected override int FrameDuration => 12;

        public ButtonFlashTransition(UIElement element, TimeSpan startTime)
            : base(element, startTime)
        {
            _startOpacity = element.Opacity;
            _endOpacity = element.Opacity == 1 ? 0 : 1;
        }

        protected override void OnUpdate(float progress)
        {
            if (progress < 3.0f / FrameDuration)
            {
                Element.Opacity = MathUtility.Lerp(
                    _startOpacity,
                    _endOpacity,
                    progress / 3 * FrameDuration);

                Element.OverlayColorOverride = FlashColor;
            }
            else if (progress < 8.0f / FrameDuration)
            {
                Element.Opacity = _endOpacity;

                var overlayColor = FlashColor;
                overlayColor.A = (byte) MathUtility.Lerp(
                    _endOpacity * 255,
                    _startOpacity * 255,
                    (progress - 3.0f / FrameDuration) / 8 * FrameDuration);

                Element.OverlayColorOverride = overlayColor;
            }
            else if (progress < 10.0f / FrameDuration)
            {
                // Nothing
                Element.Opacity = _endOpacity;
                Element.OverlayColorOverride = null;
                Element.BackgroundColorOverride = null;
            }
            else
            {
                Element.Opacity = _endOpacity;
                Element.OverlayColorOverride = null;
                Element.TextOpacity = MathUtility.Lerp(
                    _startOpacity,
                    _endOpacity,
                    (progress - 10.0f / FrameDuration) * (FrameDuration / 2.0f));
            }
        }

        protected override void OnFinish()
        {
            Element.Opacity = _endOpacity;
            Element.TextOpacity = _endOpacity;
            Element.OverlayColorOverride = null;
            Element.BackgroundColorOverride = null;
        }
    }
}
