using System.Collections.Generic;
using OpenSage.Input;

namespace OpenSage
{
    public sealed class GameMessageBuffer : DisposableBase
    {
        private readonly Queue<GameMessage> _messageQueue;

        public List<GameMessageHandler> Handlers { get; }

        public GameMessageBuffer(GameWindow window)
        {
            _messageQueue = new Queue<GameMessage>();

            Handlers = new List<GameMessageHandler>();

            window.InputMessageReceived += OnInputMessage;

            AddDisposeAction(() => window.InputMessageReceived -= OnInputMessage);
        }

        private void OnInputMessage(object sender, InputMessageEventArgs e)
        {
            _messageQueue.Enqueue(e.Message);
        }

        internal void PropagateMessages()
        {
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();

                foreach (var handler in Handlers)
                {
                    if (handler.HandleMessage(message) == GameMessageResult.Handled)
                    {
                        break;
                    }
                }
            }
        }
    }
}
