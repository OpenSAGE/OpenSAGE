using System;
using System.Globalization;

namespace OpenSage.Content.Translation
{
    public interface ITranslationManager : ITranslationProvider
    {
        CultureInfo CurrentLanguage { get; set; }

        event EventHandler LanguageChanged;

        void RegisterProvider(ITranslationProvider provider);

        string GetParticularString(string context, string str);
    }
}
