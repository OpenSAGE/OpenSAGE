using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    // TODO: These could be resolved at INI load time, since I think the ReskinOf object
    // always appears before the ObjectReskin that references it. Then the game can deal
    // only with ObjectDefinition.
    public sealed class ObjectReskin : ObjectDefinition
    {
        internal static new ObjectReskin Parse(IniParser parser)
        {
            parser.NextToken();

            var name = parser.ParseIdentifier();

            var reskinOf = parser.ParseAssetReference();

            var result = parser.ParseBlock(ReskinFieldParseTable);

            parser.NextTokenIf(IniTokenType.EndOfLine);

            result.Name = name;
            result.ReskinOf = reskinOf;

            return result;
        }

        public string ReskinOf { get; private set; }

        internal static readonly IniParseTable<ObjectReskin> ReskinFieldParseTable = new IniParseTable<ObjectReskin>()
            .Concat<ObjectReskin, ObjectDefinition>(FieldParseTable);
    }
}
