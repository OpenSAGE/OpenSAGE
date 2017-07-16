using System;

namespace OpenZH.Data.Map
{
    [Flags]
    public enum BlendFlags : byte
    {
        None = 0,
        ReverseDirection = 1,

        /// <summary>
        /// Only ever found on horizontal blends on cells that additionally
        /// have a bottom-left or top-right blend. I don't know why it's necessary
        /// to call this out specifically, perhaps to do with D3D8 texture transforms.
        /// </summary>
        AlsoHasBottomLeftOrTopRightBlend = 2,

        ReverseDirection_AlsoHasBottomLeftOrTopRightBlend = ReverseDirection | AlsoHasBottomLeftOrTopRightBlend
    }
}
