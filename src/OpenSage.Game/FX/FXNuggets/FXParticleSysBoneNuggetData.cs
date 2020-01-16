using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FXParticleSysBoneNuggetData : FXNuggetData
    {
        internal static FXParticleSysBoneNuggetData Parse(IniParser parser)
        {
            return new FXParticleSysBoneNuggetData
            {
                Bone = parser.ParseBoneName(),
                Particle = parser.ParseAssetReference(),
                FollowBone = parser.ParseAttributeBoolean("FollowBone")
            };
        }

        public string Bone { get; private set; }
        public string Particle { get; private set; }
        public bool FollowBone { get; private set; }
    }
}
