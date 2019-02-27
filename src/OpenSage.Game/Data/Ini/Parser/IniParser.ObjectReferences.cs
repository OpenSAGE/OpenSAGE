using System.Collections.Generic;

namespace OpenSage.Data.Ini.Parser
{
    internal partial class IniParser
    {
        public Armor ParseArmorReference()
        {
            var name = ParseString();
            return _dataContext.GetArmor(name);
        }

        public CommandSet ParseCommandSetReference()
        {
            var name = ParseString();
            return _dataContext.GetCommandSet(name);
        }

        public Locomotor[] ParseLocomotorReferenceArray()
        {
            var names = ParseAssetReferenceArray();
            var result = new List<Locomotor>();
            foreach (var name in names)
            {
                result.Add(_dataContext.GetLocomotor(name));
            }
            return result.ToArray();
        }
    }
}
