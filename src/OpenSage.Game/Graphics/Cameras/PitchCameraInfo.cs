
# nullable enable

namespace OpenSage.Graphics.Cameras;

public sealed class PitchCameraInfo
{
    public required double Duration { get; init; }
    public double CurrentTime { get; set; }
    public float Angle { get; set; }
    public required float StartPitch { get; init; }
    public required float EndPitch { get; init; }
    public int StartTimeMultiplier { get; init; }
    public int EndTimeMultiplier { get; set; }
    public ParabolicEase Ease { get; } = ParabolicEase.CreateLinear();
}
