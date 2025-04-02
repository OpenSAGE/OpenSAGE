using OpenSage.Content;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public sealed class WeaponSetSlot
{
    public LazyAssetReference<WeaponTemplate> Weapon { get; internal set; }
    public BitArray<CommandSourceType> AutoChooseSources { get; internal set; } = BitArray<CommandSourceType>.CreateAllSet();
    public BitArray<ObjectKinds> PreferredAgainst { get; internal set; }

    [AddedIn(SageGame.Bfme)]
    public BitArray<ObjectKinds> OnlyAgainst { get; internal set; }

    [AddedIn(SageGame.Bfme2)]
    public BitArray<ModelConditionFlag> OnlyInCondition { get; internal set; }
}
