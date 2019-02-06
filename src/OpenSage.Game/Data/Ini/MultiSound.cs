using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class Multisound
    {
        internal static Multisound Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Multisound> FieldParseTable = new IniParseTable<Multisound>
        {
            { "Subsounds", (parser, x) => x.Subsounds = parser.ParseAssetReferenceArray() },
            { "Control", (parser, x) => x.Control = parser.ParseEnum<MultisoundControl>() },
        };

        public string Name { get; private set; }
        public string[] Subsounds { get; private set; }
        public MultisoundControl Control { get; private set; }
    }

    public enum MultisoundControl
    {
        [IniEnum("PLAY_ONE")]
        PlayOne,
    }
}
