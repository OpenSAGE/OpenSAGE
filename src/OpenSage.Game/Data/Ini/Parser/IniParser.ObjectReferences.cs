namespace OpenSage.Data.Ini.Parser
{
    partial class IniParser
    {
        public Armor ParseArmorReference()
        {
            var name = ParseString();
            return _dataContext.GetArmor(name);
        }
    }
}
