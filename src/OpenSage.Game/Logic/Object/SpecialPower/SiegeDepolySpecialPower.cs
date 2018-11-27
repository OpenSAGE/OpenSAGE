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
                { "InitiateSound", (parser, x) => x.InitiateSound = parser.ParseAssetReference() },
                { "InitiateSound2", (parser, x) => x.InitiateSound2 = parser.ParseAssetReference() },
            });

        public int LowerDelay { get; internal set; }
        public int RaiseDelay { get; internal set; }
        public bool EvacuatePassengersOnDeploy { get; internal set; }
        public bool SkipAdjustPosition { get; internal set; }
        public string InitiateSound { get; internal set; }
        public string InitiateSound2 { get; internal set; }
    }
}
