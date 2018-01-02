using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Utilities.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Complement of Where.
        /// </summary>
        public static IEnumerable<T> WhereNot<T>(this IEnumerable<T> enumerable, Predicate<T> predicate)
        {
            return enumerable.Where(x => !predicate(x));
        }

        /// <summary>
        /// Creates a HashSet from an enumerable.
        /// </summary>
        public static HashSet<T> ToSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }
    }
}
