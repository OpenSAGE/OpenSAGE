using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using OpenZH.Data.Big;
using OpenZH.DataViewer.Helpers;
using Plugin.FilePicker;
using Xamarin.Forms;

namespace OpenZH.DataViewer.ViewModels
{
	public class ItemsViewModel : BaseViewModel
	{
		public ObservableRangeCollection<BigArchiveEntry> Items { get; }
		public Command OpenFileCommand { get; }

		public ItemsViewModel()
		{
			Title = "Browse";
			Items = new ObservableRangeCollection<BigArchiveEntry>();
		    OpenFileCommand = new Command(async () => await OpenFileAsync());
		}

		private async Task OpenFileAsync()
		{
			if (IsBusy)
				return;

			IsBusy = true;

            try
			{
				Items.Clear();

			    var fileData = await CrossFilePicker.Current.PickFile();

			    var memoryStream = new MemoryStream(fileData.DataArray);
			    var bigArchive = new BigArchive(memoryStream);

			    Items.ReplaceRange(bigArchive.Entries);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}