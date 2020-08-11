namespace OpenSage.Mathematics
{
    public readonly struct Percentage
    {
        private readonly float _value;

        public bool IsZero => _value == 0;

        public Percentage(float value)
        {
            _value = value;
        }

        public static float operator *(float f, Percentage p) => f * p._value;
        public static float operator *(Percentage p, float f) => p._value * f;

        public static explicit operator float(Percentage p) => p._value;

        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
