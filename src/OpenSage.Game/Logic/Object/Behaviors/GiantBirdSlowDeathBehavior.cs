using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2Rotwk)]
    public class GiantBirdSlowDeathBehaviorModuleData : SlowDeathBehaviorModuleData
    {
        internal new static GiantBirdSlowDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal new static readonly IniParseTable<GiantBirdSlowDeathBehaviorModuleData> FieldParseTable = SlowDeathBehaviorModuleData.FieldParseTable
            .Concat(new IniParseTable<GiantBirdSlowDeathBehaviorModuleData>
        {
            { "CrashAvoidKindOfs", (parser, x) => x.CrashAvoidKindOfs = parser.ParseEnumBitArray<ObjectKinds>() },
            { "CrashAvoidRadius", (parser, x) => x.CrashAvoidRadius = parser.ParseInteger() },
            { "CrashAvoidStrength", (parser, x) => x.CrashAvoidStrength = parser.ParseFloat() },
            { "NeedToMaintainFlailingHeight", (parser, x) => x.NeedToMaintainFlailingHeight = parser.ParseBoolean() }
        });

        public BitArray<ObjectKinds> CrashAvoidKindOfs { get; private set; }
        public int CrashAvoidRadius { get; private set; }
        public float CrashAvoidStrength { get; private set; }
        public bool NeedToMaintainFlailingHeight { get; private set; }
    }
}
