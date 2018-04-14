using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Input
{
    public sealed class InputMessageBuffer : DisposableBase
    {
        private readonly Queue<InputMessage> _messageQueue;

        public List<InputMessageHandler> Handlers { get; }

        internal InputMessageBuffer(GameWindow window)
        {
            _messageQueue = new Queue<InputMessage>();

            Handlers = new List<InputMessageHandler>();

            window.InputMessageReceived += OnInputMessage;

            AddDisposeAction(() => window.InputMessageReceived -= OnInputMessage);
        }

        private void OnInputMessage(object sender, InputMessageEventArgs e)
        {
            _messageQueue.Enqueue(e.Message);
        }

        internal void PumpEvents()
        {
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();
                foreach (var handler in PriorityOrderedHandlers())
                {
                    if (handler.HandleMessage(message) == InputMessageResult.Handled)
                    {
                        break;
                    }
                }
            }
        }

        private IEnumerable<InputMessageHandler> PriorityOrderedHandlers()
        {
            var requiredPriority = HighestPriority;

            while (requiredPriority >= LowestPriority)
            {
                foreach (var handler in Handlers)
                {
                    if (handler.Priority == (HandlingPriority) requiredPriority)
                    {
                        yield return handler;
                    }
                }

                requiredPriority--;
            }
        }

        private static readonly int HighestPriority =
            Enum.GetValues(typeof(HandlingPriority)).Cast<int>().Max();

        private static readonly int LowestPriority =
            Enum.GetValues(typeof(HandlingPriority)).Cast<int>().Min();
    }
}
