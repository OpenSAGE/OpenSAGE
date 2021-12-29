using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object.Production;

namespace OpenSage.Logic.Object
{
    public sealed class ParkingPlaceBehaviour : UpdateModule, IProductionExit
    {
        private readonly ParkingPlaceBehaviorModuleData _moduleData;
        private readonly GameObject _gameObject;
        private readonly GameContext _gameContext;
        private readonly GameObject[] _parkingSlots;
        private readonly Dictionary<string, bool> _blockedBones;

        internal ParkingPlaceBehaviour(ParkingPlaceBehaviorModuleData moduleData, GameObject gameObject, GameContext context)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
            _gameContext = context;
            _parkingSlots = new GameObject[_moduleData.NumRows * _moduleData.NumCols];
            _blockedBones = new Dictionary<string, bool>();
        }

        public bool CanProduceObject(ObjectDefinition definition, IReadOnlyList<ProductionJob> productionQueue)
        {
            if (ProducedAtHelipad(definition))
            {
                return true;
            }

            // e.g. a enqueued upgrade (mig armor) has no AIUpdate
            var numInQueue = productionQueue.Count(job => ((JetAIUpdateModuleData)job.ObjectDefinition?.AIUpdate?.Data)?.NeedsRunway ?? false);
            var slotsAvailable = _parkingSlots.Count(_ => _ == null);
            return slotsAvailable > numInQueue;
        }

        public bool ProducedAtHelipad(ObjectDefinition definition) => definition.KindOf.Get(ObjectKinds.ProducedAtHelipad);

        public int NextFreeSlot()
        {
            for (var index = 0; index < _parkingSlots.Length; index++)
            {
                if (_parkingSlots[index] == null)
                {
                    return index;
                }
            }

            return -1;
        }

        public int GetCorrespondingSlot(GameObject gameObject)
        {
            for (var index = 0; index < _parkingSlots.Length; index++)
            {
                if (_parkingSlots[index] == gameObject)
                {
                    return index;
                }
            }

            return -1;
        }

        public void ClearObjectFromSlot(GameObject gameObject)
        {
            for (var index = 0; index < _parkingSlots.Length; index++)
            {
                if (_parkingSlots[index] == gameObject)
                {
                    _parkingSlots[index] = null;
                    return;
                }
            }
        }

        public Vector3 GetUnitCreatePoint() => throw new InvalidOperationException("use GetUnitCreateTransform instead");

        public Transform GetUnitCreateTransform(bool producedAtHelipad)
        {
            if (producedAtHelipad)
            {
                return GetBoneTransform($"HELIPARK01");
            }

            var freeSlot = NextFreeSlot();
            var runway = SlotToRunway(freeSlot);
            var hangar = SlotToHangar(freeSlot);
            return GetBoneTransform($"RUNWAY{runway}PARK{hangar}HAN");
        }

        public Transform GetUnitCreateTransform(GameObject gameObject)
        {
            var slot = GetCorrespondingSlot(gameObject);
            var runway = SlotToRunway(slot);
            var hangar = SlotToHangar(slot);
            return GetBoneTransform($"RUNWAY{runway}PARK{hangar}HAN");
        }

        public void AddVehicle(GameObject vehicle)
        {
            var freeSlot = NextFreeSlot();
            _parkingSlots[freeSlot] = vehicle;

            vehicle.AIUpdate.SetLocomotor(LocomotorSetType.Taxiing);
            var jetAIUpdate = vehicle.AIUpdate as JetAIUpdate;
            jetAIUpdate.Base = _gameObject;
            jetAIUpdate.CurrentJetAIState = JetAIUpdate.JetAIState.Parked;
        }

        public void ParkVehicle(GameObject vehicle)
        {
            var jetAIUpdate = vehicle.AIUpdate as JetAIUpdate;
            jetAIUpdate.CurrentJetAIState = JetAIUpdate.JetAIState.JustCreated;
        }

        // this is handled via JetAIUpdate
        public Vector3? GetNaturalRallyPoint() => null;

        public Transform GetParkingTransform(GameObject gameObject)
        {
            if (_moduleData.ParkInHangars)
            {
                return GetUnitCreateTransform(gameObject);
            }

            var slot = GetCorrespondingSlot(gameObject);
            var runway = SlotToRunway(slot);
            var hangar = SlotToHangar(slot);

            return GetBoneTransform($"RUNWAY{runway}PARKING{hangar}");
        }

        public bool IsTaxiingPointBlocked(string boneName) => _blockedBones.ContainsKey(boneName) && _blockedBones[boneName];
        public void SetTaxiingPointBlocked(string boneName, bool value) => _blockedBones[boneName] = value;

        public Queue<string> GetPathToStart(GameObject vehicle)
        {
            var result = new Queue<string>();
            var slot = GetCorrespondingSlot(vehicle);
            var runway = SlotToRunway(slot);
            var hangar = SlotToHangar(slot);

            var parkingPoint = $"RUNWAY{runway}PARKING{hangar}";

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
            result.Enqueue($"RUNWAYSTART{runway}");
            return result;
        }

        public Queue<string> GetPathToHangar(GameObject vehicle) => new Queue<string>(GetPathToStart(vehicle).Reverse());

        public Vector3 GetRunwayEndPoint(GameObject vehicle)
        {
            var slot = GetCorrespondingSlot(vehicle);
            var runway = SlotToRunway(slot);
            return GetBoneTranslation($"RUNWAYEND{runway}");
        }

        public Transform GetBoneTransform(string name)
        {
            var (_, bone) = _gameObject.Drawable.FindBone(name);
            if (bone == null)
            {
                throw new InvalidOperationException("Could not find runway start point bone");
            }
            return bone.Transform;
        }

        public Vector3 GetBoneTranslation(string name) => GetBoneTransform(name).Translation;

        private int SlotToHangar(int slot) => slot / 2 + 1;
        private int SlotToRunway(int slot) => slot % 2 + 1;

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(3);

            base.Load(reader);

            var unknown1 = 4u;
            reader.ReadUInt32(ref unknown1);
            if (unknown1 != 4)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(17);

            var unknown2 = 2u;
            reader.ReadUInt32(ref unknown2);
            if (unknown2 != 2)
            {
                throw new InvalidStateException();
            }

            reader.SkipUnknownBytes(29);

            var unknown3 = 0x3FFFFFFFu;
            reader.ReadUInt32(ref unknown3);
            if (unknown3 != 0x3FFFFFFF)
            {
                throw new InvalidStateException();
            }
        }
    }

    /// <summary>
    /// Used by FS_AIRFIELD KindOfs. If <see cref="HasRunways"/> is set then the model requires 
    /// RunwayStartN, RunwayEndN, RunwayNPrepN, RunwayNParkingN and RunwayNParkNHan bones where N 
    /// corresponds to rows and columns. Module will only use the HeliPark01 bone for helicopters.
    /// </summary>
    public sealed class ParkingPlaceBehaviorModuleData : BehaviorModuleData
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

        public int HealAmountPerSecond { get; private set; }
        public int NumRows { get; private set; }
        public int NumCols { get; private set; }
        public bool HasRunways { get; private set; }
        public int ApproachHeight { get; private set; }
        public bool ParkInHangars { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ParkingPlaceBehaviour(this, gameObject, context);
        }
    }
}
