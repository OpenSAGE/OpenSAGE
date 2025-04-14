#nullable enable

using System;

namespace OpenSage.Logic;

public interface ISideId<TSelf> : IEquatable<TSelf>
{
    static abstract TSelf Invalid { get; }
    abstract bool IsValid { get; }
    abstract bool IsInvalid { get; }

    abstract void Persist(ref TSelf value, StatePersister persister, string fieldName);
}
