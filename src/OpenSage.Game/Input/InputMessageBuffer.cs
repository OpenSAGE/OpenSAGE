using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Input
{
    public sealed class InputMessageBuffer
    {
        public List<InputMessageHandler> Handlers { get; }

        internal InputMessageBuffer()
        {
            Handlers = new List<InputMessageHandler>();
        }

        internal void PumpEvents(IEnumerable<InputMessage> inputMessages)
        {
            foreach (var message in inputMessages)
            {
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
                    if (handler.Priority == HandlingPriority.Disabled)
                    {
                        continue;
                    }

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
