namespace OpenSage.Graphics.Shaders
{
    public readonly struct Bool4
    {
        private readonly int _value;

        public Bool4(bool val)
        {
            _value = val ? 1 : 0;
        }

        public static implicit operator bool(Bool4 d)
        {
            return d._value == 1;
        }

        public static implicit operator Bool4(bool d)
        {
            return new Bool4(d);
        }
    }
}
