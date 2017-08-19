using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Required on an object that uses PublicTimer code for any SpecialPower and/or required for 
    /// units/structures with object upgrades.
    /// </summary>
    public sealed class ProductionUpdate : ObjectBehavior
    {
        internal static ProductionUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProductionUpdate> FieldParseTable = new IniParseTable<ProductionUpdate>
        {
            { "NumDoorAnimations", (parser, x) => x.NumDoorAnimations = parser.ParseInteger() },
            { "DoorOpeningTime", (parser, x) => x.DoorOpeningTime = parser.ParseInteger() },
            { "DoorWaitOpenTime", (parser, x) => x.DoorWaitOpenTime = parser.ParseInteger() },
            { "DoorCloseTime", (parser, x) => x.DoorCloseTime = parser.ParseInteger() },
            { "ConstructionCompleteDuration", (parser, x) => x.ConstructionCompleteDuration = parser.ParseInteger() },
            { "MaxQueueEntries", (parser, x) => x.MaxQueueEntries = parser.ParseInteger() },
            { "QuantityModifier", (parser, x) => x.QuantityModifier = ProductionUpdateQuantityModifier.Parse(parser) },

            { "DisabledTypesToProcess", (parser, x) => x.DisabledTypesToProcess = parser.ParseEnumBitArray<DisabledType>() }
        };

        /// <summary>
        /// Specifies how many doors to use when unit training is complete.
        /// </summary>
        public int NumDoorAnimations { get; private set; }

        public int DoorOpeningTime { get; private set; }
        public int DoorWaitOpenTime { get; private set; }
        public int DoorCloseTime { get; private set; }
        public int ConstructionCompleteDuration { get; private set; }
        public int MaxQueueEntries { get; private set; }

        /// <summary>
        /// Red Guards use this so that they can come out of the barracks in pairs.
        /// </summary>
        public ProductionUpdateQuantityModifier? QuantityModifier { get; private set; }

        public BitArray<DisabledType> DisabledTypesToProcess { get; private set; }
    }

    public struct ProductionUpdateQuantityModifier
    {
        internal static ProductionUpdateQuantityModifier Parse(IniParser parser)
        {
            return new ProductionUpdateQuantityModifier
            {
                ObjectName = parser.ParseAssetReference(),
                Quantity = parser.ParseInteger()
            };
        }

        public string ObjectName;
        public int Quantity;
    }

    public enum DisabledType
    {
        [IniEnum("DISABLED_HELD")]
        DisabledHeld,

        [IniEnum("DISABLED_UNDERPOWERED")]
        DisabledUnderpowered,
    }
}
