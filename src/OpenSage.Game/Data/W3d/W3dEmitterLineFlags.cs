using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dEmitterLineFlags : uint
    {
        None = 0,

        /// <summary>
        /// Merge intersections
        /// </summary>
        MergeIntersections = 0x00000001,

        /// <summary>
        /// Freeze random (note: offsets are in camera space)
        /// </summary>
        FreezeRandom,

        /// <summary>
        /// Disable sorting (even if shader has alpha-blending)
        /// </summary>
        DisableSorting,

        /// <summary>
        /// Draw end caps on the line
        /// </summary>
        EndCaps,

        /// <summary>
        /// Entire line uses one row of texture (constant V)
        /// </summary>
        UniformWidthTextureMap = 0,

        /// <summary>
        /// Entire line uses one row of texture stretched length-wise
        /// </summary>
        UniformLengthTextureMap = 0x00000001 << 24,

        /// <summary>
        /// Tiled continuously over line
        /// </summary>
        TiledTextureMap = 0x00000002 << 24,
    }
}
