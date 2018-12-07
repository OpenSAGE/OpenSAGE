namespace OpenSage.Graphics.Shaders
{
    public readonly struct Bool32
    {
        private readonly int _value;

        public Bool32(bool val)
        {
            _value = val ? 1 : 0;
        }

        public static implicit operator bool(Bool32 d)
        {
            return d._value == 1;
        }

        public static implicit operator Bool32(bool d)
        {
            return new Bool32(d);
        }
    }
}
