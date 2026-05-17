using System;
using System.Text.Json.Serialization;

namespace OpenSage.Tools.ReplaySketch.Model;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
[JsonDerivedType(typeof(FixedTiming), "fixed")]
[JsonDerivedType(typeof(RandomTiming), "random")]
public abstract record TimingConfig
{
    public abstract uint Resolve(Random rng);
}

public record FixedTiming(uint Frames) : TimingConfig
{
    public override uint Resolve(Random rng) => Frames;
}

public record RandomTiming(uint MinFrames, uint MaxFrames) : TimingConfig
{
    public override uint Resolve(Random rng) =>
        (uint)rng.Next((int)MinFrames, (int)MaxFrames + 1);
}
