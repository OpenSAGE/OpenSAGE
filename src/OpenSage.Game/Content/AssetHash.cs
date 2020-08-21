using System;

namespace OpenSage.Content
{
    internal static class AssetHash
    {
        public static uint GetHash(string input)
        {
            if (input.Length == 0)
            {
                return 0u;
            }

            var buffer = input.AsSpan();

            static byte GetByte(ReadOnlySpan<char> span, int index)
            {
                return (byte) char.ToLowerInvariant(span[index]);
            }

            var numBlocks = buffer.Length >> 2;
            var extra = buffer.Length % 4;

            var hash = (uint) buffer.Length;

            var idy = 0;
            for (var idx = numBlocks; idx != 0; --idx)
            {
                hash += (uint) (GetByte(buffer, idy + 1) << 8 | GetByte(buffer, idy));
                hash ^= ((uint) (GetByte(buffer, idy + 3) << 8 | GetByte(buffer, idy + 2)) ^ (hash << 5)) << 11;
                hash += hash >> 11;
                idy += 4;
            }

            if (extra != 0)
            {
                switch (extra)
                {
                    case 1:
                        hash += GetByte(buffer, idy);
                        hash = (hash << 10) ^ hash;
                        hash += hash >> 1;
                        break;
                    case 2:
                        hash += (uint) (GetByte(buffer, idy + 1) << 8 | GetByte(buffer, idy));
                        hash ^= hash << 11;
                        hash += hash >> 17;
                        break;
                    case 3:
                        hash += (uint) (GetByte(buffer, idy + 1) << 8 | GetByte(buffer, idy));
                        hash ^= (hash ^ (uint) (GetByte(buffer, idy + 2) << 2)) << 16;
                        hash += hash >> 11;
                        break;
                }
            }

            hash ^= hash << 3;
            hash += hash >> 5;
            hash ^= hash << 2;
            hash += hash >> 15;
            hash ^= hash << 10;

            return hash;
        }
    }
}
