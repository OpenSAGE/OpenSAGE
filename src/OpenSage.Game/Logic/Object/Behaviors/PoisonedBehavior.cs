using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class PoisonedBehavior : ObjectBehavior
    {
        internal static PoisonedBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PoisonedBehavior> FieldParseTable = new IniParseTable<PoisonedBehavior>
        {
            { "PoisonDamageInterval", (parser, x) => x.PoisonDamageInterval = parser.ParseInteger() },
            { "PoisonDuration", (parser, x) => x.PoisonDuration = parser.ParseInteger() }
        };

        /// <summary>
        /// Frequency (in milliseconds) to apply poison damage.
        /// </summary>
        public int PoisonDamageInterval { get; private set; }

        /// <summary>
        /// Amount of time to continue being damaged after last hit by poison damage.
        /// </summary>
        public int PoisonDuration { get; private set; }
    }
}
