using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenSage.Content.Translation
{
    public interface ITranslationManager : ITranslationProvider
    {
        CultureInfo CurrentLanguage { get; set; }
        IReadOnlyList<ITranslationProvider> DefaultProviders { get; }

        event EventHandler LanguageChanged;

        void SetCultureFromLanguage(Utilities.GameLanguage gameLanguage);

        void RegisterProvider(ITranslationProvider provider, bool shouldNotifyLanguageChange = true);

        void UnregisterProvider(ITranslationProvider provider, bool shouldNotifyLanguageChange = true);

        IReadOnlyList<ITranslationProvider> GetParticularProviders(string context);

        string GetParticularString(string context, string str);
    }
}
