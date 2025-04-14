
#nullable enable

using System;

namespace OpenSage.Utilities.Extensions;

public static class ArraySegmentExtensions
{
    public static int FindIndex<T>(this ArraySegment<T> segment, Predicate<T> predicate)
    {
        if (segment.Array == null)
        {
            throw new InvalidStateException("ArraySegment is not initialized.");
        }

        var indexInFullArray = Array.FindIndex(segment.Array, segment.Offset, segment.Count, predicate);

        if (indexInFullArray == -1)
        {
            return -1;
        }

        return indexInFullArray - segment.Offset;
    }

    public static T? Find<T>(this ArraySegment<T> segment, Predicate<T> predicate)
    {
        var index = segment.FindIndex(predicate);
        if (index == -1)
        {
            return default;
        }
        return segment[index];
    }
}
