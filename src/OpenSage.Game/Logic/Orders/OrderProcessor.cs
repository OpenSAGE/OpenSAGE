using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public void Process(IEnumerable<Order> orders)
        {
            foreach (var order in orders)
            {
                Player player = null;

                if(order.PlayerIndex==-1)
                {
                    player = _game.Scene3D.LocalPlayer;
                }
                else
                {
                    player = _game.Scene3D.Players[(int) order.PlayerIndex];
                }

                var logLevel = order.OrderType == OrderType.SetCameraPosition ? NLog.LogLevel.Trace : NLog.LogLevel.Debug;
                logger.Log(logLevel, $"Order for player {order.PlayerIndex}: {order.OrderType}");

                switch (order.OrderType)
                {
                    // TODO
                    case OrderType.MoveTo:
                        {
                            var targetPosition = order.Arguments[0].Value.Position;
                            foreach (var unit in player.SelectedUnits)
                            {
                                unit.AIUpdate?.SetTargetPoint(targetPosition);
                            }
                        }
                        break;
                    case OrderType.BuildObject:
                        {
                            var objectDefinitionId = order.Arguments[0].Value.Integer;
                            var objectDefinition = _game.AssetStore.ObjectDefinitions.GetByInternalId(objectDefinitionId);
                            var position = order.Arguments[1].Value.Position;
                            var angle = order.Arguments[2].Value.Float;
                            player.SpendMoney((uint) objectDefinition.BuildCost);

                            var gameObject = _game.Scene3D.GameObjects.Add(objectDefinition, player);
                            gameObject.Transform.Translation = position;
                            gameObject.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle);
                            gameObject.StartConstruction(_game.MapTime);
                        }
                        break;

                    case OrderType.BeginUpgrade:
                        {
                            var objectDefinitionId = order.Arguments[0].Value.Integer;
                            var upgradeDefinitionId = order.Arguments[1].Value.Integer;

                            var gameObject = _game.Scene3D.GameObjects.GetObjectById(objectDefinitionId);
                            var upgradeDefinition = _game.AssetStore.Upgrades.GetByInternalId(upgradeDefinitionId);
                            player.SpendMoney((uint) upgradeDefinition.BuildCost);

                            gameObject.ProductionUpdate.QueueUpgrade(upgradeDefinition);
                        }
                        break;

                    case OrderType.CancelUpgrade:
                        {
                            var upgradeDefinitionId = order.Arguments[0].Value.Integer;
                            var upgradeDefinition = _game.AssetStore.Upgrades.GetByInternalId(upgradeDefinitionId);

                            player.SpendMoney((uint) upgradeDefinition.BuildCost);
                            // since this is a building and only one at a time can be selected
                            player.SelectedUnits.First().ProductionUpdate.CancelUpgrade(upgradeDefinition);
                        }
                        break;

                    case OrderType.StopMoving:
                        foreach (var unit in player.SelectedUnits)
                        {
                            unit.AIUpdate.Stop();
                        }
                        break;

                    case OrderType.CreateUnit:
                        {
                            var objectDefinitionId = order.Arguments[0].Value.Integer;
                            var objectDefinition = _game.AssetStore.ObjectDefinitions.GetByInternalId(objectDefinitionId);
                            player.SpendMoney((uint) objectDefinition.BuildCost);
                            var placeInQueue = order.Arguments[1].Value.Integer;

                            foreach (var unit in player.SelectedUnits)
                            {
                                // Only units that can produce stuff should produce it
                                unit.ProductionUpdate?.QueueProduction(objectDefinition);
                            }
                        }
                        break;

                    case OrderType.CancelUnit:
                        {
                            var queueIndex = order.Arguments[0].Value.Integer;

                            foreach (var unit in player.SelectedUnits)
                            {
                                // Only units that can produce stuff should produce it
                                if(unit.ProductionUpdate == null)
                                {
                                    continue;
                                }

                                var productionJob = unit.ProductionUpdate.ProductionQueue[queueIndex];
                                var objectDefinition = productionJob.ObjectDefinition;

                                player.SpendMoney((uint) objectDefinition.BuildCost);

                                unit.ProductionUpdate?.CancelProduction(queueIndex);
                            }
                        }
                        break;

                    case OrderType.Sell:
                        foreach (var unit in player.SelectedUnits)
                        {
                            unit.ModelConditionFlags.Set(ModelConditionFlag.Sold, true);
                            player.SpendMoney((uint) (unit.Definition.BuildCost * _game.AssetStore.GameData.Current.SellPercentage));
                        }
                        _game.Selection.ClearSelectedObjects(player);
                        break;

                    case OrderType.SetCameraPosition:
                        // Ignore this message.
                        break;

                    case OrderType.SetSelection:
                        // TODO: First argument is an unknown boolean.
                        try
                        {
                            var objectIds = order.Arguments.Skip(1)
                                .Select(x => (int) x.Value.ObjectId)
                                .Select(_game.Scene3D.GameObjects.GetObjectById)
                                .ToArray();

                            _game.Selection.SetSelectedObjects(player, objectIds);
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Error while setting selection");
                        }

                        break;

                    case OrderType.ClearSelection:
                        _game.Selection.ClearSelectedObjects(player);
                        break;

                    case OrderType.AttackObject:
                    case OrderType.ForceAttackObject:
                        {
                            var objectDefinitionId = order.Arguments[0].Value.Integer;
                            var gameObject = _game.Scene3D.GameObjects.GetObjectById(objectDefinitionId);

                            foreach (var unit in player.SelectedUnits)
                            {
                                if (unit.CanAttack)
                                {
                                    unit.CurrentWeapon?.SetTarget(new WeaponTarget(gameObject));
                                }
                            }
                        }
                        break;

                    case OrderType.ForceAttackGround:
                        {
                            var targetPosition = order.Arguments[0].Value.Position;
                            foreach (var unit in player.SelectedUnits)
                            {
                                if (unit.CanAttack)
                                {
                                    unit.CurrentWeapon.SetTarget(new WeaponTarget(targetPosition));
                                }
                            }
                        }
                        break;

                    case OrderType.SetRallyPoint:
                        try
                        {
                            if (order.Arguments.Count == 2)
                            {
                                var objId = order.Arguments[0].Value.ObjectId;
                                var obj = _game.Scene3D.GameObjects.GetObjectById((int) objId);

                                var rallyPoint = order.Arguments[1].Value.Position;
                                obj.RallyPoint = rallyPoint;
                            }
                            else
                            {
                                var objIds = order.Arguments.Skip(1)
                                    .Select(x => (int) x.Value.ObjectId)
                                    .Select(_game.Scene3D.GameObjects.GetObjectById)
                                    .ToArray();
                                _game.Selection.SetRallyPointForSelectedObjects(player, objIds, new Vector3());
                            }
                        }
                        catch (System.Exception e)
                        {
                            logger.Error(e, "Error while setting rallypoint");
                        }
                        break;
                    case OrderType.SpecialPower:
                    case OrderType.SpecialPowerAtLocation:
                    case OrderType.SpecialPowerAtObject:
                        throw new NotImplementedException();

                    case OrderType.Unknown27:
                        _game.EndGame();
                        break;

                    //case OrderType.ChooseGeneralPromotion:
                    //gla:
                    //tier 1:
                    //34, 35, 36

                    //usa:
                    //tier1:
                    //12, 13, 14
                    //    break;

                    default:
                        var args = string.Join(", ", order.Arguments.Select(argument => argument.ToString()));

                        logger.Info($"Unimplemented order type: {order.OrderType.ToString()} ({args})");
                        break;
                }
            }
        }
    }
}
