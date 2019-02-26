using System.Collections.Generic;

namespace OpenSage.Content.Translation
{
    public interface ITranslationProvider
    {
        string Name { get; }
        string NameOverride { get; set; }
        IReadOnlyCollection<string> Labels { get; }

        string GetString(string str);
    }
}
