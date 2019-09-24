using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.StreamFS;

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

        internal static DialogEvent ParseAsset(BinaryReader reader, Asset asset, AssetImportCollection imports)
        {
            var result = new DialogEvent();
            result.SetNameAndInstanceId(asset);

            ParseAsset(reader, result);

            result.File = new LazyAssetReference<AudioFile>(imports.GetImportedData<AudioFile>(reader));

            return result;
        }

        public LazyAssetReference<AudioFile> File { get; private set; }
    }
}
