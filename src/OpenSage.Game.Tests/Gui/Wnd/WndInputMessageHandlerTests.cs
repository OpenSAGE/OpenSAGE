using OpenSage.Input;
using OpenSage.Gui.Wnd;
using OpenSage.Mathematics;
using OpenSage.Data.Wnd;
using OpenSage.Gui.Wnd.Controls;
using Xunit;

namespace OpenSage.Tests.Gui.Wnd
{
    public class WndInputMessageHandlerTests(GameFixture fixture) : IClassFixture<GameFixture>
    {
        // swap to the null skip message in order to run locally
        private const string Skip = "game files required";
        // private const string? Skip = null;

        private readonly GameFixture _fixture = fixture;

        [Fact(Skip = Skip)]
        public void HandleMessage_When_MouseWheelEventWithNoControlUnderMouse()
        {
            var game = _fixture.GetGame(SageGame.CncGenerals);
            var windowManager = new WndWindowManager(game);
            var handler = new WndInputMessageHandler(windowManager, game);

            var inputMessage = InputMessage.CreateMouseWheel(500, new Point2D(100, 50));
            var result = handler.HandleMessage(inputMessage);
            Assert.Equal(InputMessageResult.NotHandled, result);
        }
    }
}
