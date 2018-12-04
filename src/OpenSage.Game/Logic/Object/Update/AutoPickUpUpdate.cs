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
            { "ScanDistance", (parser, x) => x.ScanDistance = parser.ParseInteger() },
            { "EatObjectEntry", (parser, x) => x.EatObjectEntry = EatObjectEntry.Parse(parser) },
            { "Bored", (parser, x) => x.Bored = parser.ParseBoolean() },
            { "BoredFilter", (parser, x) => x.BoredFilter = ObjectFilter.Parse(parser) },
            { "RunFromButton", (parser, x) => x.RunFromButton = parser.ParseBoolean() },
            { "RunFromButtonNumber", (parser, x) => x.RunFromButtonNumber = parser.ParseInteger() },
            { "PickUpFilter", (parser, x) => x.PickUpFilter = ObjectFilter.Parse(parser) }
        };

        public int ScanDelayTime { get; private set; }
        public BitArray<ObjectKinds> PickUpKindOf { get; private set; }
        public int ScanDistance { get; private set; }

        public EatObjectEntry EatObjectEntry { get; private set; }

        public bool Bored { get; private set; }
        public ObjectFilter BoredFilter { get; private set; }
        public bool RunFromButton { get; private set; }
        public int RunFromButtonNumber { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public ObjectFilter PickUpFilter { get; private set; }
    }

    public sealed class EatObjectEntry
    {
        internal static EatObjectEntry Parse(IniParser parser)
        {
            return new EatObjectEntry
            {
                MaxHealth = parser.ParseAttributePercentage("MyHealth"),
                TargetHealth = parser.ParseAttributePercentage("TargetHealth"),
                Filter = parser.ParseAttribute("Filter", ObjectFilter.Parse)
            };
        }

        public float MaxHealth { get; private set; }
        public float TargetHealth { get; private set; }
        public ObjectFilter Filter { get; private set; }
    }
}
