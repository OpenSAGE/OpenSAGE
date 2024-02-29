using System;
using System.Linq;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Logic.OrderGenerators;
using OpenSage.Logic.Orders;
using OpenSage.Mathematics;

namespace OpenSage.Gui
{
    public static class CommandButtonCallback
    {
        public static void HandleCommand(Game game, CommandButton commandButton, ObjectDefinition objectDefinition, bool cancel = false, int index = 0)
        {
            var playerIndex = game.Scene3D.GetPlayerIndex(game.Scene3D.LocalPlayer);
            Order CreateOrder(OrderType type) => new Order(playerIndex, type);

            var selection = game.Scene3D.LocalPlayer.SelectedUnits;
            var selectedObject = selection.FirstOrDefault();

            CastleBehavior castleBehavior;

            Order order = null;
            switch (commandButton.Command)
            {
                case CommandType.CastleUnpack:
                    //TODO: proper castleUnpack order & spend money of the player
                    castleBehavior = selectedObject.FindBehavior<CastleBehavior>();
                    castleBehavior.Unpack(selectedObject.Owner);
                    game.Scene3D.LocalPlayer.DeselectUnits();
                    break;

                case CommandType.CastleUnpackExplicitObject:
                case CommandType.FoundationConstruct:
                    castleBehavior = selectedObject.FindBehavior<CastleBehavior>();
                    if (castleBehavior != null)
                    {
                        castleBehavior.IsUnpacked = true;
                    }

                    order = CreateOrder(OrderType.BuildObject);
                    order.AddIntegerArgument(objectDefinition.InternalId);
                    order.AddPositionArgument(selectedObject.Translation);
                    order.AddFloatArgument(selectedObject.Yaw + MathUtility.ToRadians(objectDefinition.PlacementViewAngle));

                    game.Scene3D.LocalPlayer.DeselectUnits();
                    break;

                case CommandType.DozerConstruct:
                    game.OrderGenerator.StartConstructBuilding(objectDefinition);
                    break;
                case CommandType.DozerConstructCancel:
                    order = CreateOrder(OrderType.CancelBuild);
                    break;

                case CommandType.ToggleOvercharge:
                    order = CreateOrder(OrderType.ToggleOvercharge);
                    break;

                case CommandType.CancelUnitBuild:
                case CommandType.Sell:
                    order = CreateOrder(OrderType.Sell);
                    break;

                case CommandType.UnitBuild:
                    if (cancel)
                    {
                        order = CreateOrder(OrderType.CancelUnit);
                        order.AddIntegerArgument(index);
                        break;
                    }
                    order = CreateOrder(OrderType.CreateUnit);
                    order.AddIntegerArgument(objectDefinition.InternalId);
                    order.AddIntegerArgument(1);
                    break;

                case CommandType.SetRallyPoint:
                    game.OrderGenerator.SetRallyPoint();
                    break;

                case CommandType.PlayerUpgrade:
                case CommandType.ObjectUpgrade:
                    if (cancel)
                    {
                        order = CreateOrder(OrderType.CancelUpgrade);
                        order.AddIntegerArgument(index);
                        break;
                    }

                    order = CreateOrder(OrderType.BeginUpgrade);
                    //TODO: figure this out correctly
                    if (selection.Count == 0)
                    {
                        break;
                    }

                    order.AddIntegerArgument((int)selection.First().ID);

                    var upgrade = commandButton.Upgrade.Value;
                    order.AddIntegerArgument(upgrade.InternalId);
                    break;

                case CommandType.Stop:
                    // TODO: Also stop construction?
                    order = CreateOrder(OrderType.StopMoving);
                    break;

                case CommandType.SpecialPowerFromCommandCenter:
                    // TODO: This must somehow invoke the special power from the command center
                case CommandType.SpecialPower:
                    {
                        var specialPower = commandButton.SpecialPower.Value;
                        if (commandButton.Options != null)
                        {
                            var needsPos = commandButton.Options.Get(CommandButtonOption.NeedTargetPos);
                            var needAlly = commandButton.Options.Get(CommandButtonOption.NeedTargetAllyObject);
                            var needEnemy = commandButton.Options.Get(CommandButtonOption.NeedTargetEnemyObject);
                            var needNeutral = commandButton.Options.Get(CommandButtonOption.NeedTargetNeutralObject);
                            var needsObject = needAlly || needEnemy || needNeutral;

                            var targetOptions = new BitArray<SpecialPowerTarget>();
                            targetOptions.Set(SpecialPowerTarget.Location, needsPos);
                            targetOptions.Set(SpecialPowerTarget.AllyObject, needAlly);
                            targetOptions.Set(SpecialPowerTarget.EnemyObject, needEnemy);
                            targetOptions.Set(SpecialPowerTarget.NeutralObject, needNeutral);

                            var orderFlags = OrderFlagsForCommandButtonOptions(commandButton.Options);

                            var cursorInformation = new SpecialPowerCursorInformation(specialPower, orderFlags,
                                targetOptions, commandButton.CursorName, commandButton.InvalidCursorName);

                            if (needsPos)
                            {
                                game.OrderGenerator.StartSpecialPowerAtLocation(cursorInformation);
                            }
                            else if (needsObject)
                            {
                                game.OrderGenerator.StartSpecialPowerAtObject(cursorInformation);
                            }
                            else
                            {
                                game.OrderGenerator.StartSpecialPower(cursorInformation);
                            }
                        }
                        else
                        {
                            order = CreateOrder(OrderType.SpecialPower);
                            order.AddIntegerArgument(specialPower.InternalId);
                            order.AddIntegerArgument(0);
                            order.AddObjectIdArgument(0);
                        }
                    }
                    break;

                case CommandType.ToggleGate:
                    //TODO: proper toggle gate order
                    var gateOpenAndCloseBehavior = selectedObject.FindBehavior<GateOpenAndCloseBehavior>();
                    gateOpenAndCloseBehavior.Toggle();
                    break;

                case CommandType.PurchaseScience:
                    var science = commandButton.Science[0];
                    order = CreateOrder(OrderType.PurchaseScience);
                    order.AddIntegerArgument(science.Value.InternalId);
                    break;

                case CommandType.ExitContainer:
                    // this hack is because:
                    // a) the humvee's enter/exit slots start at 3, not 1
                    // b) these exit buttons could theoretically not even be sequential (picture them only on the top or bottom)
                    var orderedExits = selectedObject.Definition.CommandSet.Value.Buttons
                        .Where(kvp => kvp.Value.Value.Command == CommandType.ExitContainer).OrderBy(kvp => kvp.Key);

                    var objectToRemoveIndex = orderedExits.Select((kvp, i) => (kvp.Key, i)).First(t => t.Key == index).i;
                    var objectIdToRemove = selectedObject.FindBehavior<OpenContainModule>().ContainedObjectIds[objectToRemoveIndex];

                    order = CreateOrder(OrderType.ExitContainer);
                    order.AddObjectIdArgument(objectIdToRemove);
                    break;

                case CommandType.Evacuate:
                    order = CreateOrder(OrderType.Evacuate);
                    break;

                case CommandType.HackInternet:
                    order = CreateOrder(OrderType.HackInternet);
                    break;

                default:
                    throw new NotImplementedException();
            }

            if (order != null)
            {
                game.NetworkMessageBuffer.AddLocalOrder(order);
            }
        }

        private static SpecialPowerOrderFlags OrderFlagsForCommandButtonOptions(BitArray<CommandButtonOption> commandButtonOptions)
        {
            SpecialPowerOrderFlags flags = default;

            if (commandButtonOptions.Get(CommandButtonOption.NeedTargetEnemyObject))
            {
                flags |= SpecialPowerOrderFlags.NeedTargetEnemyObject;
            }
            if (commandButtonOptions.Get(CommandButtonOption.NeedTargetEnemyObject))
            {
                flags |= SpecialPowerOrderFlags.NeedTargetEnemyObject;
            }
            if (commandButtonOptions.Get(CommandButtonOption.NeedTargetNeutralObject))
            {
                flags |= SpecialPowerOrderFlags.NeedTargetNeutralObject;
            }
            if (commandButtonOptions.Get(CommandButtonOption.NeedTargetAllyObject))
            {
                flags |= SpecialPowerOrderFlags.NeedTargetAllyObject;
            }
            // if (commandButtonOptions.Get(CommandButtonOption.Unknown1))
            // {
            //     flags |= SpecialPowerOrderFlags.Unknown1;
            // }
            // if (commandButtonOptions.Get(CommandButtonOption.Unknown2))
            // {
            //     flags |= SpecialPowerOrderFlags.Unknown2;
            // }
            if (commandButtonOptions.Get(CommandButtonOption.NeedTargetPos))
            {
                flags |= SpecialPowerOrderFlags.NeedTargetPosition;
            }
            if (commandButtonOptions.Get(CommandButtonOption.NeedUpgrade))
            {
                flags |= SpecialPowerOrderFlags.NeedUpgrade;
            }
            if (commandButtonOptions.Get(CommandButtonOption.NeedSpecialPowerScience))
            {
                flags |= SpecialPowerOrderFlags.NeedSpecialPowerScience;
            }
            if (commandButtonOptions.Get(CommandButtonOption.OkForMultiSelect))
            {
                flags |= SpecialPowerOrderFlags.OkForMultiSelect;
            }
            if (commandButtonOptions.Get(CommandButtonOption.ContextModeCommand))
            {
                flags |= SpecialPowerOrderFlags.ContextModeCommand;
            }
            if (commandButtonOptions.Get(CommandButtonOption.CheckLike))
            {
                flags |= SpecialPowerOrderFlags.CheckLike;
            }
            // if (commandButtonOptions.Get(CommandButtonOption.Unknown3))
            // {
            //     flags |= SpecialPowerOrderFlags.Unknown3;
            // }
            // if (commandButtonOptions.Get(CommandButtonOption.Unknown4))
            // {
            //     flags |= SpecialPowerOrderFlags.Unknown4;
            // }
            if (commandButtonOptions.Get(CommandButtonOption.OptionOne))
            {
                flags |= SpecialPowerOrderFlags.OptionOne;
            }
            if (commandButtonOptions.Get(CommandButtonOption.OptionTwo))
            {
                flags |= SpecialPowerOrderFlags.OptionTwo;
            }
            if (commandButtonOptions.Get(CommandButtonOption.OptionThree))
            {
                flags |= SpecialPowerOrderFlags.OptionThree;
            }

            return flags;
        }
    }
}
