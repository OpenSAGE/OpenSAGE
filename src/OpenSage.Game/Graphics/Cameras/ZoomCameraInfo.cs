
namespace OpenSage.Graphics.Cameras;

// Original version is based on frames, we use milliseonds
// C++: TZoomCameraInfo
public class ZoomCameraInfo
{
    public required double Duration { get; init; }
    public double CurrentTime { get; set; }

    public required float StartZoom { get; init; }
    public required float EndZoom { get; init; }

    public int StartTimeMultiplier { get; init; }
    public int EndTimeMultiplier { get; set; }

    public ParabolicEase Ease { get; } = ParabolicEase.CreateLinear();
}
