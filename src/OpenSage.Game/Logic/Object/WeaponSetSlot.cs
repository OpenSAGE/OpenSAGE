using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSetSlot
    {
        public string Weapon { get; internal set; }
        public CommandSourceTypes AutoChooseSources { get; internal set; }
        public BitArray<ObjectKinds> PreferredAgainst { get; internal set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ObjectKinds> OnlyAgainst { get; internal set; }

        [AddedIn(SageGame.Bfme2)]
        public BitArray<ModelConditionFlag> OnlyInCondition { get; internal set; }
    }
}
