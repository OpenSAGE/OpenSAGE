using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CleanupHazardUpdateModuleData : UpdateModuleData
    {
        internal static CleanupHazardUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CleanupHazardUpdateModuleData> FieldParseTable = new IniParseTable<CleanupHazardUpdateModuleData>
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
