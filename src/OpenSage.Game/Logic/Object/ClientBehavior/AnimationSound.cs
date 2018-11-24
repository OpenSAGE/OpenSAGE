using System;
using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class AnimationSoundData
    {
        internal static AnimationSoundData Parse(IniParser parser)
        {
            return new AnimationSoundData
            {
                Sound = parser.ParseAttribute("Sound", parser.ScanAssetReference),
                Animation = parser.ParseAttribute("Animation", parser.ScanAssetReference),
                Frames = parser.ParseAttribute("Frames", parser.ParseInteger),
            };
        }

        public string Sound { get; private set; }
        public string Animation { get; private set; }
        public int Frames { get; private set; }
    }
}
