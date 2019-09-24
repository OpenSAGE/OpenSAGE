using OpenSage.Data.Ini;

namespace OpenSage.LivingWorld
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldObject : BaseAsset
    {
        internal static LivingWorldObject Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("LivingWorldObject", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldObject> FieldParseTable = new IniParseTable<LivingWorldObject>
        {
            { "ObjectType", (parser, x) => x.ObjectType = parser.ParseString() },
            { "DefaultFlashValue", (parser, x) => x.DefaultFlashValue = parser.ParseFloat() },
            { "FlashVariation", (parser, x) => x.FlashVariation = parser.ParseFloat() },
        };

        public string ObjectType { get; private set; }
        public float DefaultFlashValue { get; private set; }
        public float FlashVariation { get; private set; }
    }
}
