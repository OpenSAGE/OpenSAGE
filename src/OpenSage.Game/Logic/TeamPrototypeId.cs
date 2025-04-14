#nullable enable

namespace OpenSage.Logic;

public readonly record struct TeamPrototypeId(uint Index)
{
    public static TeamPrototypeId Invalid => new(0);
    public bool IsValid => Index != 0;
    public bool IsInvalid => Index == 0;

    public static TeamPrototypeId operator ++(TeamPrototypeId id) => new(id.Index + 1);
}
