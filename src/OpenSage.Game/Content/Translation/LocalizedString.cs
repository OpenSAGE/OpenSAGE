using System.Linq;

namespace OpenSage.Content.Translation
{
    public sealed class LocalizedString
    {
        public string Original { get; }
        public string Localized { get; }

        public LocalizedString(string original, string localized)
        {
            Original = original;
            Localized = localized;
        }

        public static LocalizedString Create(string original)
        {
            if (original is null)
            {
                return new LocalizedString(null, string.Empty);
            }
            var localized = original.Translate();
            return new LocalizedString(original, localized);
        }

        public static LocalizedString CreateApt(string original)
        {
            if (original is null)
            {
                return new LocalizedString(null, string.Empty);
            }
            var localized = original.Replace("$", "APT:") // All string values begin with $
                .Split('&').First() // Query strings after ampersand
                .Translate();
            return new LocalizedString(original, localized);
        }
    }
}
