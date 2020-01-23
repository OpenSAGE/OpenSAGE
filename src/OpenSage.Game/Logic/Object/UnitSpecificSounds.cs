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

    public sealed class UnitSpecificSounds
    {
        internal static UnitSpecificSounds Parse(IniParser parser)
        {
            return parser.ParseBlock(new IniArbitraryFieldParserProvider<UnitSpecificSounds>(
                (x, name) => x.Assets[name] = parser.ParseAudioEventReference()));
        }

        public Dictionary<string, LazyAssetReference<BaseAudioEventInfo>> Assets { get; } = new Dictionary<string, LazyAssetReference<BaseAudioEventInfo>>();
    }
}
