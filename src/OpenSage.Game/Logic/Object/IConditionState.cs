using System;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public interface IConditionState<T>
    where T : Enum
{
    ReadOnlySpan<BitArray<T>> ConditionFlags { get; }
}
