using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class StoreObjectsSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new StoreObjectsSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StoreObjectsSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<StoreObjectsSpecialPowerModuleData>
            {
                { "StartAbilityRange", (parser, x) => x.StartAbilityRange = parser.ParseFloat() },
                { "ApproachRequiresLOS", (parser, x) => x.ApproachRequiresLOS = parser.ParseBoolean() },
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "UnpackTime", (parser, x) => x.UnpackTime = parser.ParseInteger() },
                { "PreparationTime", (parser, x) => x.PreparationTime = parser.ParseInteger() },
                { "FreezeAfterTriggerDuration", (parser, x) => x.FreezeAfterTriggerDuration = parser.ParseInteger() },
                { "ChainedButton", (parser, x) => x.ChainedButton = parser.ParseQuotedString() }
            });

        public float StartAbilityRange { get; private set; }
        public bool ApproachRequiresLOS { get; private set; }
        public int Radius { get; private set; }
        public int UnpackTime { get; private set; }
        public int PreparationTime { get; private set; }
        public int FreezeAfterTriggerDuration { get; private set; }
        public string ChainedButton { get; private set; }
    }
}
