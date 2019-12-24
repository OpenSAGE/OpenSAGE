﻿using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;

namespace OpenSage.Audio
{
    public sealed class AudioEvent : BaseSingleSound
    {
        internal static AudioEvent Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("AudioEvent", name),
                FieldParseTable);
        }

        private static new readonly IniParseTable<AudioEvent> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<AudioEvent>
            {
                { "Sounds", (parser, x) => x.Sounds = parser.ParseAudioFileWithWeightArray() },
                { "Attack", (parser, x) => x.Attack = parser.ParseAudioFileWithWeightArray() },
                { "Decay", (parser, x) => x.Decay = parser.ParseAudioFileWithWeightArray() },
                { "LoopCount", (parser, x) => x.LoopCount = parser.ParseInteger() },
            });

        internal static AudioEvent ParseAsset(BinaryReader reader, Asset asset, AssetImportCollection imports)
        {
            var result = new AudioEvent();
            result.SetNameAndInstanceId(asset);

            ParseAsset(reader, result);

            result.Attack = reader.ReadArrayAtOffset(() => AudioFileWithWeight.ParseAsset(reader, imports));
            result.Sounds = reader.ReadArrayAtOffset(() => AudioFileWithWeight.ParseAsset(reader, imports));
            result.Decay = reader.ReadArrayAtOffset(() => AudioFileWithWeight.ParseAsset(reader, imports));

            return result;
        }

        public AudioFileWithWeight[] Sounds { get; private set; }
        public AudioFileWithWeight[] Attack { get; private set; }
        public AudioFileWithWeight[] Decay { get; private set; }
        public int LoopCount { get; private set; }
    }

    public sealed class AudioFileWithWeight
    {
        internal static AudioFileWithWeight ParseAsset(BinaryReader reader, AssetImportCollection imports)
        {
            return new AudioFileWithWeight
            {
                AudioFile = imports.GetImportedData<AudioFile>(reader),
                Weight = reader.ReadUInt32()
            };
        }

        public LazyAssetReference<AudioFile> AudioFile { get; internal set; }
        public uint Weight { get; private set; } = 1000;
    }
}
