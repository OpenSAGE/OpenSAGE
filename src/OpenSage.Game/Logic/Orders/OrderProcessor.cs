using System.Collections.Generic;

namespace OpenSage.Logic.Orders
{
    public sealed class OrderProcessor
    {
        private readonly Game _game;

        public OrderProcessor(Game game)
        {
            _game = game;
        }

        public void Process(IEnumerable<Order> orders)
        {
            foreach (var order in orders)
            {
                switch (order.OrderType)
                {
                    // TODO

                    case OrderType.Unknown27:
                        // TODO
                        _game.Scene3D = null;
                        _game.NetworkMessageBuffer = null;
                        _game.Scene2D.WndWindowManager.SetWindow(@"Menus\MainMenu.wnd");
                        break;
                }
            }
        }
    }
}
