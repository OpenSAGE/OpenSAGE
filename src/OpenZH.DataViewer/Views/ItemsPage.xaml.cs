using System;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Views
{
	public partial class ItemsPage : ContentPage
	{
	    public event EventHandler<SelectedItemChangedEventArgs> ItemSelected
        {
            add { ItemsListView.ItemSelected += value; }
            remove { ItemsListView.ItemSelected -= value; }
        }

		public ItemsPage()
		{
			InitializeComponent();
		}
	}
}
