using System;
using System.Collections.Generic;
using OpenSage.LowLevel;
using OpenSage.LowLevel.Input;

namespace OpenSage.Input
{
    public sealed class InputMessageBuffer : IDisposable
    {
        private readonly Queue<InputMessage> _messageQueue;

        private HostView _hostView;

        public List<InputMessageHandler> Handlers { get; }

        public InputMessageBuffer()
        {
            _messageQueue = new Queue<InputMessage>();

            Handlers = new List<InputMessageHandler>();
        }

        internal void SetHostView(HostView hostView)
        {
            DetachFromHostView();

            if (hostView != null)
            {
                hostView.InputMessage += OnInputMessage;
                _hostView = hostView;
            }
        }

        private void OnInputMessage(object sender, InputMessageEventArgs e)
        {
            _messageQueue.Enqueue(e.Message);
        }

        private void DetachFromHostView()
        {
            if (_hostView != null)
            {
                _hostView.InputMessage -= OnInputMessage;
                _hostView = null;
            }
        }

        internal void PropagateMessages()
        {
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();

                foreach (var handler in Handlers)
                {
                    if (handler.HandleMessage(message) == InputMessageResult.Handled)
                    {
                        break;
                    }
                }
            }
        }

        public void Dispose()
        {
            DetachFromHostView();
        }
    }
}
