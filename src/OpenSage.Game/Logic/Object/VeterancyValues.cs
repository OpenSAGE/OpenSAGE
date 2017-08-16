using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class VeterancyValues
    {
        internal static VeterancyValues Parse(IniParser parser)
        {
            return new VeterancyValues
            {
                Regular = parser.ParseInteger(),
                Veteran = parser.ParseInteger(),
                Elite = parser.ParseInteger(),
                Heroic = parser.ParseInteger(),
            };
        }

        public int Regular { get; private set; }
        public int Veteran { get; private set; }
        public int Elite { get; private set; }
        public int Heroic { get; private set; }
    }
}
