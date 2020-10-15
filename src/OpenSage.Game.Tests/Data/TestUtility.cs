using System;
using System.IO;
using Xunit;

namespace OpenSage.Tests.Data
{
    internal static class TestUtility
    {
        public static T DoRoundtripTest<T>(
            Func<Stream> getOriginalStream,
            Func<Stream, T> parseCallback,
            Action<T, Stream> serializeCallback,
            bool skipRoundtripEqualityTest = false)
        {
            byte[] originalUncompressedBytes;
            using (var originalUncompressedStream = new MemoryStream())
            using (var entryStream = getOriginalStream())
            {
                entryStream.CopyTo(originalUncompressedStream);
                originalUncompressedBytes = originalUncompressedStream.ToArray();
            }

            T parsedFile = default;
            try
            {
                using (var entryStream = new MemoryStream(originalUncompressedBytes, false))
                {
                    parsedFile = parseCallback(entryStream);
                }
            }
            catch
            {
                File.WriteAllBytes("original.bin", originalUncompressedBytes);
                throw;
            }

            byte[] serializedBytes;
            using (var serializedStream = new MemoryStream())
            {
                serializeCallback(parsedFile, serializedStream);
                serializedBytes = serializedStream.ToArray();
            }

            if (originalUncompressedBytes.Length != serializedBytes.Length)
            {
                File.WriteAllBytes("original.bin", originalUncompressedBytes);
                File.WriteAllBytes("serialized.bin", serializedBytes);
            }

            Assert.Equal(originalUncompressedBytes.Length, serializedBytes.Length);

            if (!skipRoundtripEqualityTest)
            {
                AssertUtility.Equal(originalUncompressedBytes, serializedBytes);
            }

            return parsedFile;
        }
    }
}
