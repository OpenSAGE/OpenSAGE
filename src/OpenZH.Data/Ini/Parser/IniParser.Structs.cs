using OpenZH.Data.Wnd;

namespace OpenZH.Data.Ini.Parser
{
    partial class IniParser
    {
        public WndFont ParseFont()
        {
            return new WndFont
            {
                Name = ParseAsciiString(),
                Size = ParseInteger(),
                Bold = ParseBoolean()
            };
        }

        public ArmorValue ParseArmorValue()
        {
            return new ArmorValue
            {
                DamageType = ParseDamageType(),
                Percent = ParsePercentage()
            };
        }
    }
}
