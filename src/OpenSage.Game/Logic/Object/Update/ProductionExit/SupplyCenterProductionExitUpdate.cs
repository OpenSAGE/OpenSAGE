using System.Numerics;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyCenterProductionExitUpdate : UpdateModule, IProductionExit
    {
        private readonly SupplyCenterProductionExitUpdateModuleData _moduleData;

        internal SupplyCenterProductionExitUpdate(SupplyCenterProductionExitUpdateModuleData moduleData)
        {
            _moduleData = moduleData;
        }

        Vector3 IProductionExit.GetUnitCreatePoint() => _moduleData.UnitCreatePoint;

        Vector3? IProductionExit.GetNaturalRallyPoint() => _moduleData.NaturalRallyPoint;
    }

    /// <summary>
    /// Requires the <see cref="ObjectKinds.SupplySource"/> KindOf defined in order to work properly.
    /// </summary>
    public sealed class SupplyCenterProductionExitUpdateModuleData : UpdateModuleData
    {
        internal static SupplyCenterProductionExitUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyCenterProductionExitUpdateModuleData> FieldParseTable = new IniParseTable<SupplyCenterProductionExitUpdateModuleData>
        {
            { "UnitCreatePoint", (parser, x) => x.UnitCreatePoint = parser.ParseVector3() },
            { "NaturalRallyPoint", (parser, x) => x.NaturalRallyPoint = parser.ParseVector3() },
            { "GrantTemporaryStealth", (parser, x) => x.GrantTemporaryStealth = parser.ParseInteger() },
        };

        public Vector3 UnitCreatePoint { get; private set; }

        /// <summary>
        /// <see cref="NaturalRallyPoint.X"/> must match <see cref="ObjectDefinition.GeometryMajorRadius"/>.
        /// </summary>
        public Vector3 NaturalRallyPoint { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int GrantTemporaryStealth { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject)
        {
            return new SupplyCenterProductionExitUpdate(this);
        }
    }
}
