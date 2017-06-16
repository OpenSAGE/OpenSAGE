namespace OpenZH.DataViewer.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new OpenZH.DataViewer.App());
        }
    }
}
