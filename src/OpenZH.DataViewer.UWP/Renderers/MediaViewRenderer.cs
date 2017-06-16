using System.IO;
using Windows.Media.Core;
using Windows.UI.Xaml.Controls;
using OpenZH.DataViewer.Controls;
using OpenZH.DataViewer.UWP.Renderers;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(MediaView), typeof(MediaViewRenderer))]

namespace OpenZH.DataViewer.UWP.Renderers
{
    public class MediaViewRenderer : ViewRenderer<MediaView, MediaPlayerElement>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<MediaView> e)
        {
            base.OnElementChanged(e);

            if (e.NewElement != null)
            {
                if (Control == null)
                {
                    var control = new MediaPlayerElement();
                    control.AreTransportControlsEnabled = true;
                    SetNativeControl(control);
                }

                Control.Source = MediaSource.CreateFromStream(e.NewElement.CreateStream().AsRandomAccessStream(), "audio/wav");
            }
        }
    }
}
