using OpenSage.Data.Ini;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FXParticleSysBoneNugget : FXNugget
    {
        internal static FXParticleSysBoneNugget Parse(IniParser parser)
        {
            return new FXParticleSysBoneNugget
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
