using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class InstantDeathBehaviorModuleData : BehaviorModuleData
    {
        internal static InstantDeathBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<InstantDeathBehaviorModuleData> FieldParseTable = new IniParseTable<InstantDeathBehaviorModuleData>
        {
            { "RequiredStatus", (parser, x) => x.RequiredStatus = parser.ParseEnum<ObjectStatus>() },
            { "DeathTypes", (parser, x) => x.DeathTypes = parser.ParseEnumBitArray<DeathType>() },
            {
                "FX",
                (parser, x) =>
                {
                    x.FX = parser.ParseAssetReference();

                    // ODDITY: ZH WeaponObjects.ini:5838 incorrectly uses a SlowDeathPhase (FINAL)
                    // before the actual FX reference.
                    if (x.FX == "FINAL" && parser.CurrentTokenType == IniTokenType.Identifier)
                    {
                        x.FX = parser.ParseAssetReference();
                    }
                }
            },
            { "OCL", (parser, x) => x.OCL = parser.ParseAssetReference() },
        };

        public ObjectStatus RequiredStatus { get; private set; }
        public BitArray<DeathType> DeathTypes { get; private set; }
        public string FX { get; private set; }
        public string OCL { get; private set; }
    }
}
