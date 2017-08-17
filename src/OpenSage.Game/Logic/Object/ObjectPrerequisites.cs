using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ObjectPrerequisites
    {
        internal static ObjectPrerequisites Parse(IniParser parser)
        {
            return parser.ParseBlock(FieldParseTable);
        }

        private static readonly IniParseTable<ObjectPrerequisites> FieldParseTable = new IniParseTable<ObjectPrerequisites>
        {
            {
                "Object",
                (parser, x) =>
                {
                    x.Objects.Add(parser.ParseAssetReference());

                    // ODDITY: FactionBuilding.ini:6810, in the AmericaStrategyCenter definition,
                    // you'll find Object = AmericaWarFactory AmericaAirfield. The second value
                    // is ignored, since the strategy centre doesn't depend on the airfield.
                    if (parser.CurrentTokenType == IniTokenType.Identifier)
                    {
                        parser.ParseAssetReference();
                    }
                }
            },
            { "Science", (parser, x) => x.Sciences.Add(parser.ParseAssetReference()) }
        };

        public List<string> Objects { get; } = new List<string>();
        public List<string> Sciences { get; } = new List<string>();
    }
}
