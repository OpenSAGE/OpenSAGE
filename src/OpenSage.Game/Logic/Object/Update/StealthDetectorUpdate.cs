using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Display MESSAGE:StealthDiscovered when triggered.
    /// </summary>
    public sealed class StealthDetectorUpdate : ObjectBehavior
    {
        internal static StealthDetectorUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<StealthDetectorUpdate> FieldParseTable = new IniParseTable<StealthDetectorUpdate>
        {
            { "DetectionRate", (parser, x) => x.DetectionRate = parser.ParseInteger() },
            { "InitiallyDisabled", (parser, x) => x.InitiallyDisabled = parser.ParseBoolean() },
            { "DetectionRange", (parser, x) => x.DetectionRange = parser.ParseInteger() },
            { "CanDetectWhileGarrisoned", (parser, x) => x.CanDetectWhileGarrisoned = parser.ParseBoolean() },
            { "CanDetectWhileContained", (parser, x) => x.CanDetectWhileContained = parser.ParseBoolean() },
            { "ExtraRequiredKindOf", (parser, x) => x.ExtraRequiredKindOf = parser.ParseEnum<ObjectKinds>() },
        };

        /// <summary>
        /// How often, in milliseconds, to scan for stealthed objects in sight range.
        /// </summary>
        public int DetectionRate { get; private set; }

        public bool InitiallyDisabled { get; private set; }

        public int DetectionRange { get; private set; }

        public bool CanDetectWhileGarrisoned { get; private set; }

        public bool CanDetectWhileContained { get; private set; }

        public ObjectKinds ExtraRequiredKindOf { get; private set; }
    }
}
