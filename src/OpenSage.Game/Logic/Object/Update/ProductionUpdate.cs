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
                    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Opening, false);
                    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1WaitingOpen, true);
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
                        if (_moduleData.NumDoorAnimations > 0)
                        {
                            Logger.Info($"Door opening for {_moduleData.DoorOpeningTime}");
                            _currentStepEnd = time.TotalTime + _moduleData.DoorOpeningTime;
                            _currentDoorState = DoorState.Opening;
                            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Opening, true);

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
                        Logger.Info($"Door closing for {_moduleData.DoorCloseTime}");
                        _currentStepEnd = time.TotalTime + _moduleData.DoorCloseTime;
                        _currentDoorState = DoorState.Closing;
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1WaitingOpen, false);
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Closing, true);
                        // TODO: What is ModelConditionFlag.Door1WaitingToClose?
                    }
                    break;

                case DoorState.Closing:
                    if (time.TotalTime >= _currentStepEnd)
                    {
                        Logger.Info($"Door closed");
                        _currentDoorState = DoorState.Closed;
                        _gameObject.ModelConditionFlags.Set(ModelConditionFlag.Door1Closing, false);
                    }
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

        private void ProduceObject(ObjectDefinition objectDefinition)
        {
            var productionExit = _gameObject.FindBehavior<IProductionExit>();

            if (productionExit == null)
            {
                // If there's no IProductionExit behavior on this object, don't emit anything.
                return;
            }

            _producedUnit = _gameObject.Parent.Add(objectDefinition, _gameObject.Owner);
            _producedUnit.Transform.Rotation = _gameObject.Transform.Rotation;
            _producedUnit.Transform.Translation = _gameObject.ToWorldspace(productionExit.GetUnitCreatePoint());

            var parkingPlace = productionExit as ParkingPlaceBehaviour;
            if (parkingPlace != null)
            {
                parkingPlace.ParkVehicle(_producedUnit);
            }
        }

        private void MoveProducedObjectOut()
        {
            if (_producedUnit == null)
            {
                return;
            }

            var productionExit = _gameObject.FindBehavior<IProductionExit>();

            // First go to the natural rally point
            var naturalRallyPoint = productionExit.GetNaturalRallyPoint();
            if (naturalRallyPoint.HasValue)
            {
                _producedUnit.AIUpdate.AddTargetPoint(_gameObject.ToWorldspace(naturalRallyPoint.Value));
            }

            // Then go to the rally point if it exists
            if (_gameObject.RallyPoint.HasValue)
            {
                _producedUnit.AIUpdate.AddTargetPoint(_gameObject.RallyPoint.Value);
            }

            _producedUnit = null;
        }

        internal void QueueProduction(ObjectDefinition objectDefinition)
        {
            var job = new ProductionJob(objectDefinition);
            _productionQueue.Add(job);
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
