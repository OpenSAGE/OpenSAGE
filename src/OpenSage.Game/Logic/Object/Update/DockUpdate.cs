using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public abstract class DockUpdateModuleData : UpdateModuleData
    {
        internal static readonly IniParseTable<DockUpdateModuleData> FieldParseTable = new IniParseTable<DockUpdateModuleData>
        {
            { "NumberApproachPositions", (parser, x) => x.NumberApproachPositions = parser.ParseInteger() },
            { "AllowsPassthrough", (parser, x) => x.AllowsPassthrough = parser.ParseBoolean() },
        };

        /// <summary>
        /// Number of approach bones in the model. If this is -1, infinite harvesters can approach.
        /// </summary>
        public int NumberApproachPositions { get; private set; }

        /// <summary>
        /// Can harvesters drive through this warehouse? Should be set to false if all dock points are external.
        /// </summary>
        public bool AllowsPassthrough { get; private set; } = true;
    }
}
