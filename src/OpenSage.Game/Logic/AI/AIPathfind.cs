using System;
using System.Numerics;
using System.Runtime.CompilerServices;

#nullable enable

namespace OpenSage.Logic.AI;

public partial class AIPathfind
{
    /// <summary>
    /// How close is close enough when moving.
    /// </summary>
    public const float PathfindCloseEnough = 1.0f;
    public const float PathfindCellSize = 10;
    public const float PathfindCellSizeF = 10.0f;
    public const int PathfindQueueLength = 512;

    /// <summary>
    /// Fits in 4 bits for now
    /// </summary>
    public const int MaxWallPieces = 128;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsReallyClose(Vector3 a, Vector3 b)
    {
	    const float closeEnough = 0.1f;
	    return 
		    MathF.Abs(a.X - b.X) <= closeEnough &&
		    MathF.Abs(a.Y - b.Y) <= closeEnough &&
            MathF.Abs(a.Z - b.Z) <= closeEnough;
    }
}
