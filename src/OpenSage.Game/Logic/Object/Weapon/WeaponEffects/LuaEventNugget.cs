using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class LuaEventNugget : WeaponEffectNugget
    {
        internal static LuaEventNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<LuaEventNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<LuaEventNugget>
            {
                { "LuaEvent", (parser, x) => x.LuaEvent = parser.ParseAssetReference() },
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "SendToEnemies", (parser, x) => x.SendToEnemies = parser.ParseBoolean() },
                { "SendToAllies", (parser, x) => x.SendToAllies = parser.ParseBoolean() },
                { "SendToNeutral", (parser, x) => x.SendToNeutral = parser.ParseBoolean() },
            });

        public string LuaEvent { get; private set; }
        public int Radius { get; private set; }
        public bool SendToEnemies { get; private set; }
        public bool SendToAllies { get; private set; }
        public bool SendToNeutral { get; private set; }
    }
}
