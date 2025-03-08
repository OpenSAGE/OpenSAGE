
using System.Numerics;

namespace OpenSage.Graphics.Cameras;

// C++: TMoveAlongWaypointPathInfo
public sealed class MoveAlongWaypointPathInfo
{
    public const int MaxWaypoints = 25;

    // For some reason Generals uses structure-of-arrays pattern here
    // Every array - expect GroundHeight - should be of size MaxWaypoints + 2

    public int NumWaypoints { get; set; }

    public Vector3[] Waypoints { get; init; }

    /// <summary>
    /// Length of each segment
    /// </summary>
    public float[] WaySegLength { get; init; }

    /// <summary>
    /// The yaw angle of the camera, in radians
    /// </summary>
    public float[] CameraAngle { get; init; }

    public float[] CameraFXPitch { get; init; }

    public float[] CmaeraZoom { get; init; }

    /// <summary>
    /// Time speedup factor (TODO: what does that mean?)
    /// </summary>
    public int[] TimeMultiplier { get; init; }

    public float[] GroundHeight { get; init; }

    /// <summary>
    /// Num of ms to do this movement
    /// </summary>
    public int TotalTimeMilliseconds { get; set; }

    /// <summary>
    /// Time since start
    /// </summary>
    public int ElapsedTimeMilliseconds { get; set; }

    /// <summary>
    /// Total length of paths
    /// </summary>
    public float TotalDistance { get; set; }

    /// <summary>
    /// How far we are along the current segment
    /// </summary>
    public float CurSegDistance { get; set; }

    /// <summary>
    /// How long to wait (in milliseconds) after each waypoint. Also known as "stutter".
    /// In Generals this is in logic frames, not milliseconds.
    /// </summary>
    public int Shutter { get; set; }

    /// <summary>
    /// The index of the current waypoint.
    /// </summary>
    public int CurSegment { get; set; }

    /// <summary>
    /// How long we've been waiting at the current waypoint.
    /// </summary>
    public int CurShutter { get; set; }

    /// <summary>
    /// Number of frames to roll (TODO: what does that mean?)
    /// </summary>
    public int RollingAverageFrames { get; set; }

    public ParabolicEase Ease { get; } = ParabolicEase.CreateDefault();

    public MoveAlongWaypointPathInfo()
    {
        const int ArrayLength = MaxWaypoints + 2;
        Waypoints = new Vector3[ArrayLength];
        WaySegLength = new float[ArrayLength];
        CameraAngle = new float[ArrayLength];
        CameraFXPitch = new float[ArrayLength];
        CmaeraZoom = new float[ArrayLength];
        TimeMultiplier = new int[ArrayLength];
        // This is intentionally different
        GroundHeight = new float[MaxWaypoints + 1];
    }
}
