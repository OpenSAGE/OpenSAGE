using System.IO;
using OpenZH.Data.Big;
using OpenZH.Data.W3d;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public class PreviewView : ContentView
    {
        public static readonly BindableProperty ArchiveEntryProperty = BindableProperty.Create(
            nameof(ArchiveEntry), typeof(BigArchiveEntry), typeof(PreviewView), propertyChanged: OnArchiveEntryChanged);

        public BigArchiveEntry ArchiveEntry
        {
            get => (BigArchiveEntry) GetValue(ArchiveEntryProperty);
            set => SetValue(ArchiveEntryProperty, value);
        }

        private static void OnArchiveEntryChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var view = (PreviewView) bindable;

            if (newvalue == null)
            {
                view.Content = new Label { Text = "No selection" };
                return;
            }

            var archiveEntry = (BigArchiveEntry) newvalue;

            var fileExtension = Path.GetExtension(archiveEntry.FullName).ToLower();

            switch (fileExtension)
            {
                case ".w3d":
                    using (var stream = archiveEntry.Open())
                    using (var binaryReader = new BinaryReader(stream))
                        view.Content = new W3dView
                        {
                            W3dFile = W3dFile.Parse(binaryReader),
                            HorizontalOptions = LayoutOptions.FillAndExpand,
                            VerticalOptions = LayoutOptions.FillAndExpand
                        };
                    break;

                case ".dds":
                    view.Content = new DdsView(archiveEntry.Open);
                    break;

                case ".wav":
                    view.Content = new MediaView
                    {
                        CreateStream = archiveEntry.Open,
                        HorizontalOptions = LayoutOptions.FillAndExpand,
                        VerticalOptions = LayoutOptions.FillAndExpand
                    };
                    break;

                default:
                    view.Content = new Label { Text = "Unknown format" };
                    break;
            }
        }

        public PreviewView()
        {
            OnArchiveEntryChanged(this, null, null);
        }
    }
}
