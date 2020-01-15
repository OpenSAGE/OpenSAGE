using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class DamageContainedNugget : DamageNuggetData
    {
        internal static new DamageContainedNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DamageContainedNugget> FieldParseTable = DamageNuggetData.FieldParseTable
            .Concat(new IniParseTable<DamageContainedNugget>
            {
                { "KillCount", (parser, x) => x.KillCount = parser.ParseInteger() },
                { "KillKindof", (parser, x) => x.KillKindof = parser.ParseEnum<ObjectKinds>() },
                { "KillKindofNot", (parser, x) => x.KillKindofNot = parser.ParseEnum<ObjectKinds>() },
            });

        public int KillCount { get; private set; }
        public ObjectKinds KillKindof { get; private set; }
        public ObjectKinds KillKindofNot { get; private set; }
    }
}
