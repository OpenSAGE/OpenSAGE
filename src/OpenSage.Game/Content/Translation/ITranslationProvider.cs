using System.Collections.Generic;

namespace OpenSage.Content.Translation
{
    public interface ITranslationProvider
    {
        string Name { get; }
        IReadOnlyCollection<string> Labels { get; }

        string GetString(string str);
    }
}
