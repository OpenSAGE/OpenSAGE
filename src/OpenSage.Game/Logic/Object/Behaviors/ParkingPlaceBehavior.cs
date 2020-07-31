using System;
using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    //TODO: HasRunways. Figure out cols & rows
    public sealed class ParkingPlaceBehaviour : BehaviorModule, IProductionExit
    {
        private readonly ParkingPlaceBehaviorModuleData _moduleData;
        private readonly GameObject _gameObject;
        private readonly GameContext _gameContext;
        private GameObject[] _parkingSlots;

        private int _currentSlot;

        internal ParkingPlaceBehaviour(ParkingPlaceBehaviorModuleData moduleData, GameObject gameObject, GameContext context)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
            _gameContext = context;
            _parkingSlots = new GameObject[_moduleData.NumRows * _moduleData.NumCols];
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

        public void ParkVehicle(GameObject gameObject)
        {
            var freeSlot = NextFreeSlot();
            //TODO: when there are no slots don't produce the plane anymore
            _parkingSlots[freeSlot] = gameObject;
        }

        public Vector3 GetUnitCreatePoint()
        {
            var freeSlot = NextFreeSlot();
            var runway = freeSlot % 2 + 1;
            var hangar = freeSlot / 2 + 1;
            var (modelInstance, bone) = _gameObject.FindBone($"RUNWAY{runway}PARK{hangar}HAN");

            if (bone == null)
            {
                throw new InvalidOperationException("Could not find spawn point bone");
            }

            return bone.Transform.Translation;
        }

        public Vector3? GetNaturalRallyPoint()
        {
            throw new InvalidOperationException("no game object provided");
        }


        public Vector3? GetNaturalRallyPoint(GameObject gameObject)
        {

            var slot = GetCorrespondingSlot(gameObject);
            var runway = slot % 2 + 1;
            var hangar = slot / 2 + 1;
            var (modelInstance, bone) = _gameObject.FindBone($"RUNWAY{runway}PARKING{hangar}");

            if (bone == null)
            {
                throw new InvalidOperationException("Could not find start point bone");
            }

            return bone.Transform.Translation;
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
