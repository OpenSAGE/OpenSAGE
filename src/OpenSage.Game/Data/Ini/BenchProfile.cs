using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class BenchProfile
    {
        internal static BenchProfile Parse(IniParser parser)
        {
            var result = new BenchProfile
            {
                CpuType = parser.ParseEnum<CpuType>(),
                MHz = parser.ParseInteger(),

                Unknown1 = parser.ParseFloat(),
                Unknown2 = parser.ParseFloat(),
                Unknown3 = parser.ParseFloat()
            };

            return result;
        }

        public CpuType CpuType { get; private set; }
        public int MHz { get; private set; }

        public float Unknown1 { get; private set; }
        public float Unknown2 { get; private set; }
        public float Unknown3 { get; private set; }
    }

    public enum CpuType
    {
        [IniEnum("P3")]
        P3,

        [IniEnum("P4")]
        P4,

        [IniEnum("K7")]
        K7
    }
}
