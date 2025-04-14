#nullable enable

using System;

namespace OpenSage.Logic;

public readonly record struct TeamId(uint Index) : ISideId<TeamId>
{
    public static TeamId Invalid => new(0);
    public bool IsValid => Index != 0;
    public bool IsInvalid => Index == 0;

    public static TeamId operator ++(TeamId id) => new(id.Index + 1);

    public static TeamId Max(TeamId a, TeamId b) => new(Math.Max(a.Index, b.Index));

    public void Persist(ref TeamId value, StatePersister persister, string fieldName)
    {
        persister.PersistTeamId(ref value, fieldName);
    }
}
