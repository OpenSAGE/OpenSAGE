using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class DialogEvent
    {
        internal static DialogEvent Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<DialogEvent> FieldParseTable = new IniParseTable<DialogEvent>
        {
            { "Filename", (parser, x) => x.Filename = parser.ParseAsciiString() },
            { "Volume", (parser, x) => x.Volume = parser.ParseInteger() },
            { "Type", (parser, x) => x.Type = parser.ParseEnumFlags<AudioTypeFlags>() },
            { "Priority", (parser, x) => x.Priority = parser.ParseEnum<AudioPriority>() },
            { "MinRange", (parser, x) => x.MinRange = parser.ParseInteger() },
            { "MaxRange", (parser, x) => x.MaxRange = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public string Filename { get; private set; }
        public int Volume { get; private set; } = 100;
        public AudioTypeFlags Type { get; private set; }
        public AudioPriority Priority { get; private set; } = AudioPriority.Normal;
        public int MinRange { get; private set; }
        public int MaxRange { get; private set; }
    }
}
