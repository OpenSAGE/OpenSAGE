﻿using System.Collections.Generic;

namespace OpenSage.Input
{
    public sealed class InputMessageBuffer : DisposableBase
    {
        private readonly Queue<InputMessage> _messageQueue;

        public List<InputMessageHandler> Handlers { get; }

        internal InputMessageBuffer(GamePanel panel)
        {
            _messageQueue = new Queue<InputMessage>();

            Handlers = new List<InputMessageHandler>();

            panel.InputMessageReceived += OnInputMessage;

            AddDisposeAction(() => panel.InputMessageReceived -= OnInputMessage);
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
                foreach (var handler in Handlers)
                {
                    if (handler.HandleMessage(message) == InputMessageResult.Handled)
                    {
                        break;
                    }
                }
            }
        }
    }
}
