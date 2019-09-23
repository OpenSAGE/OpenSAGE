using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class StopSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new StopSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StopSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<StopSpecialPowerModuleData>
            {
                { "StopPowerTemplate", (parser, x) => x.StopPowerTemplate = parser.ParseAssetReference() },
            });

        public string StopPowerTemplate { get; private set; }
    }
}
