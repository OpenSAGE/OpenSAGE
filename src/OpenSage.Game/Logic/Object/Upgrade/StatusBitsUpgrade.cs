using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class StatusBitsUpgradeModuleData : UpgradeModuleData
    {
        internal static StatusBitsUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<StatusBitsUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<StatusBitsUpgradeModuleData>
            {
                { "StatusToSet", (parser, x) => x.StatusToSet = parser.ParseEnumBitArray<Status>() }
            });

        [AddedIn(SageGame.Bfme2Rotwk)]
        public BitArray<Status> StatusToSet { get; private set; }
    }

    public enum Status
    {
        [IniEnum("IGNORE_AI_COMMAND")]
        IgnoreAICommand,

        [IniEnum("SUMMONING_REPLACEMENT")]
        SummoningReplacement
    }
}
