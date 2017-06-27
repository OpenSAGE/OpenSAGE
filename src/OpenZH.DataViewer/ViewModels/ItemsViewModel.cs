using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OpenZH.Data.Big;
using OpenZH.DataViewer.Services;
using Xamarin.Forms;

namespace OpenZH.DataViewer.ViewModels
{
    public class ArchiveEntriesViewModel : ItemsViewModel
    {
		public Command OpenFileCommand { get; }

		public ArchiveEntriesViewModel()
		{
		    OpenFileCommand = new Command(async () => await OpenFileAsync());
		}

		private async Task OpenFileAsync()
		{
            try
			{
			    var fileData = await DependencyService.Get<IFilePicker>().PickFile();

			    var memoryStream = new MemoryStream(fileData);
			    var bigArchive = new BigArchive(memoryStream);

                SetItems(bigArchive.Entries.Select(x => ArchiveEntryViewModel.Create(x)));
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
		}
	}
}