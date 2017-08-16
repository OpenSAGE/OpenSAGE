using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class WeaponSetSlot
    {
        public string Weapon { get; internal set; }
        public CommandSourceTypes AutoChooseSources { get; internal set; }
        public BitArray<ObjectKinds> PreferredAgainst { get; internal set; }
    }
}
