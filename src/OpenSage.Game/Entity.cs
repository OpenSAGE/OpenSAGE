using System.Collections.Generic;

namespace OpenSage
{
    public abstract class Entity : DisposableBase
    {
        private readonly Dictionary<string, ModuleBase> _tagToModuleLookup;

        protected Entity()
        {
            _tagToModuleLookup = new Dictionary<string, ModuleBase>();
        }

        protected void AddModule(string tag, ModuleBase module)
        {
            module.Tag = tag;
            _tagToModuleLookup.Add(tag, module);
        }

        protected ModuleBase GetModuleByTag(string tag)
        {
            return _tagToModuleLookup[tag];
        }
    }
}
