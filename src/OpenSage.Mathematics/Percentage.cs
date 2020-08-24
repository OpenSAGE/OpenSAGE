namespace OpenSage.Mathematics
{
    public readonly struct Percentage
    {
        public bool IsZero => Value == 0;

        public Percentage(float value)
        {
            Value = value;
        }

        public float Value { get; }

        public static float operator *(float f, Percentage p) => f * p.Value;
        public static float operator *(Percentage p, float f) => p.Value * f;

        public static explicit operator float(Percentage p) => p.Value;

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
