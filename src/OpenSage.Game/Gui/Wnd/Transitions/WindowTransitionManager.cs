using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Gui.Wnd.Controls;

namespace OpenSage.Gui.Wnd.Transitions
{
    public sealed class WindowTransitionManager
    {
        private readonly ScopedAssetCollection<WindowTransition> _transitions;

        private readonly Queue<WindowTransitionRequest> _transitionQueue;

        private sealed class WindowTransitionRequest
        {
            public Window From;
            public Window To;
            public WindowTransition Transition;
        }

        private WindowTransitionState _currentTransitionState;

        public WindowTransitionManager(ScopedAssetCollection<WindowTransition> transitions)
        {
            _transitions = transitions;

            _transitionQueue = new Queue<WindowTransitionRequest>();
        }

        public void QueueTransition(
            Window from,
            Window to,
            string transitionName)
        {
            var transition = _transitions.GetByName(transitionName);

            _transitionQueue.Enqueue(new WindowTransitionRequest
            {
                From = from,
                To = to,
                Transition = transition
            });
        }

        public void Update(TimeInterval currentTime)
        {
            var transitionTime = currentTime.TotalTime;

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

                if (currentTime.TotalTime > _currentTransitionState.LastEndTime)
                {
                    _currentTransitionState.Finish();
                    _currentTransitionState = null;
                }
            }
        }
    }
}
