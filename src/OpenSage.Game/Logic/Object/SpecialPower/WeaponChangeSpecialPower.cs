using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class WeaponChangeSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new WeaponChangeSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WeaponChangeSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<WeaponChangeSpecialPowerModuleData>
            {
                { "FlagsUsedForToggle", (parser, x) => x.FlagsUsedForToggle = parser.ParseEnumFlags<WeaponSetConditions>() },
                { "ToggleOnSleepFrames", (parser, x) => x.ToggleOnSleepFrames = parser.ParseInteger() },
                { "ToggleOffSleepFrames", (parser, x) => x.ToggleOffSleepFrames = parser.ParseInteger() },
                { "ToggleOnAttributeModifier", (parser, x) => x.ToggleOnAttributeModifier = parser.ParseAssetReference() },
            });

        public WeaponSetConditions FlagsUsedForToggle { get; private set; }
        public int ToggleOnSleepFrames { get; private set; }
        public int ToggleOffSleepFrames { get; private set; }
        public string ToggleOnAttributeModifier { get; private set; }
    }
}
