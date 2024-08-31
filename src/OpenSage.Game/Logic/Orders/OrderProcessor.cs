#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Logic.Object;
using OpenSage.Logic.Orders.SpecialPower;

namespace OpenSage.Logic.Orders
{
    public sealed class OrderProcessor
    {
        private readonly Game _game;

        public OrderProcessor(Game game)
        {
            _game = game;
        }

        private static NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public void Process(IEnumerable<Order> orders)
        {
            foreach (var order in orders)
            {
                Player? player = null;

                if (order.PlayerIndex == -1)
                {
                    player = _game.Scene3D.LocalPlayer;
                }
                else
                {
                    player = _game.Scene3D.Players[(int) order.PlayerIndex];
                }

                var logLevel = order.OrderType == OrderType.SetCameraPosition ? NLog.LogLevel.Trace : NLog.LogLevel.Debug;
                Logger.Log(logLevel, $"Order for player {order.PlayerIndex}: {order.OrderType}");

                switch (order.OrderType)
                {
                    case OrderType.CreateGroup0:
                    case OrderType.CreateGroup1:
                    case OrderType.CreateGroup2:
                    case OrderType.CreateGroup3:
                    case OrderType.CreateGroup4:
                    case OrderType.CreateGroup5:
                    case OrderType.CreateGroup6:
                    case OrderType.CreateGroup7:
                    case OrderType.CreateGroup8:
                    case OrderType.CreateGroup9:
                        {
                            player.CreateSelectionGroup(order.OrderType - OrderType.CreateGroup0);
                        }
                        break;
                    case OrderType.SelectGroup0:
                    case OrderType.SelectGroup1:
                    case OrderType.SelectGroup2:
                    case OrderType.SelectGroup3:
                    case OrderType.SelectGroup4:
                    case OrderType.SelectGroup5:
                    case OrderType.SelectGroup6:
                    case OrderType.SelectGroup7:
                    case OrderType.SelectGroup8:
                    case OrderType.SelectGroup9:
                        {
                            player.SelectGroup(order.OrderType - OrderType.SelectGroup0);
                        }
                        break;
                    // TODO
                    case OrderType.MoveTo:
                        {
                            var targetPosition = order.Arguments[0].Value.Position;
                            GameObject? lastUnit = null;
                            foreach (var unit in player.SelectedUnits)
                            {
                                unit.AIUpdate?.SetTargetPoint(targetPosition);
                                var sound = unit.IsDamaged ? unit.Definition.SoundMoveStartDamaged?.Value : null;
                                sound ??= unit.Definition.SoundMoveStart?.Value;

                                if (sound != null)
                                {
                                    _game.Audio.PlayAudioEvent(unit, sound);
                                }

                                lastUnit = unit;
                            }

                            lastUnit?.OnLocalMove(_game.Audio);
                        }
                        break;
                    case OrderType.BuildObject:
                        {
                            var objectDefinitionId = order.Arguments[0].Value.Integer;
                            var objectDefinition = _game.AssetStore.ObjectDefinitions.GetByInternalId(objectDefinitionId);
                            var position = order.Arguments[1].Value.Position;
                            var angle = order.Arguments[2].Value.Float;
                            player.BankAccount.Withdraw((uint) objectDefinition.BuildCost);

                            var gameObject = _game.Scene3D.GameObjects.CreateObject(objectDefinition, player);
                            gameObject.Owner = player;
                            gameObject.UpdateTransform(position, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, angle));

                            gameObject.PrepareConstruction();

                            var dozer = player.SelectedUnits.SingleOrDefault(u => u.Definition.KindOf.Get(ObjectKinds.Dozer));
                            (dozer?.AIUpdate as IBuilderAIUpdate)?.SetBuildTarget(gameObject); // todo: I don't love this cast; it would be nice to get rid of it

                            _game.Audio.PlayAudioEvent(dozer, dozer?.Definition.UnitSpecificSounds?.VoiceBuildResponse?.Value);
                        }
                        break;
                    case OrderType.CancelBuild:
                        {
                            foreach (var unit in player.SelectedUnits)
                            {
                                // This probably shouldn't trigger a Die
                                unit.Die(DeathType.Normal);
                                player.BankAccount.Deposit((uint) unit.Definition.BuildCost);
                            }
                            player.DeselectUnits();
                        }
                        break;
                    case OrderType.ResumeBuild:
                        {
                            var buildTargetId = order.Arguments[0].Value.ObjectId;
                            var buildTarget = _game.Scene3D.GameObjects.GetObjectById(buildTargetId);

                            var dozer = player.SelectedUnits.SingleOrDefault(u => u.IsKindOf(ObjectKinds.Dozer));
                            (dozer?.AIUpdate as IBuilderAIUpdate)?.SetBuildTarget(buildTarget); // todo: I don't love this cast; it would be nice to get rid of it

                            _game.Audio.PlayAudioEvent(dozer, dozer?.Definition.UnitSpecificSounds?.VoiceBuildResponse?.Value);
                        }
                        break;
                    case OrderType.BeginUpgrade:
                        {
                            var objectDefinitionId = order.Arguments[0].Value.Integer;
                            var upgradeDefinitionId = order.Arguments[1].Value.Integer;

                            var gameObject = _game.Scene3D.GameObjects.GetObjectById((uint) objectDefinitionId);
                            var upgradeDefinition = _game.AssetStore.Upgrades.GetByInternalId(upgradeDefinitionId);
                            player.BankAccount.Withdraw((uint) upgradeDefinition.BuildCost);

                            gameObject.ProductionUpdate.QueueUpgrade(upgradeDefinition);
                        }
                        break;

                    case OrderType.CancelUpgrade:
                        {
                            var upgradeDefinitionId = order.Arguments[0].Value.Integer;
                            var upgradeDefinition = _game.AssetStore.Upgrades.GetByInternalId(upgradeDefinitionId);

                            player.BankAccount.Deposit((uint) upgradeDefinition.BuildCost);
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
                            player.BankAccount.Withdraw((uint) objectDefinition.BuildCost);
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
                                if (unit.ProductionUpdate == null)
                                {
                                    continue;
                                }

                                var productionJob = unit.ProductionUpdate.ProductionQueue[queueIndex];
                                var objectDefinition = productionJob.ObjectDefinition;

                                player.BankAccount.Deposit((uint) objectDefinition.BuildCost);

                                unit.ProductionUpdate?.CancelProduction(queueIndex);
                            }
                        }
                        break;

                    case OrderType.Sell:
                        foreach (var unit in player.SelectedUnits)
                        {
                            unit.Sell();
                            // todo: do not destroy or award money until sell teardown is complete
                            _game.Scene3D.GameObjects.DestroyObject(unit);
                            // items which award free units, like a tunnel network or supply center, have refund value set which overrides SellPercentage
                            var sellAmount = unit.Definition.RefundValue;

                            // from the inis: With nothing (or zero) listed, we sell for half price.
                            if (sellAmount == 0)
                            {
                                sellAmount = (int)Math.Round(unit.Definition.BuildCost * _game.AssetStore.GameData.Current.SellPercentage);
                            }

                            player.BankAccount.Deposit((uint) sellAmount);
                        }
                        break;

                    case OrderType.RepairStructure:
                        {
                            var repairer = player.SelectedUnits.SingleOrDefault(u => u.IsKindOf(ObjectKinds.Dozer));
                            var repairTargetId = order.Arguments[0].Value.ObjectId;
                            var repairTarget =  _game.Scene3D.GameObjects.GetObjectById(repairTargetId);

                            (repairer?.AIUpdate as IBuilderAIUpdate)?.SetRepairTarget(repairTarget);

                            _game.Audio.PlayAudioEvent(repairer, repairer?.Definition.UnitSpecificSounds?.VoiceRepair?.Value);
                        }
                        break;

                    case OrderType.SetCameraPosition:
                        // Ignore this message.
                        break;

                    case OrderType.SetSelection:
                        try
                        {
                            var objectIds = order.Arguments.Skip(1)
                                .Select(x => x.Value.ObjectId)
                                .Select(_game.Scene3D.GameObjects.GetObjectById)
                                .ToArray();

                            _game.Selection.SetSelectedObjects(player, objectIds, order.Arguments[0].Value.Boolean);
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "Error while setting selection");
                        }

                        break;

                    case OrderType.ClearSelection:
                        _game.Selection.ClearSelectedObjects(player);
                        break;

                    case OrderType.AttackObject:
                    case OrderType.ForceAttackObject:
                        {
                            var objectId = order.Arguments[0].Value.Integer;

                            foreach (var unit in player.SelectedUnits)
                            {
                                if (unit.CanAttack)
                                {
                                    unit.CurrentWeapon?.SetTarget(new WeaponTarget(_game.Scene3D.GameObjects, (uint)objectId));
                                }
                            }

                            var firstUnit = player.SelectedUnits.FirstOrDefault();

                            // TODO: use the target to figure out which sound triggers
                            // TODO: handle hordes properly
                            var voiceAttack = firstUnit?.Definition.VoiceAttack?.Value;
                            if (voiceAttack != null)
                            {
                                // only the first unit says their attack line (and only for attacking objects, not ground)
                                _game.Audio.PlayAudioEvent(firstUnit, voiceAttack);
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
                                    unit.CurrentWeapon?.SetTarget(new WeaponTarget(targetPosition));
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
                                var obj = _game.Scene3D.GameObjects.GetObjectById(objId);

                                var rallyPoint = order.Arguments[1].Value.Position;
                                obj.RallyPoint = rallyPoint;
                            }
                            else
                            {
                                var objIds = order.Arguments.Skip(1)
                                    .Select(x => x.Value.ObjectId)
                                    .Select(_game.Scene3D.GameObjects.GetObjectById)
                                    .ToArray();
                                _game.Selection.SetRallyPointForSelectedObjects(player, objIds, new Vector3());
                            }

                            _game.Audio.PlayAudioEvent("RallyPointSet");
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e, "Error while setting rallypoint");
                        }
                        break;
                    case OrderType.SpecialPowerAtLocation:
                        {
                            var specialPowerDefinition = (SpecialPowerType) order.Arguments[0].Value.Integer;
                            var specialPowerLocation = order.Arguments[1].Value.Position;
                            var unknownObjectId = order.Arguments[2].Value.ObjectId;
                            var commandFlags = (SpecialPowerOrderFlags) order.Arguments[3].Value.Integer;
                            var commandCenterSource = order.Arguments[4].Value.ObjectId;

                            SpecialPowerAtLocationApplicator.Execute(specialPowerDefinition,
                                new LocationArguments(player, specialPowerLocation, commandFlags,
                                    _game.GameLogic.GetObjectById(commandCenterSource)), player.SelectedUnits);
                        }
                        break;

                    case OrderType.SpecialPower:
                        {
                            var specialPowerDefinition = (SpecialPowerType) order.Arguments[0].Value.Integer;
                            var commandFlags = (SpecialPowerOrderFlags) order.Arguments[1].Value.Integer;
                            var commandCenterSource = order.Arguments[2].Value.ObjectId; // todo: is this ever used for these commands?

                            SpecialPowerApplicator.Execute(specialPowerDefinition,
                                new SpecialPowerArguments(player, commandFlags), player.SelectedUnits);
                        }
                        break;

                    case OrderType.SpecialPowerAtObject:
                        {
                            var specialPowerDefinition = (SpecialPowerType) order.Arguments[0].Value.Integer;
                            var targetId = order.Arguments[1].Value.ObjectId;
                            var commandFlags = (SpecialPowerOrderFlags) order.Arguments[2].Value.Integer;
                            var commandCenterSource = order.Arguments[3].Value.ObjectId;

                            SpecialPowerAtObjectApplicator.Execute(specialPowerDefinition,
                                new ObjectArguments(player, _game.GameLogic.GetObjectById(targetId), commandFlags,
                                    _game.GameLogic.GetObjectById(commandCenterSource)), player.SelectedUnits);
                        }
                        break;

                    case OrderType.EndGame:
                        _game.EndGame();
                        break;

                    case OrderType.PurchaseScience:
                        var scienceDefinitionId = order.Arguments[0].Value.Integer;
                        var science = _game.AssetStore.Sciences.GetByInternalId(scienceDefinitionId);
                        player.PurchaseScience(science);
                        //TODO: implement
                        break;

                    case OrderType.ExitContainer:
                        var objectIdToExit = order.Arguments[0].Value.ObjectId;
                        foreach (var unit in player.SelectedUnits)
                        {
                            unit.FindBehavior<OpenContainModule>().Remove(objectIdToExit);
                        }
                        break;

                    case OrderType.Evacuate:
                        foreach (var unit in player.SelectedUnits)
                        {
                            unit.FindBehavior<OpenContainModule>().Evacuate();
                        }
                        break;

                    case OrderType.Enter:
                        {
                            if (order.Arguments[0].ArgumentType != OrderArgumentType.ObjectId ||
                                order.Arguments[0].Value.ObjectId != 0)
                            {
                                throw new InvalidOperationException();
                            }

                            var objectDefinitionId = order.Arguments[1].Value.Integer;
                            var gameObject = _game.Scene3D.GameObjects.GetObjectById((uint) objectDefinitionId);

                            var container = gameObject.FindBehavior<OpenContainModule>();
                            foreach (var unit in player.SelectedUnits)
                            {
                                if (!container.CanAddUnit(unit))
                                {
                                    continue; // this unit can't enter the container (kindof doesn't match, or there aren't enough slots)
                                }

                                // TODO: Don't put it in container right now. Tell it to move towards container.
                                //  use AIStateMachine EnterContainerState?
                                //  deselect unit upon entry
                                //unit.AIUpdate.SetTargetObject(gameObject);
                                container.Add(unit);
                            }

                            break;
                        }
                    case OrderType.GatherDumpSupplies:
                        var supplyPointId = order.Arguments[0].Value.Integer;
                        var supplyPoint = _game.Scene3D.GameObjects.GetObjectById((uint) supplyPointId);

                        foreach (var unit in player.SelectedUnits)
                        {
                            var behavior = unit.FindBehavior<SupplyAIUpdate>();

                            if (behavior is null)
                            {
                                continue;
                            }

                            if (supplyPoint.IsKindOf(ObjectKinds.SupplySource))
                            {
                                behavior.CurrentSupplySource = supplyPoint;
                                behavior.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.SearchingForSupplySource;
                            }
                            else // if it's not a supply source, it's a supply center
                            {
                                behavior.CurrentSupplyTarget = supplyPoint;
                                behavior.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.SearchingForSupplyTarget;
                            }
                        }
                        break;

                    case OrderType.HackInternet:
                        {
                            GameObject? lastUnit = null;
                            foreach (var unit in player.SelectedUnits)
                            {
                                if (unit.AIUpdate is not HackInternetAIUpdate hackInternetAIUpdate)
                                {
                                    Logger.Warn("Attempting to hack internet with unit without {HackInternetAIUpdate}",
                                        nameof(HackInternetAIUpdate));
                                    continue;
                                }

                                hackInternetAIUpdate.StartHackingInternet();
                                lastUnit = unit;
                            }

                            _game.Audio.PlayAudioEvent(lastUnit, lastUnit?.Definition.UnitSpecificSounds.VoiceHackInternet?.Value);
                        }
                        break;

                    case OrderType.ToggleOvercharge:
                        foreach (var unit in player.SelectedUnits)
                        {
                            foreach (var overchargeBehavior in unit.FindBehaviors<OverchargeBehavior>())
                            {
                                if (overchargeBehavior.Enabled)
                                {
                                    overchargeBehavior.Deactivate();
                                }
                                else
                                {
                                    overchargeBehavior.Activate();
                                }
                            }
                        }

                        break;
                    case OrderType.Checksum:
                        break;

                    default:
                        var args = string.Join(", ", order.Arguments.Select(argument => argument.ToString()));

                        Logger.Info($"Unimplemented order type: {order.OrderType} ({args})");
                        break;
                }
            }
        }
    }
}
