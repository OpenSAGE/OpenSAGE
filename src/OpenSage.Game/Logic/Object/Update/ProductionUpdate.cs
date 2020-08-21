using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Ini;
using OpenSage.Diagnostics.Util;
using OpenSage.FileFormats;
using OpenSage.Logic.Object.Production;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ProductionUpdate : UpdateModule
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly GameObject _gameObject;
        private readonly ProductionUpdateModuleData _moduleData;
        private readonly List<ProductionJob> _productionQueue = new List<ProductionJob>();

        private DoorState _currentDoorState;
        private TimeSpan _currentStepEnd;

        private GameObject _producedUnit;
        private IProductionExit _productionExit;

        private int _doorIndex;

        private enum DoorState
        {
            Closed,
            WaitingToOpen,
            Opening,
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

        internal override void Update(BehaviorUpdateContext context)
        {
            var time = context.Time;

            // If door is opening, halt production until it's finished opening.
            if (_currentDoorState == DoorState.Opening)
            {
                if (time.TotalTime >= _currentStepEnd)
                {
                    _productionQueue.RemoveAt(0);
                    Logger.Info($"Door waiting open for {_moduleData.DoorWaitOpenTime}");
                    _currentStepEnd = time.TotalTime + _moduleData.DoorWaitOpenTime;
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
                var result = front.Produce((float) time.DeltaTime.TotalMilliseconds);
                if (result == ProductionJobResult.Finished)
                {
                    if (front.Type == ProductionJobType.Unit)
                    {
                        if (_moduleData.NumDoorAnimations > 0 && UsesDoor(front.ObjectDefinition))
                        {
                            Logger.Info($"Door opening for {_moduleData.DoorOpeningTime}");
                            _currentStepEnd = time.TotalTime + _moduleData.DoorOpeningTime;
                            _currentDoorState = DoorState.Opening;

                            SetDoorIndex();

                            GetDoorConditionFlags(out var doorOpening, out var _, out var _);
                            _gameObject.ModelConditionFlags.Set(doorOpening, true);

                            ProduceObject(front);
                        }
                        else
                        {
                            ProduceObject(front);
                            MoveProducedObjectOut();
                            _productionQueue.RemoveAt(0);
                        }
                    }
                    else if (front.Type == ProductionJobType.Upgrade)
                    {
                        GrantUpgrade(front);
                        _productionQueue.RemoveAt(0);
                    }
                }
            }

            switch (_currentDoorState)
            {
                case DoorState.Open:
                    if (time.TotalTime >= _currentStepEnd)
                    {
                        _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
                        if (_productionExit is ParkingPlaceBehaviour)
                        {
                            break; // Door is closed on aircraft death from JetAIUpdate
                        }
                        CloseDoor(time, _doorIndex);
                    }
                    break;

                case DoorState.Closing:
                    if (time.TotalTime >= _currentStepEnd)
                    {
                        Logger.Info($"Door closed");
                        _currentDoorState = DoorState.Closed;
                        GetDoorConditionFlags(out var _, out var _, out var doorClosing);
                        _gameObject.ModelConditionFlags.Set(doorClosing, false);
                    }
                    break;
            }
        }

        public void CloseDoor(TimeInterval time, int doorIndex)
        {
            _doorIndex = doorIndex;
            Logger.Info($"Door closing for {_moduleData.DoorCloseTime}");
            _currentStepEnd = time.TotalTime + _moduleData.DoorCloseTime;
            _currentDoorState = DoorState.Closing;
            GetDoorConditionFlags(out var _, out var doorWaitingOpen, out var doorClosing);
            _gameObject.ModelConditionFlags.Set(doorWaitingOpen, false);
            _gameObject.ModelConditionFlags.Set(doorClosing, true);
            // TODO: What is ModelConditionFlag.Door1WaitingToClose?
        }

        private bool UsesDoor(ObjectDefinition definition)
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

        private void ProduceObject(ProductionJob job)
        {
            switch (job.Type)
            {
                case ProductionJobType.Unit:
                    ProduceObject(job.ObjectDefinition);
                    break;
            }
        }

        private void GrantUpgrade(ProductionJob job)
        {
            switch (job.Type)
            {
                case ProductionJobType.Upgrade:
                    GrantUpgrade(job.UpgradeDefinition);
                    break;
            }
        }

        private void GrantUpgrade(UpgradeTemplate upgradeDefinition)
        {
            _gameObject.Upgrade(upgradeDefinition);
        }

        internal bool CanProduceObject(ObjectDefinition objectDefinition)
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
            if (_productionExit == null) return true;

            if (_productionExit is ParkingPlaceBehaviour parkingPlace)
            {
                return parkingPlace.CanProduceObject(objectDefinition, ProductionQueue);
            }

            return true;
        }

        private void ProduceObject(ObjectDefinition objectDefinition)
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
            if (_productionExit == null)
            {
                // If there's no IProductionExit behavior on this object, don't emit anything.
                return;
            }

            _producedUnit = _gameObject.Parent.Add(objectDefinition, _gameObject.Owner);

            if (_productionExit is ParkingPlaceBehaviour parkingPlace)
            {
                var producedAtHelipad = parkingPlace.ProducedAtHelipad(_producedUnit.Definition);
                _producedUnit.Transform.CopyFrom(parkingPlace.GetUnitCreateTransform(producedAtHelipad).Matrix * _gameObject.Transform.Matrix);

                if (!producedAtHelipad)
                {
                    parkingPlace.AddVehicle(_producedUnit);
                }
                return;
            }

            _producedUnit.Transform.Rotation = _gameObject.Transform.Rotation;
            _producedUnit.Transform.Translation = _gameObject.ToWorldspace(_productionExit.GetUnitCreatePoint());
        }

        private void MoveProducedObjectOut()
        {
            _productionExit ??= _gameObject.FindBehavior<IProductionExit>();
            if (_producedUnit == null) return;

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
                _producedUnit.AIUpdate.AddTargetPoint(_gameObject.ToWorldspace(naturalRallyPoint.Value));
            }

            // Then go to the rally point if it exists
            if (_gameObject.RallyPoint.HasValue)
            {
                _producedUnit.AIUpdate.AddTargetPoint(_gameObject.RallyPoint.Value);
            }

            HandleHarvesterUnitCreation(_producedUnit);

            _producedUnit = null;
        }

        private void HandleHarvesterUnitCreation(GameObject spawnedUnit)
        {
            // a supply target (supply center etc.) just spawned a harvester object
            if (!_gameObject.Definition.KindOf.Get(ObjectKinds.CashGenerator) ||
                !spawnedUnit.Definition.KindOf.Get(ObjectKinds.Harvester) ||
                !(spawnedUnit.AIUpdate is SupplyAIUpdate supplyUpdate))
            {
                return;
            }

            supplyUpdate.SupplyGatherState = SupplyAIUpdate.SupplyGatherStates.SearchForSupplySource;
            supplyUpdate.CurrentSupplyTarget = _gameObject;
        }

        internal void QueueProduction(ObjectDefinition objectDefinition)
        {
            var job = new ProductionJob(objectDefinition);
            _productionQueue.Add(job);
        }

        internal void Spawn(ObjectDefinition objectDefinition)
        {
            var job = new ProductionJob(objectDefinition, instant: true);
            _productionQueue.Insert(0, job);
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
                if (_productionQueue[i].UpgradeDefinition == upgradeDefinition) index = i;
            }

            if (index < 0 || index > _productionQueue.Count) return;

            _productionQueue.RemoveAt(index);
        }

        public bool CanEnque() => _moduleData.MaxQueueEntries == 0 || _productionQueue.Count < _moduleData.MaxQueueEntries;

        internal override void DrawInspector()
        {
            ImGuiUtility.PropertyRow("DoorState", _currentDoorState);
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown = reader.ReadBytes(89);
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
            { "DoorOpeningTime", (parser, x) => x.DoorOpeningTime = parser.ParseTimeMilliseconds() },
            { "DoorWaitOpenTime", (parser, x) => x.DoorWaitOpenTime = parser.ParseTimeMilliseconds() },
            { "DoorCloseTime", (parser, x) => x.DoorCloseTime = parser.ParseTimeMilliseconds() },
            { "ConstructionCompleteDuration", (parser, x) => x.ConstructionCompleteDuration = parser.ParseTimeMilliseconds() },
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
        public TimeSpan DoorOpeningTime { get; private set; }

        /// <summary>
        /// Time the door stays open so units can exit.
        /// </summary>
        public TimeSpan DoorWaitOpenTime { get; private set; }

        /// <summary>
        /// How long doors should be closing for.
        /// </summary>
        public TimeSpan DoorCloseTime { get; private set; }

        /// <summary>
        /// Wait time between units.
        /// </summary>
        public TimeSpan ConstructionCompleteDuration { get; private set; }

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
        [IniEnum("DISABLED_HELD")]
        DisabledHeld,

        [IniEnum("DISABLED_UNDERPOWERED")]
        DisabledUnderpowered,
    }
}
