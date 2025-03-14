
# nullable enable

using System.Numerics;
using OpenSage.Logic.Object;

namespace OpenSage.Graphics.Cameras;

public sealed class RotateCameraInfo
{
    public required double Duration { get; init; }
    public double CurrentTime { get; set; }
    public int StartTimeMultiplier { get; init; }
    public int EndTimeMultiplier { get; set; }
    public required double HoldTime { get; init; }
    public ParabolicEase Ease { get; init; } = ParabolicEase.CreateLinear();
    public bool TrackObject { get; init; }

    // In C++ this is a union
    public RotateCameraInfoTarget? Target { get; init; }
    public RotateCameraInfoAngle? Angle { get; init; }
}

public class RotateCameraInfoTarget
{
    public required GameObject? Object;
    public required Vector3 Position;
}

public class RotateCameraInfoAngle
{
    public required float Start;
    public required float End;
}
