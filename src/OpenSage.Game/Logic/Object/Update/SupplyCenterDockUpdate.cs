using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SupplyCenterDockUpdate : ObjectBehavior
    {
        internal static SupplyCenterDockUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SupplyCenterDockUpdate> FieldParseTable = new IniParseTable<SupplyCenterDockUpdate>
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
