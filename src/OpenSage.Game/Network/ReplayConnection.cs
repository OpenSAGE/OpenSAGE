using System;
using System.Collections.Generic;
using OpenSage.Data.Rep;
using OpenSage.Logic.Orders;

namespace OpenSage.Network
{
    public sealed class ReplayConnection : IConnection
    {
        private readonly Queue<ReplayChunk> _chunks = new Queue<ReplayChunk>();

        public ReplayConnection(ReplayFile replayFile)
        {
            foreach (var chunk in replayFile.Chunks)
            {
                _chunks.Enqueue(chunk);
            }
        }

        // Ignore locally generated orders.
        public void Send(uint frame, List<Order> orders) { }

        public void Receive(uint frame, Action<uint, Order> packetFn)
        {
            while (_chunks.Count != 0 && _chunks.Peek().Header.Timecode <= frame)
            {
                var chunk = _chunks.Dequeue();
                packetFn(chunk.Header.Timecode, chunk.Order);
            }
        }

        public void Dispose() { }
    }
}
