using OpenZH.DataViewer.ViewModels;
using OpenZH.DataViewer.Views;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace OpenZH.DataViewer
{
	public partial class App : Application
	{
        public App()
		{
			InitializeComponent();

			SetMainPage();
		}

		public static void SetMainPage()
		{
		    var masterPage = new ArchiveEntriesPage { Title = "Files" };
		    var detailPage = new ItemPage();

            var masterNavigationPage = new NavigationPage(masterPage)
            {
                Title = "Files"
            };

            async void onItemSelected(object sender, SelectedItemChangedEventArgs e)
            {
                var selectedItem = (ItemViewModel) e.SelectedItem;
                selectedItem.OnSelected();

                if (selectedItem.Children.Any())
                {
                    var childItemsViewModel = new ItemsViewModel();
                    childItemsViewModel.SetItems(selectedItem.Children);

                    var childItemsView = new ItemsPage
                    {
                        BindingContext = childItemsViewModel
                    };
                    childItemsView.ItemSelected += onItemSelected;

                    await masterNavigationPage.PushAsync(childItemsView, true);
                }

                detailPage.BindingContext = selectedItem;
            }

		    masterPage.ItemSelected += onItemSelected;

            Current.MainPage = new MasterDetailPage
		    {
		        Master = masterNavigationPage,
		        Detail = new NavigationPage(detailPage) { Title = "Details" }
            };
		}
	}
}
