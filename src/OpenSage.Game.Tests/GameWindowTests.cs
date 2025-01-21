using System.Reflection;
using OpenSage.Input;
using Veldrid.Sdl2;
using Xunit;

namespace OpenSage.Tests;

public class GameWindowTests : StatePersisterTest
{
    [WindowsOnlyFact]
    public void GameWindow_Create()
    {
        var gameWindow = new GameWindow("Generals", 0, 0, 1024, 768, false);

        Assert.NotNull(gameWindow);
    }

    [WindowsOnlyFact]
    public void GameWindow_HandleMouseWheel_When_NotZeroDelta()
    {
        var gameWindow = new GameWindow("Generals", 0, 0, 1024, 768, false);

        var methodInfo = typeof(GameWindow).GetMethod("HandleMouseWheel", BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(gameWindow, [buildMouseWheelEventArgs(x: 100, y: 50, wheelDelta: 5)]);

        Assert.Single(gameWindow.MessageQueue);
        var inputMessage = gameWindow.MessageQueue.Dequeue();
        Assert.Equal(InputMessageType.MouseWheel, inputMessage.MessageType);
        Assert.Equal(500, inputMessage.Value.ScrollWheel); // Mouse wheel delta is multiplied by 100
        Assert.Equal(100, inputMessage.Value.MousePosition.X);
        Assert.Equal(50, inputMessage.Value.MousePosition.Y);
    }

    [WindowsOnlyFact]
    public void GameWindow_HandleMouseWheel_When_ZeroDelta()
    {
        var gameWindow = new GameWindow("Generals", 0, 0, 1024, 768, false);

        var methodInfo = typeof(GameWindow).GetMethod("HandleMouseWheel", BindingFlags.NonPublic | BindingFlags.Instance);
        methodInfo.Invoke(gameWindow, [buildMouseWheelEventArgs(x: 100, y: 50, wheelDelta: 0)]);

        Assert.Empty(gameWindow.MessageQueue);
    }

    private static MouseWheelEventArgs buildMouseWheelEventArgs(int x, int y, int wheelDelta)
    {
        var mouseState = new MouseState(x, y, false, false, false, false, false, false, false, false, false, false, false, false, false);
        return new MouseWheelEventArgs(mouseState,wheelDelta);
    }
}
