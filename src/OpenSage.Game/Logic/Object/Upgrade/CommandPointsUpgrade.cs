using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class CommandPointsUpgradeModuleData : UpgradeModuleData
    {
        internal static CommandPointsUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CommandPointsUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<CommandPointsUpgradeModuleData>
            {
                { "CommandPoints", (parser, x) => x.CommandPoints = parser.ParseInteger() },
                { "RequiredObject", (parser, x) => x.RequiredObject = ObjectFilter.Parse(parser) }
            });

        public int CommandPoints { get; private set; }
        public ObjectFilter RequiredObject { get; private set; }
    }
}
