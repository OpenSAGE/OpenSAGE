using System;
using System.IO;

namespace OpenZH.Data.Tests
{
    internal static class TestUtility
    {
        public static void DoRoundtripTest<T>(
            Func<Stream> getOriginalStream,
            Func<Stream, T> parseCallback,
            Action<T, Stream> serializeCallback)
        {
            byte[] originalUncompressedBytes;
            using (var originalUncompressedStream = new MemoryStream())
            using (var entryStream = getOriginalStream())
            {
                entryStream.CopyTo(originalUncompressedStream);
                originalUncompressedBytes = originalUncompressedStream.ToArray();
            }

            T parsedFile;
            using (var entryStream = new MemoryStream(originalUncompressedBytes, false))
            {
                parsedFile = parseCallback(entryStream);
            }

            byte[] serializedBytes;
            using (var serializedStream = new MemoryStream())
            {
                serializeCallback(parsedFile, serializedStream);
                serializedBytes = serializedStream.ToArray();
            }

            AssertUtility.Equal(originalUncompressedBytes, serializedBytes);
        }
    }
}
