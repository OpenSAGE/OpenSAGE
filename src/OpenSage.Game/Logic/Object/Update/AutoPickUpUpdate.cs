using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class AutoPickUpUpdateModuleData : UpdateModuleData
    {
        internal static AutoPickUpUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<AutoPickUpUpdateModuleData> FieldParseTable = new IniParseTable<AutoPickUpUpdateModuleData>
        {
            { "ScanDelayTime", (parser, x) => x.ScanDelayTime = parser.ParseInteger() },
            { "PickUpKindOf", (parser, x) => x.PickUpKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
        };

        public int ScanDelayTime { get; private set; }
        public BitArray<ObjectKinds> PickUpKindOf { get; private set; }
    }
}
