using OpenZH.DataViewer.Controls;
using OpenZH.DataViewer.ViewModels;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Views
{
	public partial class ItemPage : ContentPage
	{
        public ItemPage()
        {
            InitializeComponent();

            OnBindingContextChanged();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            ItemContentView.Content = CreateContentView(BindingContext);
        }

        private static View CreateContentView(object bindingContext)
        {
            switch (bindingContext)
            {
                case DdsArchiveEntryViewModel vm:
                    return new DdsView { OpenStream = vm.Item.Open };

                case WavArchiveEntryViewModel vm:
                    return new MediaView { CreateStream = vm.Item.Open };

                case W3dArchiveEntryViewModel vm:
                    return new Label { Text = "Select child item" };

                case W3dMeshItemViewModel vm:
                    return new W3dMeshView { Mesh = vm.Mesh };

                case null:
                    return new Label { Text = "No selection" };

                default:
                    return new Label { Text = "Unknown format" };
            }
        }
    }
}
