using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd.Transitions
{
    public sealed class WindowTransitionManager
    {
        private readonly Dictionary<string, WindowTransition> _transitions;

        private readonly Queue<WindowTransitionRequest> _transitionQueue;

        private sealed class WindowTransitionRequest
        {
            public Window From;
            public Window To;
            public WindowTransition Transition;
        }

        private WindowTransitionState _currentTransitionState;

        public WindowTransitionManager(List<WindowTransition> transitions)
        {
            _transitions = transitions.ToDictionary(x => x.Name);

            _transitionQueue = new Queue<WindowTransitionRequest>();
        }

        public void QueueTransition(
            Window from,
            Window to,
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
}
