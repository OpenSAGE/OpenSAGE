using System;
using System.Text.Json.Serialization;

namespace OpenSage.Tools.ReplaySketch.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(FixedAngle), "fixed")]
[JsonDerivedType(typeof(RandomAngle), "random")]
public abstract record AngleConfig
{
    public abstract float Resolve(Random rng);
}

public record FixedAngle(float Degrees) : AngleConfig
{
    public override float Resolve(Random rng) => Degrees;
}

public record RandomAngle(float MinDegrees, float MaxDegrees) : AngleConfig
{
    public override float Resolve(Random rng) =>
        MinDegrees + (float)rng.NextDouble() * (MaxDegrees - MinDegrees);
}
