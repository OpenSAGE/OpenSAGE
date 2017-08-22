using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CleanupHazardUpdate : ObjectBehavior
    {
        internal static CleanupHazardUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CleanupHazardUpdate> FieldParseTable = new IniParseTable<CleanupHazardUpdate>
        {
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "ScanRate", (parser, x) => x.ScanRate = parser.ParseInteger() },
            { "ScanRange", (parser, x) => x.ScanRange = parser.ParseFloat() }
        };

        public WeaponSlot WeaponSlot { get; private set; }
        public int ScanRate { get; private set; }
        public float ScanRange { get; private set; }
    }
}
