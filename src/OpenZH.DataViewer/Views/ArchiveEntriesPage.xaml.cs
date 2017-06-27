using OpenZH.DataViewer.ViewModels;

namespace OpenZH.DataViewer.Views
{
	public partial class ArchiveEntriesPage : ItemsPage
	{
		public ArchiveEntriesPage()
		{
			InitializeComponent();

			BindingContext = new ArchiveEntriesViewModel();
		}
	}
}
