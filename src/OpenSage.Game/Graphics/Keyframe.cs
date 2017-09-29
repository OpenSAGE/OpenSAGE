using System;
using System.Numerics;

namespace OpenSage.Graphics.Animation
{
    public abstract class Keyframe
    {
        public TimeSpan Time { get; }

        protected Keyframe(TimeSpan time)
        {
            Time = time;
        }

        public abstract void Apply(ref Transform transform, Action<bool> setVisibility);
    }

    public sealed class QuaternionKeyframe : Keyframe
    {
        public Quaternion Rotation { get; }

        public QuaternionKeyframe(TimeSpan time, Quaternion rotation)
            : base(time)
        {
            Rotation = rotation;
        }

        public override void Apply(ref Transform transform, Action<bool> setVisibility)
        {
            transform.Rotation = Rotation;
        }
    }

    public sealed class TranslationXKeyframe : Keyframe
    {
        public float Value { get; }

        public TranslationXKeyframe(TimeSpan time, float value)
            : base(time)
        {
            Value = value;
        }

        public override void Apply(ref Transform transform, Action<bool> setVisibility)
        {
            transform.Translation.X = Value;
        }
    }

    public sealed class TranslationYKeyframe : Keyframe
    {
        public float Value { get; }

        public TranslationYKeyframe(TimeSpan time, float value)
            : base(time)
        {
            Value = value;
        }

        public override void Apply(ref Transform transform, Action<bool> setVisibility)
        {
            transform.Translation.Y = Value;
        }
    }

    public sealed class TranslationZKeyframe : Keyframe
    {
        public float Value { get; }

        public TranslationZKeyframe(TimeSpan time, float value)
            : base(time)
        {
            Value = value;
        }

        public override void Apply(ref Transform transform, Action<bool> setVisibility)
        {
            transform.Translation.Z = Value;
        }
    }

    public sealed class VisibilityKeyframe : Keyframe
    {
        public bool Value { get; }

        public VisibilityKeyframe(TimeSpan time, bool value)
            : base(time)
        {
            Value = value;
        }

        public override void Apply(ref Transform transform, Action<bool> setVisibility)
        {
            setVisibility(Value);
        }
    }
}
