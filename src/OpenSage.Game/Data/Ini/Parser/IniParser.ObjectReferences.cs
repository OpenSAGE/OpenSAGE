namespace OpenSage.Data.Ini.Parser
{
    internal partial class IniParser
    {
        public Armor ParseArmorReference()
        {
            var name = ParseString();
            return _dataContext.GetArmor(name);
        }
    }
}
