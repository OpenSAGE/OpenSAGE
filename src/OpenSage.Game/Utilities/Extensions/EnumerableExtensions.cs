using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        /// <summary>
        /// Returns the smallest element in a sequence based on a transform function.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements.</typeparam>
        /// <typeparam name="TComparison">The result type of the transform function which is used for comparing.</typeparam>
        /// <param name="elements">The elements.</param>
        /// <param name="selector">The transform function.</param>
        /// <returns>The smallest element.</returns>
        public static TElement MinBy<TElement, TComparison>(this IEnumerable<TElement> elements, Func<TElement, TComparison> selector)
        {
            return MinMaxBy(elements, selector, max: false, throwWhenEmpty: true);
        }

        /// <summary>
        /// Returns the smallest element in a sequence based on a transform function, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements.</typeparam>
        /// <typeparam name="TComparison">The result type of the transform function which is used for comparing.</typeparam>
        /// <param name="elements">The elements.</param>
        /// <param name="selector">The transform function.</param>
        /// <returns>The smallest element.</returns>
        [return: MaybeNull]
        public static TElement MinByOrDefault<TElement, TComparison>(this IEnumerable<TElement> elements, Func<TElement, TComparison> selector)
        {
            return MinMaxBy(elements, selector, max: false, throwWhenEmpty: false);
        }

        /// <summary>
        /// Returns the largest element in a sequence based on a transform function.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements.</typeparam>
        /// <typeparam name="TComparison">The result type of the transform function which is used for comparing.</typeparam>
        /// <param name="elements">The elements.</param>
        /// <param name="selector">The transform function.</param>
        /// <returns>The largest element.</returns>
        public static TElement MaxBy<TElement, TComparison>(this IEnumerable<TElement> elements, Func<TElement, TComparison> selector)
        {
            return MinMaxBy(elements, selector, max: true, throwWhenEmpty: true);
        }

        /// <summary>
        /// Returns the largest element in a sequence based on a transform function, or a default value if the sequence contains no elements.
        /// </summary>
        /// <typeparam name="TElement">The type of the elements.</typeparam>
        /// <typeparam name="TComparison">The result type of the transform function which is used for comparing.</typeparam>
        /// <param name="elements">The elements.</param>
        /// <param name="selector">The transform function.</param>
        /// <returns>The largest element.</returns>
        [return: MaybeNull]
        public static TElement MaxByOrDefault<TElement, TComparison>(this IEnumerable<TElement> elements, Func<TElement, TComparison> selector)
        {
            return MinMaxBy(elements, selector, max: true, throwWhenEmpty: false);
        }

        [return: MaybeNull]
        private static TElement MinMaxBy<TElement, TComparison>(this IEnumerable<TElement> elements, Func<TElement, TComparison> selector, bool max, bool throwWhenEmpty)
        {
            var enumerator = elements.GetEnumerator();
            if (!enumerator.MoveNext())
            {
                if (throwWhenEmpty)
                {
                    throw new InvalidOperationException("Sequence is empty.");
                }
                else
                {
                    return default!;
                }
            }

            var minMax = enumerator.Current;
            var minMaxValue = selector(minMax);
            var comparer = Comparer<TComparison>.Default;

            while (enumerator.MoveNext())
            {
                var value = selector(enumerator.Current);
                var result = comparer.Compare(value, minMaxValue);
                if ((max && result > 0) || (!max && result < 0))
                {
                    minMax = enumerator.Current;
                    minMaxValue = value;
                }
            }

            return minMax;
        }
    }
}
