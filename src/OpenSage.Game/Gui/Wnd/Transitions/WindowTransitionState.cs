using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;

namespace OpenSage.Gui.Wnd.Transitions
{
    internal sealed class WindowTransitionState
    {
        private readonly List<WindowTransitionOperation> _operations;

        public TimeSpan LastEndTime { get; }

        public WindowTransitionState(WndTopLevelWindow window, WindowTransition transition, TimeSpan currentTime)
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
}
