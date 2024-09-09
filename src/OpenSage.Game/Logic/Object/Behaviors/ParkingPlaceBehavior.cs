using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using FixedMath.NET;
using ImGuiNET;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class ParkingPlaceBehaviour : UpdateModule, IHasRallyPoint, IProductionExit
    {
        public RallyPointManager RallyPointManager { get; } = new();

        internal ReadOnlySpan<ParkingSlot> ParkingSlots => _parkingSlots;
        internal ReadOnlySpan<RunwayAssignment> RunwayAssignments => _runwayAssignments;
        internal IReadOnlyList<ParkingPlaceHealingData> HealingData => _healingData;
        internal LogicFrame NextHealFrame => _nextHealFrame;

        private readonly GameObject _gameObject;
        private readonly GameContext _gameContext;
        private readonly ParkingPlaceBehaviorModuleData _moduleData;

        private readonly ParkingSlot[] _parkingSlots;
        private readonly RunwayAssignment[] _runwayAssignments;
        private readonly List<ParkingPlaceHealingData> _healingData = [];
        private static LogicFrame NoHealingFrame => new(0x3FFFFFFFu);
        private LogicFrame _nextHealFrame = NoHealingFrame;
        private readonly Fix64 _healAmountPerHealTick;
        private const int HealsPerSecond = 5; // not sure if this is configured anywhere
        private static readonly LogicFrameSpan HealUpdateRate = new((uint)(Game.LogicFramesPerSecond / HealsPerSecond));

        internal ParkingPlaceBehaviour(GameObject gameObject, GameContext context, ParkingPlaceBehaviorModuleData moduleData)
        {
            _gameObject = gameObject;
            _gameContext = context;
            _moduleData = moduleData;

            _parkingSlots = new ParkingSlot[_moduleData.NumRows * _moduleData.NumCols];
            _runwayAssignments = new RunwayAssignment[_moduleData.HasRunways ? _moduleData.NumCols : 0];
            _healAmountPerHealTick = new Fix64(_moduleData.HealAmountPerSecond) / new Fix64(HealsPerSecond);
        }

        public bool HasFreeSlots()
        {
            return _parkingSlots.Any(s => !s.Occupied);
        }

        public void EnqueueObject()
        {
            for (var i = 0; i < _parkingSlots.Length; i++)
            {
                if (!_parkingSlots[i].Occupied)
                {
                    _parkingSlots[i] = ParkingSlot.UnderConstruction;
                    return;
                }
            }

            throw new InvalidStateException("No parking slots available");
        }

        public void CancelQueuedObject()
        {
            for (var i = 0; i < _parkingSlots.Length; i++)
            {
                var slot = _parkingSlots[i];
                if (!slot.Constructing)
                {
                    if (i > 0)
                    {
                        _parkingSlots[i - 1] = ParkingSlot.Empty;
                    }

                    return;
                }
            }

            _parkingSlots[^1] = ParkingSlot.Empty;
        }

        private protected override void RunUpdate(BehaviorUpdateContext context)
        {
            if (_healingData.Count == 0)
            {
                _nextHealFrame = NoHealingFrame;
            }
            else
            {
                if (_nextHealFrame <= context.LogicFrame)
                {
                    _nextHealFrame += HealUpdateRate;
                    foreach (var objectToHeal in _healingData)
                    {
                        var gameObject = _gameContext.GameLogic.GetObjectById(objectToHeal.ObjectId);
                        gameObject?.Heal(_healAmountPerHealTick, _gameObject);
                    }
                }
            }

            base.RunUpdate(context);
        }

        public int NextSpawnableSlot()
        {
            for (var index = 0; index < _parkingSlots.Length; index++)
            {
                if (_parkingSlots[index].ObjectId == 0)
                {
                    return index;
                }
            }

            return -1;
        }

        private int GetCorrespondingSlot(uint gameObjectId)
        {
            for (var index = 0; index < _parkingSlots.Length; index++)
            {
                if (_parkingSlots[index].ObjectId == gameObjectId)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Clears the object from its slot, and returns the slot index so the door can be closed
        /// </summary>
        public int ClearObjectFromSlot(uint objectId)
        {
            for (var index = 0; index < _parkingSlots.Length; index++)
            {
                if (_parkingSlots[index].ObjectId == objectId)
                {
                    _parkingSlots[index] = ParkingSlot.Empty;

                    ClearRunway(objectId); // in case this object had a runway reserved
                    return index;
                }
            }

            return -1;
        }

        public Vector3 GetUnitCreatePoint() => throw new InvalidOperationException("use GetUnitCreateTransform instead");

        public Transform GetUnitCreateTransform(bool producedAtHelipad, uint objectId)
        {
            if (producedAtHelipad)
            {
                return GetBoneTransform($"HELIPARK01");
            }

            var slot = GetCorrespondingSlot(objectId);
            var runway = SlotToRunway(slot);
            var hangar = SlotToHangar(slot);
            return GetBoneTransform($"RUNWAY{runway}PARK{hangar}HAN");
        }

        public Transform GetUnitCreateTransform(uint gameObjectId)
        {
            var slot = GetCorrespondingSlot(gameObjectId);
            var runway = SlotToRunway(slot);
            var hangar = SlotToHangar(slot);
            return GetBoneTransform($"RUNWAY{runway}PARK{hangar}HAN");
        }

        // todo: what about helicopers?
        // this is handled via JetAIUpdate
        public Vector3? GetNaturalRallyPoint() => null;

        public Transform GetParkingTransform(uint gameObjectId)
        {
            if (_moduleData.ParkInHangars)
            {
                return GetUnitCreateTransform(gameObjectId);
            }

            var slot = GetCorrespondingSlot(gameObjectId);
            var runway = SlotToRunway(slot);
            var hangar = SlotToHangar(slot);

            return GetBoneTransform($"RUNWAY{runway}PARKING{hangar}");
        }

        // todo: track multiple aircraft assigned to a single runway
        public bool IsTaxiingPointBlocked(string boneName) =>
            _runwayAssignments[IndexForRunwayBone(boneName)].Aircraft1Id != 0;

        public void ReserveRunway(uint objectId)
        {
            // check if there are any free runways
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                if (!_runwayAssignments[i].AircraftOccupyingRunway)
                {
                    _runwayAssignments[i] = new RunwayAssignment(objectId);
                    return;
                }
            }

            // if all runways are occupied, check if we can double-stack
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                if (_runwayAssignments[i].Aircraft2Id == 0)
                {
                    _runwayAssignments[i] = new RunwayAssignment(_runwayAssignments[i].Aircraft1Id, objectId);
                    return;
                }
            }
        }

        public void ClearRunway(uint gameObjectId)
        {
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                var assignment = _runwayAssignments[i];
                if (assignment.Aircraft1Id == gameObjectId)
                {
                    // if there were previously two aircraft assigned to the runway, then it is probably considered active?
                    // from testing, it seems like runwayActive is never set to false independent of clearing the runway assignment
                    _runwayAssignments[i] =
                        new RunwayAssignment(assignment.Aircraft2Id, runwayActive: assignment.Aircraft2Id != 0);
                    return;
                }

                // if an object is destroyed while trying to taxi, this could happen
                if (assignment.Aircraft2Id == gameObjectId)
                {
                    _runwayAssignments[i] = new RunwayAssignment(assignment.Aircraft1Id);
                    return;
                }
            }
        }

        // AddState(1001, new SpawnState());
        /// <summary>
        /// Reports that a new aircraft has spawned in a parking place.
        /// </summary>
        /// <param name="objectId">object to spawn</param>
        /// <returns>index of parking slot claimed</returns>
        /// <exception cref="InvalidStateException">thrown when there are no parking places available</exception>
        public int ReportSpawn(uint objectId)
        {
            for (var i = 0; i < _parkingSlots.Length; i++)
            {
                if (_parkingSlots[i].Constructing)
                {
                    _parkingSlots[i] = new ParkingSlot(objectId);
                    return i;
                }
            }

            throw new InvalidStateException("no parking places available to spawn unit");
        }

        /// <summary>
        /// Reports that an aircraft is fully parked and ready for repairs.
        /// </summary>
        /// <param name="objectId">The parked aircraft</param>
        /// <param name="currentFrame">The current frame</param>
        public void ReportParkedIdle(uint objectId, LogicFrame currentFrame)
        {
            _healingData.Add(new ParkingPlaceHealingData(objectId, currentFrame));
            if (_nextHealFrame.Value == NoHealingFrame.Value)
            {
                _nextHealFrame = currentFrame;
            }
        }

        /// <summary>
        /// Reports that an aircraft is ready to taxi to the runway, stops healing, and reserves a runway, even if one is not yet available.
        /// </summary>
        /// <param name="objectId">Aircraft ready to taxi</param>
        /// <param name="runway">The runway assigned to the aircraft</param>
        /// <returns>Whether the aircraft was successfully assigned a runway</returns>
        /// <exception cref="InvalidStateException">thrown when there are no runways available (all runways are double-stacked)</exception>
        public bool ReportReadyToTaxi(uint objectId, out int runway)
        {
            var healingDataSlot = _healingData.FindIndex(s => s.ObjectId == objectId);

            if (healingDataSlot >= 0)
            {
                _healingData.RemoveAt(healingDataSlot);
            }

            if (_healingData.Count == 0)
            {
                _nextHealFrame = NoHealingFrame;
            }

            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                var assignment = _runwayAssignments[i];
                if (assignment.Aircraft1Id == objectId || assignment.Aircraft2Id == objectId)
                {
                    // we already have a runway assigned
                    runway = i;
                    return assignment.Aircraft1Id == objectId;
                }

                if (assignment.Aircraft1Id == 0)
                {
                    // this runway is free
                    _runwayAssignments[i] = new RunwayAssignment(objectId);
                    runway = i;
                    return true;
                }
            }

            // all runways are occupied, so we'll take the next one available
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                var assignment = _runwayAssignments[i];

                if (assignment.Aircraft2Id == 0)
                {
                    // todo: we should actually kick Aircraft1 off the runway ownership (and just set the bool to true) if they aren't actively taxiing to or landing on the runway
                    _runwayAssignments[i] = new RunwayAssignment(assignment.Aircraft1Id, objectId);
                    runway = i;
                    return false;
                }
            }

            throw new InvalidStateException("No runways are available");
        }

        // todo: if there are any aircraft waiting for the runway, they need a way of knowing they can have it now
        public void ReportEngineRunUp(uint objectId)
        {
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                var assignment = _runwayAssignments[i];
                if (assignment.Aircraft1Id == objectId && assignment.Aircraft2Id != 0)
                {
                    // promote aircraft2 to the owner of this runway
                    _runwayAssignments[i] = new RunwayAssignment(assignment.Aircraft2Id, runwayActive: true);
                }
            }
        }

        /// <summary>
        /// Reports that an aircraft has fully departed the airfield frees up their runway.
        /// </summary>
        /// <param name="objectId">The departing aircraft</param>
        public void ReportDeparted(uint objectId)
        {
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                var assignment = _runwayAssignments[i];
                if (assignment.Aircraft1Id == objectId)
                {
                    _runwayAssignments[i] = new RunwayAssignment(assignment.Aircraft2Id, runwayActive: assignment.Aircraft2Id != 0);
                }
            }
        }

        // todo: we should be able to order an aircraft to land at an airfield which is technically full as long as the aircraft aren't on the ground - how is that tracked?
        /// <summary>
        /// Allows an aircraft to report that they are inbound.
        /// </summary>
        /// <param name="objectId">Inbound aircraft</param>
        /// <returns>Hangar index assigned</returns>
        /// <exception cref="InvalidStateException">thrown when the airfield doesn't have any parking slots available</exception>
        public int ReportInbound(uint objectId)
        {
            for (var i = 0; i < _parkingSlots.Length; i++)
            {
                if (_parkingSlots[i].ObjectId == objectId)
                {
                    return i;
                }
            }

            // if this aircraft isn't already based here, try to find them a slot
            for (var i = 0; i < _parkingSlots.Length; i++)
            {
                if (!_parkingSlots[i].Occupied)
                {
                    _parkingSlots[i] = new ParkingSlot(objectId);
                    return i;
                }
            }

            throw new InvalidStateException("no parking slots available for landing");
        }

        /// <summary>
        /// Reports that an aircraft is coming in to land.
        /// </summary>
        /// <param name="objectId">Landing aircraft</param>
        /// <returns>Runway assignment, or -1 if no runways are available</returns>
        public int ReportLanding(uint objectId)
        {
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                if (_runwayAssignments[i].Aircraft1Id == 0)
                {
                    _runwayAssignments[i] = new RunwayAssignment(objectId);
                    return i;
                }
            }

            // we don't double-stack landing clearances
            return -1;
        }

        /// <summary>
        /// Reports that an aircraft has landed and is taxiing to parking.
        /// </summary>
        /// <param name="objectId">Landed aircraft</param>
        /// <returns>Hangar assignment</returns>
        /// <exception cref="InvalidStateException">thrown when an aircraft doesn't have a hangar reserved</exception>
        public (int Runway, int Hangar) ReportLanded(uint objectId)
        {
            var runway = -1;
            for (var i = 0; i < _runwayAssignments.Length; i++)
            {
                // clear the assigned runway
                var assignment = _runwayAssignments[i];
                if (assignment.Aircraft1Id == objectId)
                {
                    _runwayAssignments[i] = new RunwayAssignment(assignment.Aircraft2Id, runwayActive: assignment.Aircraft2Id != 0);
                    runway = i;
                    break;
                }
            }

            if (runway == -1)
            {
                throw new InvalidStateException("no runway found for aircraft");
            }

            for (var i = 0; i < _parkingSlots.Length; i++)
            {
                if (_parkingSlots[i].ObjectId == objectId)
                {
                    return (runway, i);
                }
            }

            throw new InvalidStateException("no hangar reserved for aircraft");
        }

        private static int IndexForRunwayBone(string boneName) => int.Parse(boneName[^1].ToString()) - 1;

        public Queue<string> GetPathToRunway(uint objectId, int runway)
        {
            var slot = GetCorrespondingSlot(objectId);
            return GetPathToStart(SlotToHangar(slot), runway + 1);
        }

        public Queue<string> GetPathToHangar(int runway, int slot)
        {
            return new Queue<string>(GetPathToStart(SlotToHangar(slot), runway + 1).Reverse());
        }

        private Queue<string> GetPathToStart(int hangar, int runwayBoneIndex)
        {
            var result = new Queue<string>();

            var parkingPoint = $"RUNWAY{runwayBoneIndex}PARKING{hangar}";

            if (_moduleData.ParkInHangars)
            {
                result.Enqueue(parkingPoint);
            }

            // this is a very hacky solution, but a generic one does not seem to work for both airfields
            // and even does not work for all aircrafts of the US airfield
            // also with this approach the aircrafts do not collide like in the vanilla version
            switch (parkingPoint)
            {
                case "RUNWAY1PARKING1":
                    if (_moduleData.ParkInHangars)
                    {
                        result.Enqueue($"RUNWAY1PARKING2");
                        result.Enqueue($"RUNWAY1PREP2");
                    }
                    else
                    {
                        result.Enqueue($"RUNWAY2PREP2");
                    }
                    result.Enqueue($"RUNWAY1PREP1");
                    break;
                case "RUNWAY1PARKING2":
                    if (_moduleData.ParkInHangars)
                    {
                        result.Enqueue($"RUNWAY1PARKING2");
                    }

                    result.Enqueue($"RUNWAY1PREP2");
                    result.Enqueue($"RUNWAY1PREP1");
                    break;
                case "RUNWAY2PARKING1":
                    if (_moduleData.ParkInHangars)
                    {
                        result.Enqueue($"RUNWAY2PARKING2");
                        result.Enqueue($"RUNWAY2PREP2");
                    }
                    result.Enqueue($"RUNWAY2PREP1");
                    break;
                case "RUNWAY2PARKING2":
                    if (_moduleData.ParkInHangars)
                    {
                        result.Enqueue($"RUNWAY2PARKING2");
                    }

                    result.Enqueue($"RUNWAY2PREP2");
                    result.Enqueue($"RUNWAY2PREP1");
                    break;
            }
            result.Enqueue($"RUNWAYSTART{runwayBoneIndex}");
            return result;
        }

        public Vector3 GetRunwayEndPoint(uint vehicleId)
        {
            var slot = GetCorrespondingSlot(vehicleId);
            var runway = SlotToRunway(slot);
            return GetBoneTranslation($"RUNWAYEND{runway}");
        }

        private Transform GetBoneTransform(string name)
        {
            var (_, bone) = _gameObject.Drawable.FindBone(name);
            if (bone == null)
            {
                throw new InvalidOperationException("Could not find runway start point bone");
            }
            return bone.Transform;
        }

        public Vector3 GetBoneTranslation(string name) => GetBoneTransform(name).Translation;

        private int SlotToHangar(int slot) => slot / _moduleData.NumRows + 1;
        private int SlotToRunway(int slot) => slot % _moduleData.NumCols + 1;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(3);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistArrayWithByteLength(_parkingSlots, (StatePersister persister, ref ParkingSlot item) => persister.PersistObjectValue(ref item));

            reader.PersistArrayWithByteLength(_runwayAssignments, (StatePersister persister, ref RunwayAssignment item) => persister.PersistObjectValue(ref item));

            reader.PersistListWithByteCount(_healingData, (StatePersister persister, ref ParkingPlaceHealingData item) =>
            {
                persister.PersistObjectValue(ref item);
            });

            reader.PersistObject(RallyPointManager);

            reader.PersistLogicFrame(ref _nextHealFrame);
        }

        internal override void DrawInspector()
        {
            base.DrawInspector();
            ImGui.LabelText("Next heal frame", _nextHealFrame.ToString());
        }

        internal record struct ParkingPlaceHealingData : IPersistableObject
        {
            /// <summary>
            /// The object id of the object being healed.
            /// </summary>
            public uint ObjectId => _objectId;

            /// <summary>
            /// The frame at which the object was fully parked and facing the correct direction, ready to be healed.
            /// </summary>
            public LogicFrame ParkedAtFrame => _parkedAtFrame;

            private uint _objectId;
            private LogicFrame _parkedAtFrame;

            public ParkingPlaceHealingData(uint objectId, LogicFrame parkedAtFrame)
            {
                _objectId = objectId;
                _parkedAtFrame = parkedAtFrame;
            }

            public void Persist(StatePersister persister)
            {
                persister.PersistObjectID(ref _objectId);
                persister.PersistLogicFrame(ref _parkedAtFrame);
            }
        }

        internal record struct ParkingSlot : IPersistableObject
        {
            /// <summary>
            /// The object ID of the object which currently owns the parking space.
            /// </summary>
            public uint ObjectId => _objectId;

            /// <summary>
            /// Whether the parking space is currently reserved for construction.
            /// </summary>
            public bool Constructing => _constructing;

            public bool Occupied => Constructing || ObjectId != 0;

            private uint _objectId;
            private bool _constructing;

            public static ParkingSlot Empty => new();
            public static ParkingSlot UnderConstruction => new(true);

            public ParkingSlot(uint objectId)
            {
                _objectId = objectId;
            }

            private ParkingSlot(bool constructing)
            {
                _constructing = constructing;
            }

            public void Persist(StatePersister persister)
            {
                persister.PersistObjectID(ref _objectId);
                persister.PersistBoolean(ref _constructing);
            }
        }

        internal record struct RunwayAssignment : IPersistableObject
        {
            /// <summary>
            /// The object ID of the object which is currently assigned the runway.
            /// </summary>
            public uint Aircraft1Id => _aircraft1Id;

            /// <summary>
            /// The object ID of the object which will take the runway after aircraft 1 clears the runway.
            /// </summary>
            public uint Aircraft2Id => _aircraft2Id;

            /// <summary>
            /// Whether an aircraft which is <i>not</i> <see cref="Aircraft1Id"/> is currently occupying the runway.
            /// </summary>
            /// <remarks>
            /// It's unclear what this field is used for.
            /// </remarks>
            public bool AircraftOccupyingRunway => _aircraftOccupyingRunway;

            private uint _aircraft1Id;
            private uint _aircraft2Id;
            private bool _aircraftOccupyingRunway;

            public static RunwayAssignment Empty => new();

            public RunwayAssignment(uint aircraft1Id, uint aircraft2Id = 0, bool runwayActive = false)
            {
                _aircraft1Id = aircraft1Id;
                _aircraft2Id = aircraft2Id;
                _aircraftOccupyingRunway = runwayActive;
            }

            public void Persist(StatePersister persister)
            {
                persister.PersistObjectID(ref _aircraft1Id);
                persister.PersistObjectID(ref _aircraft2Id);
                persister.PersistBoolean(ref _aircraftOccupyingRunway);
            }
        }
    }

    /// <summary>
    /// Used by FS_AIRFIELD KindOfs. If <see cref="HasRunways"/> is set then the model requires
    /// RunwayStartN, RunwayEndN, RunwayNPrepN, RunwayNParkingN and RunwayNParkNHan bones where N
    /// corresponds to rows and columns. Module will only use the HeliPark01 bone for helicopters.
    /// </summary>
    public sealed class ParkingPlaceBehaviorModuleData : UpdateModuleData
    {
        internal static ParkingPlaceBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ParkingPlaceBehaviorModuleData> FieldParseTable = new IniParseTable<ParkingPlaceBehaviorModuleData>
        {
            { "HealAmountPerSecond", (parser, x) => x.HealAmountPerSecond = parser.ParseInteger() },
            { "NumRows", (parser, x) => x.NumRows = parser.ParseInteger() },
            { "NumCols", (parser, x) => x.NumCols = parser.ParseInteger() },
            { "HasRunways", (parser, x) => x.HasRunways = parser.ParseBoolean() },
            { "ApproachHeight", (parser, x) => x.ApproachHeight = parser.ParseInteger() },
            { "ParkInHangars", (parser, x) => x.ParkInHangars = parser.ParseBoolean() },
        };

        /// <summary>
        /// Amount to heal parked aircraft, per second.
        /// </summary>
        public int HealAmountPerSecond { get; private set; }

        /// <summary>
        /// Number of rows aircraft can spawn in.
        /// </summary>
        public int NumRows { get; internal set; }

        /// <summary>
        /// Numbers of columns aircraft can spawn in (equal to number of runways).
        /// </summary>
        public int NumCols { get; internal set; }

        /// <summary>
        /// Whether the airfield has runways.
        /// </summary>
        public bool HasRunways { get; internal set; }

        public int ApproachHeight { get; private set; }
        public bool ParkInHangars { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ParkingPlaceBehaviour(gameObject, context, this);
        }
    }
}
