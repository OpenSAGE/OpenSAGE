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

        internal ParkingPlaceBehaviour(ParkingPlaceBehaviorModuleData moduleData, GameObject gameObject, GameContext context)
        {
            _moduleData = moduleData;
            _gameObject = gameObject;
            _gameContext = context;
        }

        public Vector3 GetUnitCreatePoint()
        {
            int runway = _gameContext.Random.Next(1, _moduleData.NumCols);
            int hangar = _gameContext.Random.Next(1, _moduleData.NumRows);
            var (modelInstance, bone) = _gameObject.FindBone($"RUNWAY{runway}PARK{hangar}HAN");

            if (bone == null)
            {
                throw new InvalidOperationException("Could not find spawn point bone");
            }

            return bone.Transform.Translation;
        }

        public Vector3? GetNaturalRallyPoint()
        {
            int start = _gameContext.Random.Next(1, _moduleData.NumCols);
            var (modelInstance, bone) = _gameObject.FindBone($"RUNWAYSTART{start}");

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
