using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class QueueProductionExitUpdate : UpdateModule, IProductionExit
    {
        private readonly QueueProductionExitUpdateModuleData _moduleData;

        internal QueueProductionExitUpdate(QueueProductionExitUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
        }

        Vector3 IProductionExit.GetUnitCreatePoint() => _moduleData.UnitCreatePoint;

        Vector3? IProductionExit.GetNaturalRallyPoint() => _moduleData.NaturalRallyPoint;

        public int ExitDelay => _moduleData.ExitDelay > 0 ? _moduleData.ExitDelay : 300; 
    }

    public sealed class QueueProductionExitUpdateModuleData : UpdateModuleData
    {
        internal static QueueProductionExitUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<QueueProductionExitUpdateModuleData> FieldParseTable = new IniParseTable<QueueProductionExitUpdateModuleData>
        {
            { "UnitCreatePoint", (parser, x) => x.UnitCreatePoint = parser.ParseVector3() },
            { "NaturalRallyPoint", (parser, x) => x.NaturalRallyPoint = parser.ParseVector3() },
            { "ExitDelay", (parser, x) => x.ExitDelay = parser.ParseInteger() },
            { "InitialBurst", (parser, x) => x.InitialBurst = parser.ParseInteger() },
            { "PlacementViewAngle", (parser, x) => x.PlacementViewAngle = parser.ParseInteger() },
            { "NoExitPath", (parser, x) => x.NoExitPath = parser.ParseBoolean() },
            { "AllowAirborneCreation", (parser, x) => x.AllowAirborneCreation = parser.ParseBoolean() },
            { "UseReturnToFormation", (parser, x) => x.UseReturnToFormation = parser.ParseBoolean() }
        };

        public Vector3 UnitCreatePoint { get; private set; }

        /// <summary>
        /// <see cref="NaturalRallyPoint.X"/> must match <see cref="ObjectDefinition.GeometryMajorRadius"/>.
        /// </summary>
        public Vector3 NaturalRallyPoint { get; private set; }

        /// <summary>
        /// Used for Red Guards to make them come out one at a time.
        /// </summary>
        public int ExitDelay { get; private set; }

        public int InitialBurst { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int PlacementViewAngle { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool NoExitPath { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool AllowAirborneCreation { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool UseReturnToFormation { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new QueueProductionExitUpdate(this);
        }
    }
}
