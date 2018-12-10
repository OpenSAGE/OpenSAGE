using System.Numerics;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class LivingWorldObject
    {
        internal static LivingWorldObject Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<LivingWorldObject> FieldParseTable = new IniParseTable<LivingWorldObject>
        {
            { "ObjectType", (parser, x) => x.ObjectType = parser.ParseString() },
            { "DefaultFlashValue", (parser, x) => x.DefaultFlashValue = parser.ParseFloat() },
            { "FlashVariation", (parser, x) => x.FlashVariation = parser.ParseFloat() },
        };

        public string Name { get; private set; }

        public string ObjectType { get; private set; }
        public float DefaultFlashValue { get; private set; }
        public float FlashVariation { get; private set; }
    }
}
