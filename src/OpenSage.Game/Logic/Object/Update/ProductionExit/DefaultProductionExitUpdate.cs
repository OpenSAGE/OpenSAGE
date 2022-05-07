using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DefaultProductionExitUpdate : UpdateModule, IProductionExit
    {
        private readonly DefaultProductionExitUpdateModuleData _moduleData;

        private Vector3 _unknownPos;
        private bool _unknownBool;

        internal DefaultProductionExitUpdate(DefaultProductionExitUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
        }

        Vector3 IProductionExit.GetUnitCreatePoint() => _moduleData.UnitCreatePoint;

        Vector3? IProductionExit.GetNaturalRallyPoint() => _moduleData.NaturalRallyPoint;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistVector3(ref _unknownPos);
            reader.PersistBoolean(ref _unknownBool);
        }
    }

    public sealed class DefaultProductionExitUpdateModuleData : UpdateModuleData
    {
        internal static DefaultProductionExitUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DefaultProductionExitUpdateModuleData> FieldParseTable = new IniParseTable<DefaultProductionExitUpdateModuleData>
        {
            { "UnitCreatePoint", (parser, x) => x.UnitCreatePoint = parser.ParseVector3() },
            { "NaturalRallyPoint", (parser, x) => x.NaturalRallyPoint = parser.ParseVector3() },
            { "UseSpawnRallyPoint", (parser, x) => x.UseSpawnRallyPoint = parser.ParseBoolean() },
        };

        public Vector3 UnitCreatePoint { get; private set; }

        /// <summary>
        /// <see cref="NaturalRallyPoint.X"/> must match <see cref="ObjectDefinition.GeometryMajorRadius"/>.
        /// </summary>
        public Vector3 NaturalRallyPoint { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool UseSpawnRallyPoint { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DefaultProductionExitUpdate(this);
        }
    }
}
