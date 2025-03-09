
namespace OpenSage.Graphics.Cameras;

// Original version is based on frames, we use milliseonds
// C++: TZoomCameraInfo
public class ZoomCameraInfo
{
    public double Duration { get; init; }
    public double CurrentTime { get; set; }

    public float StartZoom { get; init; }
    public float EndZoom { get; init; }

    public int StartTimeMultiplier { get; init; }
    public int EndTimeMultiplier { get; init; }

    public ParabolicEase Ease { get; } = ParabolicEase.CreateDefault();
}
