using System.IO;
using OpenSage.Content;
using OpenSage.Data.StreamFS;
using OpenSage.FileFormats;
using OpenSage.Logic.Object;

namespace OpenSage.Audio
{
    public sealed class AudioObjectSpecificVoiceEntry : SoundOrEvaEvent
    {
        public ThingTemplateObjectSpecificVoiceType AudioType { get; private set; }
        public LazyAssetReference<ObjectDefinition> TargetObject { get; private set; }

        internal static AudioObjectSpecificVoiceEntry ParseAsset(BinaryReader reader, AssetImportCollection imports)
        {
            var result = new AudioObjectSpecificVoiceEntry();

            ParseAsset(reader, result, imports);

            result.AudioType = reader.ReadUInt32AsEnum<ThingTemplateObjectSpecificVoiceType>();
            result.TargetObject = imports.GetImportedData<ObjectDefinition>(reader);

            return result;
        }
    }
}
