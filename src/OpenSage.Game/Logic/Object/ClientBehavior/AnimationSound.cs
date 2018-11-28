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
            var result = new AnimationSoundData();
            parser.ParseAttribute("Sound", parser.ScanAssetReference);
            parser.GetNextTokenOptional();
            parser.GetNextTokenOptional();
            parser.GetNextTokenOptional();
            parser.GetNextTokenOptional();
            return result;

            //return new AnimationSoundData
            //{
            //    //TODO: parse this shit
            //    //AnimationSound = Sound:OliphantFootStep	ExcludedMC:WADING	Animation:MUMumakil_SKL.MUMumakil_DECL4	Frames:59 68
            //    //AnimationSound = Sound:OliphantFootstepWater	RequiredMC:WADING	Animation:MUMumakil_SKL.MUMumakil_DIEA	Frames:90
            //    Sound = parser.ParseAttribute("Sound", parser.ScanAssetReference),
            //    Animation = parser.ParseAttribute("Animation", parser.ScanAssetReference),
            //    Frames = parser.ParseAttribute("Frames", parser.ParseInteger),
            //};
        }

        public string Sound { get; private set; }
        public string Animation { get; private set; }
        public int Frames { get; private set; }
    }
}
