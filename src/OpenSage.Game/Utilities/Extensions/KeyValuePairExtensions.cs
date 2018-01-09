using System.Collections.Generic;

namespace OpenSage.Utilities.Extensions
{
    public static class KeyValuePairExtensions
    {
        // https://github.com/dotnet/corefx/issues/13746#issuecomment-308172282
        // Enables KeyValuePairs to be tuple deconstructed.
        // Note: This is unnecessary in .NET Core, because it includes this OOB.
        // .NET Framework doesn't include it yet.
        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> source, out TKey key, out TValue value)
        {
            key = source.Key;
            value = source.Value;
        }
    }
}
