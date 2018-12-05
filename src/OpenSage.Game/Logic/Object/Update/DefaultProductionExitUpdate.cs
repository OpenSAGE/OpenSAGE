using System.Numerics;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
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
    }
}
