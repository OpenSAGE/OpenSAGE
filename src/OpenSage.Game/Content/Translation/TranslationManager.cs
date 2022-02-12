using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using OpenSage.Content.Translation.Providers;
using OpenSage.IO;
using OpenSage.Utilities;

namespace OpenSage.Content.Translation
{
    public static class TranslationManager
    {
        private sealed class TranslationManagerInternal : ITranslationManager
        {
            private const string _missing = "MISSING: '{0}'";

            private readonly Dictionary<string, List<ITranslationProvider>> _translationProviders = new();

            public string Name => nameof(TranslationManager);
            public string NameOverride { get => Name; set { } } // do nothing on set
            public CultureInfo CurrentLanguage
            {
                get => CultureInfo.CurrentCulture;
                set
                {
                    if (Equals(CultureInfo.CurrentCulture, value))
                    {
                        return;
                    }

                    CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture = value;
                    OnLanguageChanged();
                }
            }

            public void SetCultureFromLanguage(GameLanguage gameLanguage)
            {
                //TODO: just a hack for now
                string cultureString;
                switch (gameLanguage)
                {
                    case Utilities.GameLanguage.German:
                        cultureString = "de-DE";
                        break;
                    // Special case for Generals: 
                    // Generals does not distinct between Simplified Chinese (chinese_s) / Traditional Chinese (chinese_t)
                    // It just assumes it's traditional
                    case Utilities.GameLanguage.Chinese:
                        cultureString = "zh-Hant";
                        break;
                    case Utilities.GameLanguage.English:
                    default:
                        cultureString = "en-US";
                        break;
                }
                CurrentLanguage = new CultureInfo(cultureString);
            }

            public IReadOnlyCollection<string> Labels
            {
                get
                {
                    var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var translationProviders in _translationProviders.Values)
                    {
                        foreach (var translationProvider in translationProviders)
                        {
                            foreach (var label in translationProvider.Labels)
                            {
                                if (!result.Contains(label))
                                {
                                    result.Add(label);
                                }
                            }
                        }
                    }
                    return result;
                }
            }

            public IReadOnlyList<ITranslationProvider> DefaultProviders
            {
                get
                {
                    if (!_translationProviders.TryGetValue(CurrentLanguage.Name, out var result))
                    {
                        // we don't want to check for null for the current language
                        result = new List<ITranslationProvider>();
                        _translationProviders.Add(CurrentLanguage.Name, result);
                    }
                    return result;
                }
            }

            public event EventHandler LanguageChanged;

            public IReadOnlyList<ITranslationProvider> GetParticularProviders(string context)
            {
                Debug.Assert(context is not null, $"{nameof(context)} is null");
                _translationProviders.TryGetValue(context, out var result);
                return result;
            }

            private void OnLanguageChanged()
            {
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }

            public void RegisterProvider(ITranslationProvider provider, bool shouldNotifyLanguageChange = true)
            {
                if (provider is null)
                {
                    throw new ArgumentNullException(nameof(provider));
                }

                if (!_translationProviders.TryGetValue(provider.Name, out var providers))
                {
                    providers = new List<ITranslationProvider> { provider };
                    _translationProviders.Add(provider.Name, providers);
                }
                else
                {
                    if (!providers.Contains(provider))
                    {
                        providers.Add(provider);
                    }
                }

                if (shouldNotifyLanguageChange && Equals(providers, DefaultProviders))
                {
                    OnLanguageChanged();
                }
            }

            public void UnregisterProvider(ITranslationProvider provider, bool shouldNotifyLanguageChange = true)
            {
                if (!_translationProviders.TryGetValue(provider.Name, out _))
                {
                    return;
                }

                if (_translationProviders.Remove(provider.Name) && shouldNotifyLanguageChange)
                {
                    OnLanguageChanged();
                }
            }

            public string GetString(string str)
            {
                // Do we want to do this here or in the caller?
                if (string.IsNullOrEmpty(str))
                {
                    return string.Empty;
                }

                // If the string does not contain a Category/Label separator we return the string
                if (!str.Contains(':'))
                {
                    return str;
                }

                string result;
                foreach (var provider in DefaultProviders)
                {
                    if (!((result = provider.GetString(str)) is null))
                    {
                        return result;
                    }
                }

                return string.Format(_missing, str);
            }

            public string GetParticularString(string context, string str)
            {
                // Do we want to do this here or in the caller?
                if (string.IsNullOrEmpty(context))
                {
                    return GetString(str);
                }
                // Do we want to do this here or in the caller?
                if (string.IsNullOrEmpty(str))
                {
                    return string.Empty;
                }
                // If the string does not contain a Category/Label separator we return the string
                if (!str.Contains(':'))
                {
                    return str;
                }
                string result;
                var providers = GetParticularProviders(context);
                if (!(providers is null))
                {
                    foreach (var provider in providers)
                    {
                        if (!((result = provider.GetString(str)) is null))
                        {
                            return result;
                        }
                    }
                }
                return string.Format(_missing, str);
            }
        }

        private static readonly Lazy<ITranslationManager> _lazy = new Lazy<ITranslationManager>(() => new TranslationManagerInternal());

        public static ITranslationManager Instance => _lazy.Value;

        public static void LoadGameStrings(FileSystem fileSystem, GameLanguage language, IGameDefinition gameDefinition)
        {
            var path = gameDefinition.GetLocalizedStringsPath(language.ToString());

            FileSystemEntry file;
            if (!((file = fileSystem.GetFile($"{path}.csf")) is null))
            {
                using var stream = file.Open();
                Instance.SetCultureFromLanguage(language);
                Instance.RegisterProvider(new CsfTranslationProvider(stream, gameDefinition.Game));

                return;
            }

            if (!((file = fileSystem.GetFile($"{path}.str")) is null))
            {
                using var stream = file.Open();
                Instance.SetCultureFromLanguage(language);
                Instance.RegisterProvider(new StrTranslationProvider(stream, language.ToString()));

                return;
            }
        }
    }
}
