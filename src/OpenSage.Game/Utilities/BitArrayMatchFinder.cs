using System;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Utilities;

// TODO(Port): Check if the logic here matches SparseMatchFinder.h.

internal static class BitArrayMatchFinder
{
    public static T FindBest<T, TFlag>(Span<T> set, BitArray<TFlag> flags)
        where T : IConditionState<TFlag>
        where TFlag : Enum
    {
        // prefer exact match
        // if not, find the first one with the most number of matching bits
        // this may be convoluted, but the unit tests pass!
        T best = default;
        var bestIntersections = 0;
        T bestContained = default;
        var leastMissing = int.MaxValue;
        var leastMissingBestIntersections = 0;
        T defaultItem = default;

        foreach (var item in set)
        {
            foreach (var flagSet in item.ConditionFlags)
            {
                // the default condition state has no bits set
                if (!flagSet.AnyBitSet)
                {
                    defaultItem = item;
                }

                var intersections = flagSet.CountIntersectionBits(flags);
                // we've found a state that matches everything we have, but it may have extra things we don't have that we don't want
                // the order of the condition states in the ini files is not guaranteed to be ideal
                if (intersections == flags.NumBitsSet)
                {
                    var missing = flagSet.NumBitsSet - intersections;
                    if (missing == 0)
                    {
                        // if we have an exact match, great! doesn't matter what else we've found
                        return item;
                    }

                    // otherwise, this state may have extra things we don't have. Cool, but less than ideal... let's keep searching?
                    // this ensures we don't store the first least missing as better, even if a subsequent one has better best intersections (which should be a better "score")
                    if (missing < leastMissing || missing == leastMissing && intersections > leastMissingBestIntersections)
                    {
                        bestContained = item;
                        leastMissing = missing;
                        leastMissingBestIntersections = intersections;
                    }
                }
                else if (intersections > bestIntersections)
                {
                    // not an exact match, but save this if we can't find an exact match
                    bestIntersections = flagSet.NumBitsSet;
                    best = item;
                }
            }
        }

        return bestContained ?? best ?? defaultItem;
    }
}
