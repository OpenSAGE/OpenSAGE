using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class PoisonedBehavior : UpdateModule
    {
        private uint _unknown;

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(2);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.SkipUnknownBytes(12);

            reader.PersistUInt32("Unknown", ref _unknown);
        }
    }

    public sealed class PoisonedBehaviorModuleData : UpdateModuleData
    {
        internal static PoisonedBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<PoisonedBehaviorModuleData> FieldParseTable = new IniParseTable<PoisonedBehaviorModuleData>
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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new PoisonedBehavior();
        }
    }
}
