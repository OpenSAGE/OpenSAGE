#nullable enable

using System.Linq;

namespace OpenSage.Content.Translation
{
    public sealed class LocalizedString
    {
        public string Original { get; }
        private string? _localized;

        public LocalizedString(string original)
        {
            Original = original;
        }

        public string Localize(params object[] args)
        {
            _localized ??= Original?.Translate() ?? string.Empty;
            return args.Length > 0 ? SprintfNET.StringFormatter.PrintF(_localized, args) : _localized;
        }
        public static LocalizedString CreateApt(string original)
        {
            var trimmed = original.Replace("$", "APT:") // All string values begin with $
                .Split('&').First(); // Query strings after ampersand

            return new LocalizedString(trimmed);
        }
    }
}
