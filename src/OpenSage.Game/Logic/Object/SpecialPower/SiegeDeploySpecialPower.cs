using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class SiegeDeploySpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new SiegeDeploySpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SiegeDeploySpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<SiegeDeploySpecialPowerModuleData>
            {
                { "LowerDelay", (parser, x) => x.LowerDelay = parser.ParseInteger() },
                { "RaiseDelay", (parser, x) => x.RaiseDelay = parser.ParseInteger() },
                { "EvacuatePassengersOnDeploy", (parser, x) => x.EvacuatePassengersOnDeploy = parser.ParseBoolean() },
                { "SkipAdjustPosition", (parser, x) => x.SkipAdjustPosition = parser.ParseBoolean() },
                { "WallSearchDistance", (parser, x) => x.WallSearchDistance = parser.ParseFloat() }
            });

        public int LowerDelay { get; private set; }
        public int RaiseDelay { get; private set; }
        public bool EvacuatePassengersOnDeploy { get; private set; }
        public bool SkipAdjustPosition { get; private set; }
        public float WallSearchDistance { get; private set; }
    }
}
