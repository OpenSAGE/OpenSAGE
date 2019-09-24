﻿using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    [AddedIn(SageGame.Bfme)]
    public sealed class StreamedSound : BaseSingleSound
    {
        internal static StreamedSound Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("StreamedSound", name),
                FieldParseTable);
        }

        private static new readonly IniParseTable<StreamedSound> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<StreamedSound>
            {
                { "Filename", (parser, x) => x.Filename = parser.ParseAssetReference() },
            });

        public string Filename { get; private set; }
    }
}
