#nullable enable

namespace OpenSage.Logic;

public readonly record struct PlayerIndex(int Value) : ISideId<PlayerIndex>
{
    public static PlayerIndex Invalid => new(-1);
    public bool IsValid => Value != -1;
    public bool IsInvalid => Value == -1;

    public void Persist(ref PlayerIndex value, StatePersister persister, string fieldName)
    {
        persister.PersistPlayerIndex(ref value, fieldName);
    }
}
