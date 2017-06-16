using System;
using OpenZH.Data.Big;
using OpenZH.DataViewer.ViewModels;

using Xamarin.Forms;

namespace OpenZH.DataViewer.Views
{
	public partial class ItemsPage : ContentPage
	{
	    public event EventHandler<SelectedItemChangedEventArgs> ItemSelected;

		public ItemsPage()
		{
			InitializeComponent();

			BindingContext = new ItemsViewModel();
		}

		private void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
		{
			var item = args.SelectedItem as BigArchiveEntry;
			if (item == null)
				return;

		    ItemSelected?.Invoke(sender, args);
		}
	}
}
