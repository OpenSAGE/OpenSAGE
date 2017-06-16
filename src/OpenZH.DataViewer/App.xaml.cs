using OpenZH.Data.Big;
using OpenZH.DataViewer.ViewModels;
using OpenZH.DataViewer.Views;
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
		    var masterPage = new ItemsPage();
		    var detailPage = new ItemDetailPage();

		    masterPage.ItemSelected += (sender, e) =>
		    {
		        detailPage.BindingContext = new ItemDetailViewModel((BigArchiveEntry) e.SelectedItem) { Title = "Details" };
		    };

            Current.MainPage = new MasterDetailPage
		    {
                MasterBehavior = MasterBehavior.Split,

		        Master = new NavigationPage(masterPage) { Title = "Files" },
		        Detail = new NavigationPage(detailPage) { Title = "Details" }
            };
		}
	}
}
