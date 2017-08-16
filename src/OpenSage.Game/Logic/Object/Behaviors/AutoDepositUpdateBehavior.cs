using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class AutoDepositUpdateBehavior : ObjectBehavior
    {
        internal static AutoDepositUpdateBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoDepositUpdateBehavior> FieldParseTable = new IniParseTable<AutoDepositUpdateBehavior>
        {
            { "DepositTiming", (parser, x) => x.DepositTiming = parser.ParseInteger() },
            { "DepositAmount", (parser, x) => x.DepositAmount = parser.ParseInteger() },
            { "InitialCaptureBonus", (parser, x) => x.InitialCaptureBonus = parser.ParseInteger() }
        };

        /// <summary>
        /// How often, in milliseconds, to give money to the owning player.
        /// </summary>
        public int DepositTiming { get; private set; }

        /// <summary>
        /// Amount of cash to deposit after every <see cref="DepositTiming"/>.
        /// </summary>
        public int DepositAmount { get; private set; }

        /// <summary>
        /// One-time capture bonus.
        /// </summary>
        public int InitialCaptureBonus { get; private set; }
    }
}
