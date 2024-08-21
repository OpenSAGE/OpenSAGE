#nullable enable

using System.Collections.Generic;
using OpenSage.Audio;
using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class UnitSpecificAssets
    {
        internal static UnitSpecificAssets Parse(IniParser parser)
        {
            return parser.ParseBlock(new IniArbitraryFieldParserProvider<UnitSpecificAssets>(
                (x, name) => x.Assets[name] = parser.ParseAssetReference()));
        }

        // These keys will eventually mean something to some code, as noted in FactionUnit.ini:32029.
        public Dictionary<string, string> Assets { get; } = new Dictionary<string, string>();
    }

    public sealed class UnitSpecificSounds : Dictionary<string, LazyAssetReference<BaseAudioEventInfo>>
    {
        /// <summary>
        /// The sound played when a unit is produced
        /// </summary>
        public LazyAssetReference<BaseAudioEventInfo>? VoiceCreate => TryGetValue("VoiceCreate", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceNoBuild => TryGetValue("VoiceNoBuild", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceCrush => TryGetValue("VoiceCrush", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceRepair => TryGetValue("VoiceRepair", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceDisarm => TryGetValue("VoiceDisarm", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceUnload => TryGetValue("VoiceUnload", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceEnter => TryGetValue("VoiceEnter", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceEnterHostile => TryGetValue("VoiceEnterHostile", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceGarrison => TryGetValue("VoiceGarrison", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceBuildResponse => TryGetValue("VoiceBuildResponse", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceGetHealed => TryGetValue("VoiceGetHealed", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceEject => TryGetValue("VoiceEject", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? UnitPack => TryGetValue("UnitPack", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? UnitUnpack => TryGetValue("UnitUnpack", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? UnitCashPing => TryGetValue("UnitCashPing", out var sound) ? sound : null;
        public LazyAssetReference<BaseAudioEventInfo>? VoiceHackInternet => TryGetValue("VoiceHackInternet", out var sound) ? sound : null;


        internal static UnitSpecificSounds Parse(IniParser parser)
        {
            return parser.ParseBlock(new IniArbitraryFieldParserProvider<UnitSpecificSounds>(
                (x, name) => x[name] = parser.ParseAudioEventReference()));
        }

        public UnitSpecificSounds() { }

        public UnitSpecificSounds(IEnumerable<KeyValuePair<string, LazyAssetReference<BaseAudioEventInfo>>> collection)
            : base(collection)
        {

        }
    }
}
