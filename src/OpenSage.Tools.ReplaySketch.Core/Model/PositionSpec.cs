using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenSage.Tools.ReplaySketch.Model;

/// <summary>
/// Contextual data supplied by the exporter/validator so that a <see cref="PositionSpec"/>
/// can resolve itself into a world-space <see cref="Vector3"/>.
/// </summary>
public sealed class PositionContext
{
    /// <summary>Returns the world-space anchor for the given landmark type.</summary>
    public required Func<LandmarkType, Vector3> GetLandmark { get; init; }

    /// <summary>
    /// The radius (in world units) treated as "1 base width".
    /// Distance expressed as <c>N base widths</c> is multiplied by this value.
    /// </summary>
    public required float BaseRadiusWorldUnits { get; init; }

    /// <summary>Samples terrain height at the given world XY coordinates.</summary>
    public required Func<float, float, float> SampleHeight { get; init; }

    /// <summary>Min corner of the playable map extent (world units).</summary>
    public required Vector3 ExtentMin { get; init; }

    /// <summary>Max corner of the playable map extent (world units).</summary>
    public required Vector3 ExtentMax { get; init; }
}

/// <summary>
/// Discriminated union describing how a command position is specified.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(LandmarkRelativePosition), "landmarkRelative")]
[JsonDerivedType(typeof(NormalizedPosition), "normalized")]
public abstract record PositionSpec
{
    public abstract Vector3 Resolve(PositionContext ctx, Random rng);
}

/// <summary>
/// Raw 0–1 fractions of the playable map extents. Z is sampled from the height map.
/// </summary>
public record NormalizedPosition(float NormX, float NormY) : PositionSpec
{
    public override Vector3 Resolve(PositionContext ctx, Random rng)
    {
        var x = ctx.ExtentMin.X + NormX * (ctx.ExtentMax.X - ctx.ExtentMin.X);
        var y = ctx.ExtentMin.Y + NormY * (ctx.ExtentMax.Y - ctx.ExtentMin.Y);
        var z = ctx.SampleHeight(x, y);
        return new Vector3(x, y, z);
    }
}

/// <summary>
/// A polar offset from a named landmark. Distance is expressed in "base widths"
/// (multiples of <see cref="PositionContext.BaseRadiusWorldUnits"/>).
/// Angle is measured in degrees, clockwise from east (standard math convention).
/// </summary>
public record LandmarkRelativePosition(
    LandmarkType Landmark,
    float DistanceInBaseWidths,
    AngleConfig Angle) : PositionSpec
{
    public override Vector3 Resolve(PositionContext ctx, Random rng)
    {
        var anchor = ctx.GetLandmark(Landmark);
        var angleRad = Angle.Resolve(rng) * MathF.PI / 180f;
        var distance = DistanceInBaseWidths * ctx.BaseRadiusWorldUnits;
        var x = anchor.X + distance * MathF.Cos(angleRad);
        var y = anchor.Y + distance * MathF.Sin(angleRad);
        var z = ctx.SampleHeight(x, y);
        return new Vector3(x, y, z);
    }
}
