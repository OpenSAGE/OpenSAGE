using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public class EmotionWeaponNugget : WeaponEffectNugget
    {
        internal static EmotionWeaponNugget Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<EmotionWeaponNugget> FieldParseTable = WeaponEffectNugget.FieldParseTable
            .Concat(new IniParseTable<EmotionWeaponNugget>
            {
                { "EmotionType", (parser, x) => x.EmotionType = parser.ParseEnum<EmotionType>() },
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "Duration", (parser, x) => x.Duration = parser.ParseInteger() },
            });

        public EmotionType EmotionType { get; private set; }
        public int Radius { get; private set; }
        public int Duration { get; private set; }
    }
}
