using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Forces object to dynamically restore itself.
    /// Triggered when object is REALLYDAMAGED, or at 30% of MaxHealth.
    /// </summary>
    public sealed class SupplyWarehouseCripplingBehaviorModuleData : BehaviorModuleData
    {
        internal static SupplyWarehouseCripplingBehaviorModuleData Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<SupplyWarehouseCripplingBehaviorModuleData> FieldParseTable = new IniParseTable<SupplyWarehouseCripplingBehaviorModuleData>
        {
            { "SelfHealSupression", (parser, x) => x.SelfHealSuppression = parser.ParseInteger() },
            { "SelfHealDelay", (parser, x) => x.SelfHealDelay = parser.ParseInteger() },
            { "SelfHealAmount", (parser, x) => x.SelfHealAmount = parser.ParseInteger() }
        };

        /// <summary>
        /// Time since last damage until healing starts.
        /// </summary>
        public int SelfHealSuppression { get; private set; }

        /// <summary>
        /// How frequently to heal.
        /// </summary>
        public int SelfHealDelay { get; private set; }

        public int SelfHealAmount { get; private set; }
    }
}
