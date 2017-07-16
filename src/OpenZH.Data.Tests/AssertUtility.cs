using System;
using System.Collections.Generic;
using Xunit;

namespace OpenZH.Data.Tests
{
    internal static class AssertUtility
    {
        // XUnit's built-in collection assert doesn't show *where* the difference is, if the test fails.
        public static void Equal<T>(T[] array1, T[] array2)
            where T : struct
        {
            Assert.Equal(array1.Length, array2.Length);

            var comparer = EqualityComparer<T>.Default;

            var differentIndices = new List<int>();
            for (var i = 0; i < array1.Length; i++)
            {
                if (!comparer.Equals(array1[i], array2[i]))
                {
                    differentIndices.Add(i);
                }
            }
            if (differentIndices.Count > 0)
            {
                var message = string.Empty;
                foreach (var index in differentIndices)
                {
                    message += $"Different values at index {index}. Expected: {array1[index]}. Actual: {array2[index]}.{Environment.NewLine}";
                }
                throw new Exception(message);
            }
        }
    }
}
