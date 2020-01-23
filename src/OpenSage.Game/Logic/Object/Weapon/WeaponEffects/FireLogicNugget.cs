using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class FireLogicNugget : DamageNugget
    {
        internal static new FireLogicNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FireLogicNugget> FieldParseTable = DamageNugget.FieldParseTable
            .Concat(new IniParseTable<FireLogicNugget>
            {
                { "LogicType", (parser, x) => x.LogicType = parser.ParseEnum<FireLogicType>() },
                { "MinMaxBurnRate", (parser, x) => x.MinMaxBurnRate = parser.ParseInteger() },
                { "MinDecay", (parser, x) => x.MinDecay = parser.ParseInteger() },
                { "MaxResistance", (parser, x) => x.MaxResistance = parser.ParseInteger() },
            });

        public FireLogicType LogicType { get; private set; }
        public int MinMaxBurnRate { get; private set; }
        public int MinDecay { get; private set; }
        public int MaxResistance { get; private set; }
    }
}
