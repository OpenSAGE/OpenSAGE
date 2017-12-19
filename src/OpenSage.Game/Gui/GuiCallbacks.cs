using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Gui.Elements;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    // TODO: This class should move to a Generals-specific csproj.
    internal static class GuiCallbacks
    {
        private static bool _doneMainMenuFadeIn;
        private static Vector2? _mousePosition;

        public static void W3DMainMenuInit(GuiWindow window)
        {
            // We'll show these later via window transitions.
            window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Opacity = 0;
            window.Root.FindChild("MainMenu.wnd:MainMenuRuler").Hide();
            window.Root.FindChild("MainMenu.wnd:MapBorder2").Hide();

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

        public static void MainMenuSystem(UIElement element, UIElementCallbackContext context)
        {
            if (!_doneMainMenuFadeIn)
            {
                if (_mousePosition == null)
                {
                    _mousePosition = context.MousePosition;
                }
                else if (_mousePosition.Value != context.MousePosition)
                {
                    context.TransitionManager.QueueTransition(null, context.Window, "MainMenuFade");
                    context.TransitionManager.QueueTransition(null, context.Window, "MainMenuDefaultMenu");
                    _doneMainMenuFadeIn = true;
                }
            }
        }
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
    }

    internal abstract class WindowTransitionOperation
    {
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

            var progress = MathUtility.Clamp(
                (float) (relativeTime.TotalSeconds / Duration.TotalSeconds),
                0,
                1);

            OnUpdate(progress);
        }

        protected abstract void OnUpdate(float progress);
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

            Element.Show();
        }
    }
}
