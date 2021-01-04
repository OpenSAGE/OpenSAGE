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

        public static LocalizedString Apt(string original)
        {
            var localized = original.Replace("$", "APT:") // All string values begin with $
                .Split('&').First() // Query strings after ampersand
                .Translate();
            return new LocalizedString(original, localized);
        }
    }
}
