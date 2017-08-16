using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Data.Csf;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class CsfFileContentViewModel : FileContentViewModel
    {
        public string Language { get; }
        public IReadOnlyList<CsfFileEntryViewModel> FileEntries { get; }

        public CsfFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            CsfFile csfFile;
            using (var fileStream = file.Open())
                csfFile = CsfFile.Parse(fileStream);

            Language = csfFile.Header.Language.ToString();
            FileEntries = csfFile.Labels.Select(x => new CsfFileEntryViewModel(x)).ToList();
        }
    }

    public sealed class CsfFileEntryViewModel
    {
        public string Name { get; }
        public string Value { get; }
        public string ExtraVaue { get; }

        public CsfFileEntryViewModel(CsfLabel label)
        {
            Name = label.Name;

            if (label.Strings.Length > 0)
            {
                var csfString = label.Strings[0];
                Value = csfString.Value;
                ExtraVaue = csfString.ExtraValue;
            }
        }
    }
}
