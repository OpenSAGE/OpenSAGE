using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;
using OpenSage.Gui.ControlBar;
using OpenSage.Logic.Object.Production;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ProductionUpdate : UpdateModule
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly GameObject _gameObject;
        private readonly ProductionUpdateModuleData _moduleData;
        private readonly List<ProductionJob> _productionQueue = new();

        private DoorState _currentDoorState;
        private LogicFrame _currentStepEnd;

        private GameObject _producedUnit;
        private IProductionExit _productionExit;

        private int _doorIndex;

        private uint _nextJobId;
        private uint _unknownFrame1;
        private ProductionUpdateSomething[] _unknownSomethings = new ProductionUpdateSomething[4];

        public GameObject ParentHorde;

        private enum DoorState
        {
            Closed,
            WaitingToOpen,
            Opening,
            OpenForHordePayload,
            Open,
            Closing,
        }

        public bool IsProducing => _productionQueue.Count > 0;

        public IReadOnlyList<ProductionJob> ProductionQueue => _productionQueue;

        internal ProductionUpdate(GameObject gameObject, ProductionUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
            _currentDoorState = DoorState.Closed;
        }

        public void CloseDoor()
        {
            _currentDoorState = DoorState.Open;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            // If door is opening, halt production until it's finished opening.
            if (_currentDoorState == DoorState.Opening)
            {
                if (context.LogicFrame >= _currentStepEnd)
                {
                    _productionQueue.RemoveAt(0);
                    Logger.Info($"Door waiting open for {_moduleData.DoorWaitOpenTime}");
                    _currentStepEnd = context.LogicFrame + _moduleData.DoorWaitOpenTime;
                    _currentDoorState = DoorState.Open;

                    GetDoorConditionFlags(out var doorOpening, out var doorWaitingOpen, out var _);
                   
                    _gameObject.ModelConditionFlags.Set(doorOpening, false);
                    _gameObject.ModelConditionFlags.Set(doorWaitingOpen, true);
                    MoveProducedObjectOut();
                }

                return;
            }

            if (_productionQueue.Count > 0)
            {
                var front = _productionQueue[0];
                var result = front.Produce();
                if (result == ProductionJobResult.Finished)
                {
                    if (front.Type == ProductionJobType.Unit)
                    {
                        if (_moduleData.NumDoorAnimations > 0
                            && ExitsThroughDoor(front.ObjectDefinition)
                            && (_currentDoorState != DoorState.OpenForHordePayload))
                        {
                            Logger.Info($"Door opening for {_moduleData.DoorOpeningTime}");
                            _currentStepEnd = context.LogicFrame + _moduleData.DoorOpeningTime;
                            _currentDoorState = DoorState.Opening;

                            SetDoorIndex();

                            GetDoorConditionFlags(out var doorOpening, out var _, out var _);
                            _gameObject.ModelConditionFlags.Set(doorOpening, true);

                            ProduceObject(front.ObjectDefinition);
                        }
                        else
                        {
                            ProduceObject(front.ObjectDefinition);
                            MoveProducedObjectOut();
                            _productionQueue.RemoveAt(0);
                        }
                    }
                    else if (front.Type == ProductionJobType.Upgrade)
                    {
                        front.UpgradeDefinition.GrantUpgrade(_gameObject);
                        _productionQueue.RemoveAt(0);
                    }
                }
            }

            switch (_currentDoorState)
            {
                case DoorState.Open:
                    if (context.LogicFrame >= _currentStepEnd)
                    {
                        _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
                        if (_productionExit is ParkingPlaceBehaviour)
                        {
                            break; // Door is closed on aircraft death from JetAIUpdate
                        }
                        CloseDoor(_doorIndex);
                    }
                    break;

                case DoorState.Closing:
                    if (context.LogicFrame >= _currentStepEnd)
                    {
                        Logger.Info($"Door closed");
                        _currentDoorState = DoorState.Closed;
                        GetDoorConditionFlags(out var _, out var _, out var doorClosing);
                        _gameObject.ModelConditionFlags.Set(doorClosing, false);
                    }
                    break;
                case DoorState.OpenForHordePayload:
                    break; //door is closed again by HordeContain
            }
        }

        public void CloseDoor(int doorIndex)
        {
            _doorIndex = doorIndex;
            Logger.Info($"Door closing for {_moduleData.DoorCloseTime}");
            _currentStepEnd = _gameObject.GameContext.GameLogic.CurrentFrame + _moduleData.DoorCloseTime;
            _currentDoorState = DoorState.Closing;
            GetDoorConditionFlags(out var _, out var doorWaitingOpen, out var doorClosing);
            _gameObject.ModelConditionFlags.Set(doorWaitingOpen, false);
            _gameObject.ModelConditionFlags.Set(doorClosing, true);
            // TODO: What is ModelConditionFlag.Door1WaitingToClose?
        }

        private bool ExitsThroughDoor(ObjectDefinition definition)
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
            if (_productionExit is ParkingPlaceBehaviour parkingPlace)
            {
                return !parkingPlace.ProducedAtHelipad(definition);
            }
            return true;
        }

        private void SetDoorIndex()
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
            if (_productionExit is ParkingPlaceBehaviour parkingPlace)
            {
                _doorIndex = parkingPlace.NextFreeSlot();
            }
        }

        private void GetDoorConditionFlags(out ModelConditionFlag opening, out ModelConditionFlag waitingOpen, out ModelConditionFlag closing)
        {
            opening = ModelConditionFlag.Door1Opening;
            waitingOpen = ModelConditionFlag.Door1WaitingOpen;
            closing = ModelConditionFlag.Door1Closing;

            switch (_doorIndex)
            {
                case 0:
                    break;
                case 1:
                    opening = ModelConditionFlag.Door2Opening;
                    waitingOpen = ModelConditionFlag.Door2WaitingOpen;
                    closing = ModelConditionFlag.Door2Closing;
                    break;
                case 2:
                    opening = ModelConditionFlag.Door3Opening;
                    waitingOpen = ModelConditionFlag.Door3WaitingOpen;
                    closing = ModelConditionFlag.Door3Closing;
                    break;
                case 3:
                    opening = ModelConditionFlag.Door4Opening;
                    waitingOpen = ModelConditionFlag.Door4WaitingOpen;
                    closing = ModelConditionFlag.Door4Closing;
                    break;
            }
        }

        internal bool CanProduceObject(ObjectDefinition objectDefinition)
        {
            // only one hero of the same kind can be produced at a time
            if (objectDefinition.KindOf.Get(ObjectKinds.Hero))
            {
                foreach (var job in ProductionQueue)
                {
                    if (job.ObjectDefinition == objectDefinition)
                    {
                        return false;
                    }
                }
            }

            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();

            if (_productionExit != null && _productionExit is ParkingPlaceBehaviour parkingPlace)
            {
                return parkingPlace.CanProduceObject(objectDefinition, ProductionQueue);
            }

            return true;
        }

        public (int, float) GetCountAndProgress(CommandButton button)
        {
            //check upgrades here first (object upgrades e.g. upgrade barracks to level 2 have object AND upgrade)
            if (button.Upgrade != null && button.Upgrade.Value != null)
            {
                return GetCountAndProgress(button.Upgrade.Value);
            }
            if (button.Object != null && button.Object.Value != null)
            {
                return GetCountAndProgress(button.Object.Value);
            }
            return (0, 0.0f);
        }

        private (int, float) GetCountAndProgress(ObjectDefinition objectDefinition)
        {
            var progress = 0.0f;
            var count = _productionQueue.Where(x => x.ObjectDefinition != null && objectDefinition.Name == x.ObjectDefinition.Name).Count();
            var currentJob = _productionQueue[0];
            if (currentJob.ObjectDefinition != null && objectDefinition.Name == currentJob.ObjectDefinition.Name)
            {
                progress = currentJob.Progress;
            }

            return (count, progress);
        }

        private (int, float) GetCountAndProgress(UpgradeTemplate upgradeTemplate)
        {
            var progress = 0.0f;
            var count = _productionQueue.Where(x => x.UpgradeDefinition != null && upgradeTemplate.Name == x.UpgradeDefinition.Name).Count();
            var currentJob = _productionQueue[0];
            if (currentJob.UpgradeDefinition != null && upgradeTemplate.Name == currentJob.UpgradeDefinition.Name)
            {
                progress = currentJob.Progress;
            }

            return (count, progress);
        }

        private void ProduceObject(ObjectDefinition objectDefinition)
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
            if (_productionExit == null)
            {
                // If there's no IProductionExit behavior on this object, don't emit anything.
                return;
            }

            _producedUnit = _gameObject.GameContext.GameLogic.CreateObject(objectDefinition, _gameObject.Owner);
            _producedUnit.Owner = _gameObject.Owner;
            _producedUnit.ParentHorde = ParentHorde;

            if (!_moduleData.GiveNoXP)
            {
                _gameObject.GainExperience((int)_producedUnit.Definition.BuildCost);
            }

            var isHorde = _producedUnit.Definition.KindOf.Get(ObjectKinds.Horde);
            if (isHorde)
            {
                var hordeContain = _producedUnit.FindBehavior<HordeContainBehavior>();
                ParentHorde = _producedUnit;
                hordeContain.EnqueuePayload(this, ((QueueProductionExitUpdate)_productionExit).ExitDelay);
            }

            if (_productionExit is ParkingPlaceBehaviour parkingPlace)
            {
                var producedAtHelipad = parkingPlace.ProducedAtHelipad(_producedUnit.Definition);
                _producedUnit.SetTransformMatrix(parkingPlace.GetUnitCreateTransform(producedAtHelipad).Matrix * _gameObject.TransformMatrix);

                if (!producedAtHelipad)
                {
                    parkingPlace.AddVehicle(_producedUnit);
                }
                return;
            }

            _producedUnit.UpdateTransform(_gameObject.ToWorldspace(_productionExit.GetUnitCreatePoint()), _gameObject.Rotation);
        }

        private void MoveProducedObjectOut()
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
            if (_producedUnit == null)
            {
                return;
            }

            if (_productionExit is ParkingPlaceBehaviour parkingPlace && !parkingPlace.ProducedAtHelipad(_producedUnit.Definition))
            {
                parkingPlace.ParkVehicle(_producedUnit);
                _producedUnit = null;
                return;
            }

            // First go to the natural rally point
            var naturalRallyPoint = _productionExit.GetNaturalRallyPoint();
            if (naturalRallyPoint.HasValue)
            {
                naturalRallyPoint = _gameObject.ToWorldspace(naturalRallyPoint.Value);
                _producedUnit.AIUpdate.AddTargetPoint(naturalRallyPoint.Value);
            }

            // Then go to the rally point if it exists
            if (_gameObject.RallyPoint.HasValue)
            {
                _producedUnit.AIUpdate.AddTargetPoint(_gameObject.RallyPoint.Value);
            }

            HandleHordeCreation();
            HandleHarvesterUnitCreation(_gameObject, _producedUnit);

            _producedUnit = null;
        }

        private void HandleHordeCreation()
        {
            if (_producedUnit.Definition.KindOf.Get(ObjectKinds.Horde))
            {
                _currentDoorState = DoorState.OpenForHordePayload;
            }
            else if (_producedUnit.ParentHorde != null)
            {
                var hordeContain = _producedUnit.ParentHorde.FindBehavior<HordeContainBehavior>();
                hordeContain.Register(_producedUnit);

                var count = _producedUnit.AIUpdate.TargetPoints.Count;
                var direction = _producedUnit.AIUpdate.TargetPoints[count - 1] - _producedUnit.Translation;
                if (count > 1)
                {
                    direction = _producedUnit.AIUpdate.TargetPoints[count - 1] - _producedUnit.AIUpdate.TargetPoints[count - 2];
                }

                var formationOffset = hordeContain.GetFormationOffset(_producedUnit);
                var offset = Vector3.Transform(formationOffset, Quaternion.CreateFromYawPitchRoll(MathUtility.GetYawFromDirection(direction.Vector2XY()), 0, 0));
                _producedUnit.AIUpdate.AddTargetPoint(_producedUnit.AIUpdate.TargetPoints[count - 1] + offset);
                _producedUnit.AIUpdate.SetTargetDirection(direction);
            }
        }

        public static void HandleHarvesterUnitCreation(GameObject producer, GameObject producedUnit)
        {
            // a supply target (supply center etc.) just spawned a harvester object
            if (!producer.Definition.KindOf.Get(ObjectKinds.CashGenerator) && !producer.Definition.KindOf.Get(ObjectKinds.SupplyGatheringCenter)
                || !producedUnit.Definition.KindOf.Get(ObjectKinds.Harvester)
                || !(producedUnit.AIUpdate is SupplyAIUpdate supplyUpdate))
            {
                return;
            }

            supplyUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.SearchingForSupplySource;
            supplyUpdate.CurrentSupplyTarget = producer;
        }

        internal void QueueProduction(ObjectDefinition objectDefinition)
        {
            var job = new ProductionJob(objectDefinition, objectDefinition.BuildTime / _gameObject.ProductionModifier);
            _productionQueue.Add(job);

            // TODO: Set ModelConditionFlag.ActivelyConstructing.
        }

        internal void Spawn(ObjectDefinition objectDefinition)
        {
            var job = new ProductionJob(objectDefinition, LogicFrameSpan.Zero);
            _productionQueue.Insert(0, job);
        }

        internal void SpawnPayload(ObjectDefinition objectDefinition, LogicFrameSpan buildTime)
        {
            var job = new ProductionJob(objectDefinition, buildTime / _gameObject.ProductionModifier);
            _productionQueue.Insert(1, job);
        }

        public void CancelProduction(int index)
        {
            if (index < _productionQueue.Count)
            {
                _productionQueue.RemoveAt(index);
            }
        }

        internal void QueueUpgrade(UpgradeTemplate upgradeDefinition)
        {
            var job = new ProductionJob(upgradeDefinition);
            _productionQueue.Add(job);
        }

        internal void CancelUpgrade(UpgradeTemplate upgradeDefinition)
        {
            var index = -1;
            for (var i = 0; i < _productionQueue.Count; i++)
            {
                if (_productionQueue[i].UpgradeDefinition == upgradeDefinition)
                {
                    index = i;
                }
            }

            if (index < 0 || index > _productionQueue.Count)
            {
                return;
            }

            _productionQueue.RemoveAt(index);
        }

        public bool CanEnqueue() => _moduleData.MaxQueueEntries == 0 || _productionQueue.Count < _moduleData.MaxQueueEntries;

        internal override void DrawInspector()
        {
            ImGuiUtility.ComboEnum("DoorState", ref _currentDoorState);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistList(_productionQueue, static (StatePersister persister, ref ProductionJob item) =>
            {
                persister.BeginObject();

                var productionJobType = item?.Type ?? default;
                persister.PersistEnum(ref productionJobType, "JobType");

                var templateName = item != null
                    ? item.Type == ProductionJobType.Unit
                        ? item.ObjectDefinition.Name
                        : item.UpgradeDefinition.Name
                    : null;
                persister.PersistAsciiString(ref templateName);

                if (persister.Mode == StatePersistMode.Read)
                {
                    item = productionJobType switch
                    {
                        ProductionJobType.Unit => new ProductionJob(persister.AssetStore.ObjectDefinitions.GetByName(templateName), LogicFrameSpan.Zero),
                        ProductionJobType.Upgrade => new ProductionJob(persister.AssetStore.Upgrades.GetByName(templateName)),
                        _ => throw new InvalidStateException(),
                    };
                }

                persister.PersistObject(item, "Job");

                persister.EndObject();
            });

            reader.PersistUInt32(ref _nextJobId);

            var productionJobCount2 = (uint)_productionQueue.Count;
            reader.PersistUInt32(ref productionJobCount2);
            if (productionJobCount2 != _productionQueue.Count)
            {
                throw new InvalidStateException();
            }

            reader.PersistFrame(ref _unknownFrame1);

            reader.PersistArray(
                _unknownSomethings,
                static (StatePersister persister, ref ProductionUpdateSomething item) =>
                {
                    persister.PersistObjectValue(ref item);
                });

            reader.BeginArray("UnknownArray");
            for (var i = 0; i < 2; i++)
            {
                var unknown1 = true;
                reader.PersistBooleanValue(ref unknown1);
                if (!unknown1)
                {
                    throw new InvalidStateException();
                }

                reader.SkipUnknownBytes(4);
            }
            reader.EndArray();

            reader.SkipUnknownBytes(1);
        }
    }

    /// <summary>
    /// Required on an object that uses PublicTimer code for any SpecialPower and/or required for 
    /// units/structures with object upgrades.
    /// </summary>
    public sealed class ProductionUpdateModuleData : UpdateModuleData
    {
        internal static ProductionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProductionUpdateModuleData> FieldParseTable = new IniParseTable<ProductionUpdateModuleData>
        {
            { "NumDoorAnimations", (parser, x) => x.NumDoorAnimations = parser.ParseInteger() },
            { "DoorOpeningTime", (parser, x) => x.DoorOpeningTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "DoorWaitOpenTime", (parser, x) => x.DoorWaitOpenTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "DoorCloseTime", (parser, x) => x.DoorCloseTime = parser.ParseTimeMillisecondsToLogicFrames() },
            { "ConstructionCompleteDuration", (parser, x) => x.ConstructionCompleteDuration = parser.ParseTimeMillisecondsToLogicFrames() },
            { "MaxQueueEntries", (parser, x) => x.MaxQueueEntries = parser.ParseInteger() },
            { "QuantityModifier", (parser, x) => x.QuantityModifier = Object.QuantityModifier.Parse(parser) },

            { "DisabledTypesToProcess", (parser, x) => x.DisabledTypesToProcess = parser.ParseEnumBitArray<DisabledType>() },
            { "VeteranUnitsFromVeteranFactory", (parser, x) => x.VeteranUnitsFromVeteranFactory = parser.ParseBoolean() },
            { "SetBonusModelConditionOnSpeedBonus", (parser, x) => x.SetBonusModelConditionOnSpeedBonus = parser.ParseBoolean() },
            { "BonusForType", (parser, x) => x.BonusForType = parser.ParseString() },
            { "SpeedBonusAudioLoop", (parser, x) => x.SpeedBonusAudioLoop = parser.ParseAssetReference() },
            { "UnitInvulnerableTime", (parser, x) => x.UnitInvulnerableTime = parser.ParseInteger() },
            { "GiveNoXP", (parser, x) => x.GiveNoXP = parser.ParseBoolean() },
            { "SpecialPrepModelconditionTime", (parser, x) => x.SpecialPrepModelconditionTime = parser.ParseInteger() },
            { "ProductionModifier", (parser, x) => x.ProductionModifiers.Add(ProductionModifier.Parse(parser)) }
        };

        /// <summary>
        /// Specifies how many doors to use when unit training is complete.
        /// Valid values are between 0 and 4 inclusive.
        /// </summary>
        public int NumDoorAnimations { get; private set; }

        /// <summary>
        /// How long doors should be opening for.
        /// </summary>
        public LogicFrameSpan DoorOpeningTime { get; private set; }

        /// <summary>
        /// Time the door stays open so units can exit.
        /// </summary>
        public LogicFrameSpan DoorWaitOpenTime { get; private set; }

        /// <summary>
        /// How long doors should be closing for.
        /// </summary>
        public LogicFrameSpan DoorCloseTime { get; private set; }

        /// <summary>
        /// Wait time between units.
        /// </summary>
        public LogicFrameSpan ConstructionCompleteDuration { get; private set; }

        public int MaxQueueEntries { get; private set; }

        /// <summary>
        /// Red Guards use this so that they can come out of the barracks in pairs.
        /// </summary>
        public QuantityModifier? QuantityModifier { get; private set; }

        public BitArray<DisabledType> DisabledTypesToProcess { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool VeteranUnitsFromVeteranFactory { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SetBonusModelConditionOnSpeedBonus { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BonusForType { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SpeedBonusAudioLoop { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int UnitInvulnerableTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool GiveNoXP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SpecialPrepModelconditionTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<ProductionModifier> ProductionModifiers { get; } = new List<ProductionModifier>();

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ProductionUpdate(gameObject, this);
        }
    }

    public struct QuantityModifier
    {
        internal static QuantityModifier Parse(IniParser parser)
        {
            return new QuantityModifier
            {
                ObjectName = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string ObjectName { get; private set; }
        public int Count { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class ProductionModifier
    {
        internal static ProductionModifier Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProductionModifier> FieldParseTable = new IniParseTable<ProductionModifier>
        {
            { "RequiredUpgrade", (parser, x) => x.RequiredUpgrade = parser.ParseAssetReference() },
            { "CostMultiplier", (parser, x) => x.CostMultiplier = parser.ParseFloat() },
            { "TimeMultiplier", (parser, x) => x.TimeMultiplier = parser.ParseFloat() },
            { "ModifierFilter", (parser, x) => x.ModifierFilter = ObjectFilter.Parse(parser) },
            { "HeroPurchase", (parser, x) => x.HeroPurchase = parser.ParseBoolean() },
            { "HeroRevive", (parser, x) => x.HeroRevive = parser.ParseBoolean() }
        };

        public string RequiredUpgrade { get; private set; }
        public float CostMultiplier { get; private set; }
        public float TimeMultiplier { get; private set; }
        public ObjectFilter ModifierFilter { get; private set; }
        public bool HeroPurchase { get; private set; }
        public bool HeroRevive { get; private set; }
    }

    public enum DisabledType
    {
        [IniEnum("DEFAULT")]
        Default,

        UserParalyzed,

        [IniEnum("DISABLED_EMP")]
        Emp,

        [IniEnum("DISABLED_HELD")]
        Held,

        [IniEnum("DISABLED_PARALYZED")]
        Paralyzed,

        [IniEnum("DISABLED_UNMANNED")]
        Unmanned,

        [IniEnum("DISABLED_UNDERPOWERED")]
        Underpowered,

        [IniEnum("DISABLED_FREEFALL")]
        Freefall,

        [IniEnum("DISABLED_SCRIPT_DISABLED")]
        ScriptDisabled,

        [IniEnum("DISABLED_SCRIPT_UNDERPOWERED")]
        ScriptUnderpowered,

        TemporarilyBusy,

        Infiltrated,
    }

    internal struct ProductionUpdateSomething : IPersistableObject
    {
        private uint _unknownFrame1;
        private uint _unknownFrame2;
        private uint _unknownFrame3;

        public void Persist(StatePersister reader)
        {
            reader.PersistFrame(ref _unknownFrame1);
            reader.PersistFrame(ref _unknownFrame2);
            reader.PersistFrame(ref _unknownFrame3);

            reader.SkipUnknownBytes(4);
        }
    }
}
