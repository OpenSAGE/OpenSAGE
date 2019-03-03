using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;

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
                var player = _game.Scene3D.Players[(int) order.PlayerIndex];

                switch (order.OrderType)
                {
                    // TODO
                    case OrderType.MoveTo:
                        {
                            var targetPosition = order.Arguments[0].Value.Position;
                            foreach(var unit in player.SelectedUnits)
                            {
                                //TODO: only play this for local players
                                unit.OnLocalMove(_game.Audio);

                                unit.MoveTo(targetPosition);
                            }
                        }
                        break;
                    case OrderType.BuildObject:
                        {
                            var objectDefinitionId = order.Arguments[0].Value.Integer;
                            var objectDefinition = _game.ContentManager.IniDataContext.Objects[objectDefinitionId - 1];
                            var position = order.Arguments[1].Value.Position;
                            var angle = order.Arguments[2].Value.Float;
                            var gameObject = _game.Scene3D.GameObjects.Add(objectDefinition, player);

                            gameObject.Transform.Translation = position;
                            gameObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
                            gameObject.StartConstruction(_game.MapTime);
                        }
                        break;

                    case OrderType.Sell:
                        _game.Scene3D.GameObjects.Remove(player.SelectedUnits.First());
                        _game.Selection.ClearSelectedObjects(player);
                        break;

                    case OrderType.SetCameraPosition:
                        // Ignore this message.
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

                    case OrderType.SetRallyPoint:
                          var objIds = order.Arguments.Skip(1)
                            .Select(x => (int) x.Value.ObjectId)
                            .Select(_game.Scene3D.GameObjects.GetObjectById)
                            .ToArray();
                        _game.Selection.SetRallyPointForSelectedObjects(player, objIds, new Vector3());
                        break;

                    case OrderType.Unknown27:
                        _game.EndGame();
                        break;
                }
            }
        }
    }
}
