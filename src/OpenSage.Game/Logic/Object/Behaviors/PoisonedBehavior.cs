using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class PoisonedBehavior : UpdateModule
    {
        // TODO

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 2)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            var unknown = reader.ReadBytes(16);
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
