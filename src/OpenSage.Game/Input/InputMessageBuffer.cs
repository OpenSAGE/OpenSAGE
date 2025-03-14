using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenSage.Input;

public sealed class InputMessageBuffer
{
    public List<InputMessageHandler> Handlers { get; }

    // TODO: Find a better way to do this.
    public Vector2 MousePosition { get; private set; }

    internal InputMessageBuffer()
    {
        Handlers = new List<InputMessageHandler>();
    }

    internal void PumpEvents(IEnumerable<InputMessage> inputMessages, in TimeInterval gameTime)
    {
        foreach (var message in inputMessages)
        {
            if (message.MessageType == InputMessageType.MouseMove)
            {
                MousePosition = message.Value.MousePosition.ToVector2();
            }

            foreach (var handler in PriorityOrderedHandlers())
            {
                if (handler.HandleMessage(message, gameTime) == InputMessageResult.Handled)
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
                if (handler.Priority == HandlingPriority.Disabled)
                {
                    continue;
                }

                if (handler.Priority == (HandlingPriority)requiredPriority)
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
