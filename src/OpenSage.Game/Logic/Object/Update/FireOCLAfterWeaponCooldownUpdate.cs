using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class FireOCLAfterWeaponCooldownUpdateModuleData : UpdateModuleData
    {
        internal static FireOCLAfterWeaponCooldownUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<FireOCLAfterWeaponCooldownUpdateModuleData> FieldParseTable = new IniParseTable<FireOCLAfterWeaponCooldownUpdateModuleData>
        {
            { "WeaponSlot", (parser, x) => x.WeaponSlot = parser.ParseEnum<WeaponSlot>() },
            { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() },
            { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
            { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
            { "MinShotsToCreateOCL", (parser, x) => x.MinShotsToCreateOCL = parser.ParseInteger() },
            { "OCLLifetimePerSecond", (parser, x) => x.OCLLifetimePerSecond = parser.ParseInteger() },
            { "OCLLifetimeMaxCap", (parser, x) => x.OCLLifetimeMaxCap = parser.ParseInteger() }
        };

        public WeaponSlot WeaponSlot { get; private set; }
        public string[] TriggeredBy { get; private set; }
        public string[] ConflictsWith { get; private set; }
        public string OCL { get; private set; }
        public int MinShotsToCreateOCL { get; private set; }
        public int OCLLifetimePerSecond { get; private set; }
        public int OCLLifetimeMaxCap { get; private set; }
    }
}
