using OpenZH.Data.Big;

namespace OpenZH.DataViewer.ViewModels
{
	public class ItemDetailViewModel : BaseViewModel
	{
		public BigArchiveEntry Item { get; set; }

		public ItemDetailViewModel(BigArchiveEntry item)
		{
			Title = item.FullName;
			Item = item;
		}
	}
}