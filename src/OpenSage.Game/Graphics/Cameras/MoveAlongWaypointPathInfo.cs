
using System.Numerics;
using OpenSage.Mathematics;

namespace OpenSage.Graphics.Cameras;

// C++: TMoveAlongWaypointPathInfo
public sealed class MoveAlongWaypointPathInfo
{
    public const int MaxWaypoints = 25;

    // Every array - expect GroundHeight - should be of size MaxWaypoints + 2

    /// <summary>
    /// The number of waypoints in the path.
    /// </summary>
    public int NumWaypoints { get; set; }

    /// <summary>
    /// The positions of the waypoints.
    /// </summary>
    public Vector3[] Waypoints { get; init; }

    /// <summary>
    /// The length of each segment between waypoints.
    /// </summary>
    public float[] WaySegLength { get; init; }

    /// <summary>
    /// The yaw angles of the camera at each waypoint, in radians.
    /// </summary>
    public float[] CameraAngle { get; init; }

    /// <summary>
    /// The time multiplier at each waypoint.
    /// TODO: This might be unnecessary for OpenSAGE.
    /// </summary>
    public int[] TimeMultiplier { get; init; }

    /// <summary>
    /// Ground height at each waypoint.
    /// </summary>
    public float[] GroundHeight { get; init; }

    /// <summary>
    /// Total time to complete the path.
    /// </summary>
    public double TotalTimeMilliseconds { get; set; }

    /// <summary>
    /// Elapsed time since the start of the path.
    /// </summary>
    public double ElapsedTimeMilliseconds { get; set; }

    /// <summary>
    /// Total length of the path in world units.
    /// </summary>
    public float TotalDistance { get; set; }

    /// <summary>
    /// How far we are along the current segment, in world units.
    /// </summary>
    public float CurSegDistance { get; set; }

    /// <summary>
    /// How long to wait (in milliseconds) before each camera position update.
    /// Also known as stutter. See RtsCameraController for more comprehensive explanation.
    /// </summary>
    public double Shutter { get; set; }

    /// <summary>
    /// The index of the current waypoint.
    /// </summary>
    public int CurSegment { get; set; }

    /// <summary>
    /// How long we've been waiting after the last camera position update.
    /// </summary>
    public double CurShutter { get; set; }

    /// <summary>
    /// Number of frames to roll (TODO: what does that mean?)
    /// </summary>
    public int RollingAverageFrames { get; set; }

    /// <summary>
    /// The easing function to use for the camera path.
    /// </summary>
    public ParabolicEase Ease { get; } = ParabolicEase.CreateLinear();

    public MoveAlongWaypointPathInfo()
    {
        const int ArrayLength = MaxWaypoints + 2;
        Waypoints = new Vector3[ArrayLength];
        WaySegLength = new float[ArrayLength];
        CameraAngle = new float[ArrayLength];
        TimeMultiplier = new int[ArrayLength];
        // This is intentionally different
        GroundHeight = new float[MaxWaypoints + 1];
    }

    /// <summary>
    /// Resets the mutable state of this object to its initial state.
    /// </summary>
    public void Reset()
    {
        ElapsedTimeMilliseconds = 0;
        CurSegment = 1;
        CurSegDistance = 0;
        CurShutter = 0;
    }

    /// <summary>
    /// Calculates a point on a quadratic bezier curve from three waypoints.
    /// The Z component of the result is always 0.
    /// </summary>
    /// <param name="centerIndex">The index of the center waypoint.</param>
    /// <param name="t">The progress along the curve, from 0 to 1.</param>
    /// <returns></returns>
    public Vector3 GetBezierPoint(int centerIndex, float t)
    {
        var previousWaypoint = Waypoints[centerIndex - 1];
        var currentWaypoint = Waypoints[centerIndex];
        var nextWaypoint = Waypoints[centerIndex + 1];

        var start = (previousWaypoint + currentWaypoint) / 2;
        var mid = currentWaypoint;
        var end = (currentWaypoint + nextWaypoint) / 2;

        return Vector3Utility.QuadBezier2D(start, mid, end, t);
    }
}
