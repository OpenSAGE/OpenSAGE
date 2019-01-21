using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenSage.Content.Translation
{
    public abstract class ATranslationProviderBase : ITranslationProvider
    {
        private string _nameOverride;

        public abstract string Name { get; }
        public string NameOverride
        {
            get => _nameOverride;
            set
            {
                var wasRegistered = false;
                var providers = TranslationManager.Instance.GetParticularProviders(Name);
                if (providers?.Contains(this) == true)
                {
                    wasRegistered = true;
                    TranslationManager.Instance.UnregisterProvider(this, false);
                }
                _nameOverride = value;
                if (wasRegistered)
                {
                    TranslationManager.Instance.RegisterProvider(this);
                }
            }
        }
        public abstract IReadOnlyCollection<string> Labels { get; }

        public abstract string GetString(string str);
    }
}
