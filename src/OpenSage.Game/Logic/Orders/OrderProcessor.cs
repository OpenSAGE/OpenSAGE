using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenSage.Logic.Orders
{
    public sealed class OrderProcessor
    {
        private readonly Game _game;
        private readonly bool _isReplay;

        public OrderProcessor(Game game, bool isReplay)
        {
            _game = game;
            _isReplay = isReplay;
        }

        public void Process(IEnumerable<Order> orders)
        {
            foreach (var order in orders)
            {
                var player = _game.Scene3D.Players[(int) order.PlayerIndex];

                switch (order.OrderType)
                {
                    // TODO

                    case OrderType.BuildObject:
                        var objectDefinitionId = order.Arguments[0].Value.Integer;
                        var objectDefinition = _game.ContentManager.IniDataContext.Objects[objectDefinitionId - 1];
                        var position = order.Arguments[1].Value.Position;
                        var angle = order.Arguments[2].Value.Float;
                        var gameObject = _game.Scene3D.GameObjects.Add(objectDefinition, player);
                        gameObject.Transform.Translation = position;
                        gameObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
                        break;

                    case OrderType.Sell:
                        _game.Scene3D.GameObjects.Remove(player.SelectedUnits.First());
                        _game.Selection.ClearSelectedObjects(player);
                        break;

                    case OrderType.SetCameraPosition:
                        // Ignore SetCameraPosition during replays
                        if (!_isReplay)
                        {
                            _game.Scene3D.CameraController.TerrainPosition = order.Arguments[0].Value.Position;
                        }
                        break;

                    case OrderType.SetSelection:
                        // TODO: First argument is an unknown boolean.

                        var objectIds = order.Arguments.Skip(1)
                            .Select(x => (int) x.Value.ObjectId)
                            .Select(_game.Scene3D.GameObjects.GetObjectById)
                            .ToArray();

                        _game.Selection.SetSelectedObjects(player, objectIds);
                        break;

                    case OrderType.ClearSelection:
                        _game.Selection.ClearSelectedObjects(player);
                        break;

                    case OrderType.Unknown27:
                        _game.EndGame();
                        break;
                }
            }
        }
    }
}
