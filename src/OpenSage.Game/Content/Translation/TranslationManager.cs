#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using OpenSage.Content.Translation.Providers;
using OpenSage.IO;

namespace OpenSage.Content.Translation;

public static class TranslationManager
{
    private sealed class TranslationManagerInternal : ITranslationManager
    {
        private const string Missing = "MISSING: '{0}'";

        /// <summary>
        /// Providers that are specific to a language
        /// </summary>
        private readonly Dictionary<string, List<ITranslationProvider>> _translationProviders = [];

        /// <summary>
        /// Providers that are not language specific
        /// </summary>
        private readonly List<ITranslationProvider> _universalProviders = [];

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

        public void SetCultureFromLanguage(string language)
        {
            //TODO: just a hack for now
            var cultureString = language.ToLower() switch
            {
                "german" or "german2" => "de-DE",
                // Special case for Generals:
                // Generals does not distinct between Simplified Chinese (chinese_s) / Traditional Chinese (chinese_t)
                // It just assumes it's traditional
                "chinese" => "zh-Hant",
                _ => "en-US",
            };

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
                    result = [];
                    _translationProviders.Add(CurrentLanguage.Name, result);
                }
                return result;
            }
        }

        public event EventHandler? LanguageChanged;

        public IReadOnlyList<ITranslationProvider>? GetParticularProviders(string context)
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
            if (!_translationProviders.TryGetValue(provider.Name, out var providers))
            {
                providers = [provider];
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

        public void RegisterUniversalProvider(ITranslationProvider provider)
        {
            if (!_universalProviders.Contains(provider))
            {
                _universalProviders.Add(provider);
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

            foreach (var provider in DefaultProviders.Concat(_universalProviders))
            {
                if (provider.GetString(str) is string result)
                {
                    return result;
                }
            }

            return string.Format(Missing, str);
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
            if (providers is not null)
            {
                foreach (var provider in providers)
                {
                    if (!((result = provider.GetString(str)) is null))
                    {
                        return result;
                    }
                }
            }
            return string.Format(Missing, str);
        }
    }

    private static readonly Lazy<ITranslationManager> Lazy = new Lazy<ITranslationManager>(() => new TranslationManagerInternal());

    public static ITranslationManager Instance => Lazy.Value;

    public static void LoadGameStrings(FileSystem fileSystem, string language, IGameDefinition gameDefinition)
    {
        var path = gameDefinition.GetLocalizedStringsPath(language);

        if (LoadCsfFile(fileSystem, language, gameDefinition, $"{path}.csf"))
        {
            return;
        }

        if (LoadStrFile(fileSystem, language, $"{path}.str"))
        {
            return;
        }
    }

    public static bool LoadCsfFile(FileSystem fileSystem, string language, IGameDefinition gameDefinition, string path)
    {
        if (fileSystem.GetFile(path) is FileSystemEntry file)
        {
            using var stream = file.Open();
            Instance.SetCultureFromLanguage(language);
            Instance.RegisterProvider(new CsfTranslationProvider(stream, gameDefinition.Game));
            return true;
        }
        return false;
    }

    public static bool LoadStrFile(FileSystem fileSystem, string? language, string path)
    {
        if (fileSystem.GetFile(path) is FileSystemEntry file)
        {
            using var stream = file.Open();

            if (language != null)
            {
                Instance.SetCultureFromLanguage(language);
                Instance.RegisterProvider(new StrTranslationProvider(stream, language));
            }
            else
            {
                Instance.RegisterUniversalProvider(new StrTranslationProvider(stream, "default"));
            }

            return true;
        }

        return false;
    }
}
