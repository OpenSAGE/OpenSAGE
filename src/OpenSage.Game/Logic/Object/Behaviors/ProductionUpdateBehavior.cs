using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Required on an object that uses PublicTimer code for any SpecialPower and/or required for 
    /// units/structures with object upgrades.
    /// </summary>
    public sealed class ProductionUpdateBehavior : ObjectBehavior
    {
        internal static ProductionUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProductionUpdateBehavior> FieldParseTable = new IniParseTable<ProductionUpdateBehavior>
        {
            { "NumDoorAnimations", (parser, x) => x.NumDoorAnimations = parser.ParseInteger() },
            { "DoorOpeningTime", (parser, x) => x.DoorOpeningTime = parser.ParseInteger() },
            { "DoorWaitOpenTime", (parser, x) => x.DoorWaitOpenTime = parser.ParseInteger() },
            { "DoorCloseTime", (parser, x) => x.DoorCloseTime = parser.ParseInteger() },
            { "ConstructionCompleteDuration", (parser, x) => x.ConstructionCompleteDuration = parser.ParseInteger() }
        };

        /// <summary>
        /// Specifies how many doors to use when unit training is complete.
        /// </summary>
        public int NumDoorAnimations { get; private set; }

        public int DoorOpeningTime { get; private set; }
        public int DoorWaitOpenTime { get; private set; }
        public int DoorCloseTime { get; private set; }
        public int ConstructionCompleteDuration { get; private set; }
    }
}
