using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenSage.Graphics.Animation
{
    public struct Keyframe
    {
        public TimeSpan Time;
        public KeyframeValue Value;

        public Keyframe(TimeSpan time, KeyframeValue value)
        {
            Time = time;
            Value = value;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct KeyframeValue
    {
        [FieldOffset(0)]
        public Quaternion Quaternion;

        [FieldOffset(0)]
        public float FloatValue;

        [FieldOffset(0)]
        public bool BoolValue;
    }
}
