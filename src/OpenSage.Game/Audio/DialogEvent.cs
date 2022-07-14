using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Audio
{
    public sealed class DialogEvent : BaseSingleSound
    {
        internal static DialogEvent Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("DialogEvent", name),
                FieldParseTable);
        }

        private static new readonly IniParseTable<DialogEvent> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<DialogEvent>
            {
                { "Filename", (parser, x) => x.File = parser.ParseAudioFileReference() }
            });

        public LazyAssetReference<AudioFile> File { get; private set; }
    }
}
