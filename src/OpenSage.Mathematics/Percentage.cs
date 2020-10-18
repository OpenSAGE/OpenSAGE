using System;

namespace OpenSage.Mathematics
{
    public readonly struct Percentage
    {
        private readonly float _value;

        public bool IsZero => MathF.Abs(_value) < 0.0001f;

        public Percentage(float value)
        {
            _value = value;
        }

        public static float operator *(float f, Percentage p) => f * p._value;
        public static float operator *(Percentage p, float f) => p._value * f;
        public static float operator /(float f, Percentage p) => f / p._value;
        public static float operator /(Percentage p, float f) => p._value / f;
        public static Percentage operator *(Percentage p1, Percentage p2) => new Percentage(p1._value * p2._value);
        public static Percentage operator /(Percentage p1, Percentage p2) => new Percentage(p1._value / p2._value);
        public static bool operator <(float f, Percentage p) => f < p._value;
        public static bool operator >(float f, Percentage p) => f > p._value;
        public static explicit operator float(Percentage p) => p._value;

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
