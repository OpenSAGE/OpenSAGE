using OpenSage.Input;

namespace OpenSage.Gui.Apt
{
    internal sealed class AptInputMessageHandler : InputMessageHandler
    {
        private readonly AptWindowManager _windowManager;
        private readonly Game _game;

        public override HandlingPriority Priority => HandlingPriority.UIPriority;

        public AptInputMessageHandler(AptWindowManager windowManager, Game game)
        {
            _game = game;
            _windowManager = windowManager;
        }

        public override InputMessageResult HandleMessage(InputMessage message)
        {
            return InputMessageResult.NotHandled;
            //throw new NotImplementedException();
        }
    }
}
