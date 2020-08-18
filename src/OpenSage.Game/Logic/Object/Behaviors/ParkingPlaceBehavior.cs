using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Graphics;

namespace OpenSage.Logic.Object
{
    public sealed class ParkingPlaceBehaviour : BehaviorModule, IProductionExit
    {
        private readonly ParkingPlaceBehaviorModuleData _moduleData;
        private readonly GameObject _gameObject;
        private readonly GameContext _gameContext;
        private GameObject[] _parkingSlots;

        internal ParkingPlaceBehaviour(ParkingPlaceBehaviorModuleData moduleData, GameObject gameObject, GameContext context)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
            _gameContext = context;
            _parkingSlots = new GameObject[_moduleData.NumRows * _moduleData.NumCols];
        }

        public bool ProducedAtHelipad(ObjectDefinition definition)
        {
            return definition.KindOf.Get(ObjectKinds.ProducedAtHelipad);
        }

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

        private int GetCorrespondingSlot(GameObject gameObject)
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

        //Runway1Prking1
        //Runway1Parking2
        //Runway2Parking1
        //Runway2Parking2
        //Runway1Park1Han
        //Runway1Park2Han
        //Runway2Park1Han
        //Runway2Park2Han

        //Runway1Prep1
        //Runway1Prep2
        //RunwayStart1
        //RunwayEnd1
        //HeliPark01

        public Vector3 GetUnitCreatePoint()
        {
            throw new InvalidOperationException("not supported here");
        }

        public Transform GetUnitCreateTransform(bool producedAtHelipad)
        {
            ModelBone bone;

            if (producedAtHelipad)
            {
                (_, bone) = _gameObject.FindBone($"HELIPARK01");
            }
            else
            {
                var freeSlot = NextFreeSlot();
                var runway = freeSlot % 2 + 1;
                var hangar = freeSlot / 2 + 1;
                (_, bone) = _gameObject.FindBone($"RUNWAY{runway}PARK{hangar}HAN");
            }

            if (bone == null)
            {
                throw new InvalidOperationException("Could not find spawn point bone");
            }

            return bone.Transform;
        }

        public void ParkVehicle(GameObject vehicle)
        {
            var freeSlot = NextFreeSlot();
            _parkingSlots[freeSlot] = vehicle;

            vehicle.AIUpdate.SetLocomotor(LocomotorSetType.Taxiing);
            var jetAIUpdate = vehicle.AIUpdate as JetAIUpdate;
            jetAIUpdate.Base = _gameObject;
            jetAIUpdate.CurrentJetAIState = JetAIUpdate.JetAIState.PARKED;
        }

        public Vector3? GetNaturalRallyPoint() => null;

        public Vector3 GetNaturalRallyPoint(GameObject gameObject)
        {
            var slot = GetCorrespondingSlot(gameObject);
            var runway = slot % 2 + 1;
            var hangar = slot / 2 + 1;
            var (_, bone) = _gameObject.FindBone($"RUNWAY{runway}PARKING{hangar}");

            if (bone == null)
            {
                throw new InvalidOperationException("Could not find start point bone");
            }

            return bone.Transform.Translation;
        }

        // TODO: multiple prep points
        public Vector3 GetPrepPoint(GameObject vehicle)
        {
            var slot = GetCorrespondingSlot(vehicle);
            var runway = slot % 2 + 1;
            var hangar = slot / 2 + 1;

            var (_, bone) = _gameObject.FindBone($"RUNWAY{runway}PREP{hangar}");
            if (bone == null)
            {
                throw new InvalidOperationException("Could not find prep point bone");
            }
            return bone.Transform.Translation;
        }

        public Vector3 GetRunwayStartPoint(GameObject vehicle)
        {
            var slot = GetCorrespondingSlot(vehicle);
            var runway = slot % 2 + 1;

            var (_, bone) = _gameObject.FindBone($"RUNWAYSTART{runway}");
            if (bone == null)
            {
                throw new InvalidOperationException("Could not find runway start point bone");
            }
            return bone.Transform.Translation;
        }

        public Vector3 GetRunwayEndPoint(GameObject vehicle)
        {
            var slot = GetCorrespondingSlot(vehicle);
            var runway = slot % 2 + 1;

            var (_, bone) = _gameObject.FindBone($"RUNWAYEND{runway}");
            if (bone == null)
            {
                throw new InvalidOperationException("Could not find runway end point bone");
            }
            return bone.Transform.Translation;
        }

        //public List<Vector3> GetUnparkingPath(GameObject gameObject)
        //{
        //    var result = new List<Vector3>();
        //    var slot = GetCorrespondingSlot(gameObject);
        //    var runway = slot % 2 + 1;
        //    var hangar = slot / 2 + 1;

        //    var (_, bone) = _gameObject.FindBone($"RUNWAY{runway}PREP{hangar}");
        //    if (bone == null)
        //    {
        //        throw new InvalidOperationException("Could not find start point bone");
        //    }
        //    result.Add(bone.Transform.Translation);

        //    (_, bone) = _gameObject.FindBone($"RUNWAYSTART{runway}");
        //    if (bone == null)
        //    {
        //        throw new InvalidOperationException("Could not find start point bone");
        //    }
        //    result.Add(bone.Transform.Translation);

        //    (_, bone) = _gameObject.FindBone($"RUNWAYEND{runway}");
        //    if (bone == null)
        //    {
        //        throw new InvalidOperationException("Could not find start point bone");
        //    }
        //    result.Add(bone.Transform.Translation);

        //    return result;
        //}

        public void Unpark(GameObject gameObject)
        {
            //
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
