using System.Linq;
using System.Threading.Tasks;
using OpenZH.Data;
using OpenZH.DataViewer.Services;
using Xamarin.Forms;

namespace OpenZH.DataViewer.ViewModels
{
    public class ArchiveEntriesViewModel : ItemsViewModel
    {
		public ArchiveEntriesViewModel()
		{
            OpenFileAsync();
		}

        private async Task OpenFileAsync()
        {
            var rootDirectory = await DependencyService.Get<IFilePicker>().PickFolder();

            await Task.Run(() =>
            {
                // TODO: Dispose this.
                var fileSystem = new FileSystem(rootDirectory);

                SetItems(fileSystem.Files.Select(x => ArchiveEntryViewModel.Create(x)));
            });
        }
	}
}