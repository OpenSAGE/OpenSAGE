using System;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class AmbientStream
    {
        internal static AmbientStream Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<AmbientStream> FieldParseTable = new IniParseTable<AmbientStream>
        {
            { "Filename", (parser, x) => x.FileName = parser.ParseFileName() },
            { "Volume", (parser, x) => x.Volume = parser.ParseInteger() },
            { "Type", (parser, x) => x.Type = parser.ParseEnumFlags<AudioTypeFlags>() }
        };

        public string Name { get; private set; }

        public string FileName { get; private set; }
        public int Volume { get; private set; }
        public AudioTypeFlags Type { get; private set; }
    }
}
