#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
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

        private IProductionExit? _productionExit;
        private IProductionExit? ProductionExit => _productionExit ??= _gameObject.FindBehavior<IProductionExit>();

        private int _doorIndex;

        private uint _nextJobId;
        private uint _unknownFrame1;
        private readonly DoorStatus[] _doorStatuses = new DoorStatus[4];

        // todo: persist or remove
        public GameObject? ParentHorde;

        private enum DoorState
        {
            Closed,
            Opening,
            OpenForHordePayload,
            WaitingOpen,
            Closing,
        }

        public bool IsProducing => _productionQueue.Count > 0;

        public IReadOnlyList<ProductionJob> ProductionQueue => _productionQueue;

        internal ProductionUpdate(GameObject gameObject, ProductionUpdateModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        public void CloseDoor()
        {
            // todo: update with bfme save data
            // _currentDoorState = DoorState.WaitingOpen;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var (currentDoorState, doorStateEndFrame) = GetDoorStatus();

            // If door is opening, halt production until it's finished opening.
            if (currentDoorState == DoorState.Opening)
            {
                if (context.LogicFrame >= doorStateEndFrame)
                {
                    var newObject = _productionQueue[0];
                    ProduceAndMoveOut(newObject.ObjectDefinition);
                    _productionQueue.RemoveAt(0);
                    Logger.Info($"Door waiting open for {_moduleData.DoorWaitOpenTime}");
                    SetDoorStateEndFrame(DoorState.WaitingOpen, context.LogicFrame + _moduleData.DoorWaitOpenTime);
                    UpdateDoorModelConditionFlags();
                }

                return;
            }

            var isProducing = _productionQueue.Count > 0;
            _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ActivelyConstructing, isProducing);

            if (isProducing)
            {
                var front = _productionQueue[0];
                front.Update();

                if (ProductionExit is { CanProduce: true })
                {
                    var result = front.Produce();
                    if (result is ProductionJobResult.UnitReady or ProductionJobResult.Finished)
                    {
                        switch (front.Type)
                        {
                            case ProductionJobType.Unit when _moduleData.NumDoorAnimations > 0
                                                             && ExitsThroughDoor(front.ObjectDefinition)
                                                             && (currentDoorState != DoorState.OpenForHordePayload):
                                Logger.Info($"Door opening for {_moduleData.DoorOpeningTime}");

                                SetDoorIndex();

                                SetDoorStateEndFrame(DoorState.Opening, context.LogicFrame + _moduleData.DoorOpeningTime);
                                UpdateDoorModelConditionFlags();
                                _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ConstructionComplete, true);

                                return; // don't empty the queue - that's handled further up
                            case ProductionJobType.Unit:
                                // don't play audio for subsequent spawns (only for first)
                                ProduceAndMoveOut(front.ObjectDefinition, front.UnitsProduced <= 1);
                                break;
                            case ProductionJobType.Upgrade:
                            {
                                front.UpgradeDefinition.GrantUpgrade(_gameObject);
                                if (front.UpgradeDefinition.ResearchSound != null)
                                {
                                    // todo: if null, trigger DialogEvent EvaUSA_UpgradeComplete?
                                    context.GameContext.AudioSystem.PlayAudioEvent(front.UpgradeDefinition.ResearchSound.Value);
                                }

                                break;
                            }
                        }
                    }

                    if (result is ProductionJobResult.Finished)
                    {
                        _productionQueue.RemoveAt(0);
                    }
                }
            }

            switch (currentDoorState)
            {
                case DoorState.WaitingOpen when context.LogicFrame >= doorStateEndFrame:
                    _gameObject.ModelConditionFlags.Set(ModelConditionFlag.ConstructionComplete, false);
                    if (ProductionExit is ParkingPlaceBehaviour)
                    {
                        break; // Door is closed on aircraft death from JetAIUpdate
                    }
                    CloseDoor(_doorIndex);
                    break;

                case DoorState.Closing when context.LogicFrame >= doorStateEndFrame:
                    Logger.Info($"Door closed");
                    SetDoorStateEndFrame(DoorState.Closed, default);
                    UpdateDoorModelConditionFlags();
                    break;
                case DoorState.OpenForHordePayload:
                    break; //door is closed again by HordeContain
            }
        }

        public void CloseDoor(int doorIndex)
        {
            _doorIndex = doorIndex;
            Logger.Info($"Door closing for {_moduleData.DoorCloseTime}");
            SetDoorStateEndFrame(DoorState.Closing, _gameObject.GameContext.GameLogic.CurrentFrame + _moduleData.DoorCloseTime);
            // TODO: What is ModelConditionFlag.Door1WaitingToClose?
        }

        private void SetDoorStateEndFrame(DoorState doorState, LogicFrame frame)
        {
            var doorStatus = new DoorStatus();

            switch (doorState)
            {
                case DoorState.Opening:
                    doorStatus.DoorOpeningUntil = frame;
                    break;
                case DoorState.WaitingOpen:
                    doorStatus.DoorWaitingOpenUntil = frame;
                    break;
                case DoorState.Closing:
                    doorStatus.DoorClosingUntil = frame;
                    break;
                case DoorState.Closed:
                    // resets door state
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(doorState), doorState, null);
            }

            _doorStatuses[_doorIndex] = doorStatus;

            UpdateDoorModelConditionFlags();
        }

        private (DoorState DoorState, LogicFrame EndFrame) GetDoorStatus()
        {
            var doorStatus = _doorStatuses[_doorIndex];

            if (doorStatus.DoorOpeningUntil > LogicFrame.Zero)
            {
                return (DoorState.Opening, doorStatus.DoorOpeningUntil);
            }

            if (doorStatus.DoorWaitingOpenUntil > LogicFrame.Zero)
            {
                return (DoorState.WaitingOpen, doorStatus.DoorWaitingOpenUntil);
            }

            if (doorStatus.DoorClosingUntil > LogicFrame.Zero)
            {
                return (DoorState.Closing, doorStatus.DoorClosingUntil);
            }

            return (DoorState.Closed, LogicFrame.Zero);
        }

        private void UpdateDoorModelConditionFlags()
        {
            var (doorState, _) = GetDoorStatus();

            GetDoorConditionFlags(out var opening, out var waitingOpen, out var closing);

            _gameObject.ModelConditionFlags.Set(opening, doorState is DoorState.Opening);
            _gameObject.ModelConditionFlags.Set(waitingOpen, doorState is DoorState.WaitingOpen);
            _gameObject.ModelConditionFlags.Set(closing, doorState is DoorState.Closing);
        }

        private bool ExitsThroughDoor(ObjectDefinition definition)
        {
            if (ProductionExit is ParkingPlaceBehaviour parkingPlace)
            {
                return !parkingPlace.ProducedAtHelipad(definition);
            }
            return true;
        }

        private void SetDoorIndex()
        {
            if (ProductionExit is ParkingPlaceBehaviour parkingPlace)
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

            if (ProductionExit is ParkingPlaceBehaviour parkingPlace)
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

        private void ProduceAndMoveOut(ObjectDefinition objectDefinition, bool playAudio = true)
        {
            var producedUnit = ProduceObject(objectDefinition, playAudio);

            if (producedUnit != null)
            {
                MoveProducedObjectOut(producedUnit);
            }
        }

        private GameObject? ProduceObject(ObjectDefinition objectDefinition, bool playAudio)
        {
            if (ProductionExit == null)
            {
                // If there's no IProductionExit behavior on this object, don't emit anything.
                // if we're not ready to spawn a unit, then sit tight
                return null;
            }

            ProductionExit.ProduceUnit();

            var producedUnit = _gameObject.GameContext.GameLogic.CreateObject(objectDefinition, _gameObject.Owner);
            producedUnit.Owner = _gameObject.Owner;
            producedUnit.ParentHorde = ParentHorde;

            if (playAudio)
            {
                producedUnit.GameContext.Scene3D.Audio.PlayAudioEvent(producedUnit, producedUnit.Definition.UnitSpecificSounds?.VoiceCreate?.Value);
            }

            if (!_moduleData.GiveNoXP)
            {
                _gameObject.GainExperience((int)producedUnit.Definition.BuildCost);
            }

            var isHorde = producedUnit.Definition.KindOf.Get(ObjectKinds.Horde);
            if (isHorde && ProductionExit is QueueProductionExitUpdate queueProductionExitUpdate)
            {
                var hordeContain = producedUnit.FindBehavior<HordeContainBehavior>();
                ParentHorde = producedUnit;
                hordeContain.EnqueuePayload(this, queueProductionExitUpdate.ExitDelay);
            }

            if (ProductionExit is ParkingPlaceBehaviour parkingPlace)
            {
                var producedAtHelipad = parkingPlace.ProducedAtHelipad(producedUnit.Definition);
                producedUnit.SetTransformMatrix(parkingPlace.GetUnitCreateTransform(producedAtHelipad).Matrix * _gameObject.TransformMatrix);

                if (!producedAtHelipad)
                {
                    parkingPlace.AddVehicle(producedUnit);
                }
                return producedUnit;
            }

            producedUnit.UpdateTransform(_gameObject.ToWorldspace(ProductionExit.GetUnitCreatePoint()), _gameObject.Rotation);

            return producedUnit;
        }

        private void MoveProducedObjectOut(GameObject producedUnit)
        {
            if (ProductionExit is ParkingPlaceBehaviour parkingPlace && !parkingPlace.ProducedAtHelipad(producedUnit.Definition))
            {
                parkingPlace.ParkVehicle(producedUnit);
                return;
            }

            // First go to the natural rally point
            var naturalRallyPoint = ProductionExit?.GetNaturalRallyPoint();
            if (naturalRallyPoint.HasValue)
            {
                naturalRallyPoint = _gameObject.ToWorldspace(naturalRallyPoint.Value);
                producedUnit.AIUpdate.AddTargetPoint(naturalRallyPoint.Value);
            }

            // Then go to the rally point if it exists
            if (_gameObject.RallyPoint.HasValue)
            {
                producedUnit.AIUpdate.AddTargetPoint(_gameObject.RallyPoint.Value);
            }

            _gameObject.GameContext.AudioSystem.PlayAudioEvent(producedUnit, producedUnit.Definition.SoundMoveStart.Value);

            HandleHordeCreation(producedUnit);
            HandleHarvesterUnitCreation(_gameObject, producedUnit);
        }

        private void HandleHordeCreation(GameObject producedUnit)
        {
            if (producedUnit.Definition.KindOf.Get(ObjectKinds.Horde))
            {
                // todo: update with bfme save data
                // _currentDoorState = DoorState.OpenForHordePayload;
            }
            else if (producedUnit.ParentHorde != null)
            {
                var hordeContain = producedUnit.ParentHorde.FindBehavior<HordeContainBehavior>();
                hordeContain.Register(producedUnit);

                var count = producedUnit.AIUpdate.TargetPoints.Count;
                var direction = producedUnit.AIUpdate.TargetPoints[count - 1] - producedUnit.Translation;
                if (count > 1)
                {
                    direction = producedUnit.AIUpdate.TargetPoints[count - 1] - producedUnit.AIUpdate.TargetPoints[count - 2];
                }

                var formationOffset = hordeContain.GetFormationOffset(producedUnit);
                var offset = Vector3.Transform(formationOffset, Quaternion.CreateFromYawPitchRoll(MathUtility.GetYawFromDirection(direction.Vector2XY()), 0, 0));
                producedUnit.AIUpdate.AddTargetPoint(producedUnit.AIUpdate.TargetPoints[count - 1] + offset);
                producedUnit.AIUpdate.SetTargetDirection(direction);
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
            var job = new ProductionJob(objectDefinition, objectDefinition.BuildTime / _gameObject.ProductionModifier, _nextJobId++,
                _moduleData.QuantityModifiers.TryGetValue(objectDefinition.Name, out var quantity) ? quantity : 1);
            _productionQueue.Add(job);

            // TODO: Set ModelConditionFlag.ActivelyConstructing.
        }

        internal void Spawn(ObjectDefinition objectDefinition)
        {
            var job = new ProductionJob(objectDefinition, LogicFrameSpan.Zero, _nextJobId++);
            _productionQueue.Insert(0, job);
        }

        internal void SpawnPayload(ObjectDefinition objectDefinition, LogicFrameSpan buildTime)
        {
            var job = new ProductionJob(objectDefinition, buildTime / _gameObject.ProductionModifier, _nextJobId++);
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
            var job = new ProductionJob(upgradeDefinition, _nextJobId++);
            _productionQueue.Add(job);

            if (upgradeDefinition.Type == UpgradeType.Player)
            {
                _gameObject.Owner.AddUpgrade(upgradeDefinition, UpgradeStatus.Queued);
            }
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

            if (upgradeDefinition.Type == UpgradeType.Player)
            {
                _gameObject.Owner.CancelUpgrade(upgradeDefinition);
            }
        }

        public bool CanEnqueue() => _moduleData.MaxQueueEntries == 0 || _productionQueue.Count < _moduleData.MaxQueueEntries;

        internal override void DrawInspector()
        {
            var (currentDoorState, doorStateEndFrame) = GetDoorStatus();
            ImGui.LabelText("DoorState", currentDoorState.ToString());
            ImGui.LabelText("DoorStateEndFrame", doorStateEndFrame.Value.ToString());
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistList(_productionQueue, static (StatePersister persister, ref ProductionJob item) =>
            {
                if (persister.Mode is StatePersistMode.Read)
                {
                    item = new ProductionJob();
                }

                persister.PersistObjectValue(item);
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
                _doorStatuses,
                static (StatePersister persister, ref DoorStatus item) =>
                {
                    persister.PersistObjectValue(ref item);
                });

            // this seems to be set for only a single frame upon construction completion
            // the same may also be true for _unknownFrame1_
            // in the case of queued production (e.g. red guard), the values set after the first and second red guard are different

            // upon creating the first item, we show CONSTRUCTION_COMPLETE
            // 01 00 00 00 00 01 01 00 00 00 15 43 4f 4e 53 54 52 55 43 54 49 4f 4e 5f 43 4f 4d 50 4c 45 54 45 01

            // after creating the second item, we show ACTIVELY_CONSTRUCTING CONSTRUCTION_COMPLETE, but only for one frame? seems like a bug
            // CONSTRUCTION_COMPLETE is only set for the one frame where we spawn something
            // this logic only persists object modelconditionstate for a _single frame_. It's unclear why.

            // some more examples

            // productionupdate ACTIVELY_CONSTRUCTING CONSTRUCTION_COMPLETE
            // 01 01 00 00 00 15 41 43 54 49 56 45 4c 59 5f 43 4f 4e 53 54 52 55 43 54 49 4e 47 01 01 00 00 00 15 43 4f 4e 53 54 52 55 43 54 49 4f 4e 5f 43 4f 4d 50 4c 45 54 45 01

            // productionupdate DOOR1_OPENING CONSTRUCTION_COMPLETE
            // 01 00 00 00 00 01 02 00 00 00 0e 44 4f 4f 52 5f 31 5f 4f 50 45 4e 49 4e 47 15 43 4f 4e 53 54 52 55 43 54 49 4f 4e 5f 43 4f 4d 50 4c 45 54 45 01

            // it's unclear how this should be parsed

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
            { "QuantityModifier", (parser, x) => x.QuantityModifiers[parser.ParseAssetReference()] = parser.ParseUnsignedInteger() },

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
        /// <remarks>
        /// The engine <i>does</i> support multiple <c>QuantityModifier</c>s.
        /// </remarks>
        public Dictionary<string, uint> QuantityModifiers { get; } = [];

        public BitArray<DisabledType> DisabledTypesToProcess { get; private set; } = new();

        [AddedIn(SageGame.Bfme)]
        public bool VeteranUnitsFromVeteranFactory { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SetBonusModelConditionOnSpeedBonus { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string? BonusForType { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string? SpeedBonusAudioLoop { get; private set; }

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

        public string? RequiredUpgrade { get; private set; }
        public float CostMultiplier { get; private set; }
        public float TimeMultiplier { get; private set; }
        public ObjectFilter ModifierFilter { get; private set; } = new();
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

    internal struct DoorStatus : IPersistableObject
    {
        public LogicFrame DoorOpeningUntil;
        public LogicFrame DoorWaitingOpenUntil;
        public LogicFrame DoorClosingUntil;

        public void Persist(StatePersister reader)
        {
            reader.PersistLogicFrame(ref DoorOpeningUntil);
            reader.PersistLogicFrame(ref DoorWaitingOpenUntil);
            reader.PersistLogicFrame(ref DoorClosingUntil);

            reader.SkipUnknownBytes(4);
        }
    }
}
