using System;

namespace OpenSage.Content
{
    public static class MipMapUtility
    {
        public static uint CalculateMipMapCount(uint width, uint height)
        {
            return 1u + (uint) Math.Floor(Math.Log(Math.Max(width, height), 2));
        }
    }
}
