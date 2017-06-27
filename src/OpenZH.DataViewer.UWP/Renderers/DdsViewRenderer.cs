using OpenZH.DataViewer.Controls;
using Pfim;
using System;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(DdsView), typeof(OpenZH.DataViewer.UWP.Renderers.DdsViewRenderer))]

namespace OpenZH.DataViewer.UWP.Renderers
{
    public class DdsViewRenderer : ViewRenderer<DdsView, Image>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<DdsView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                SetNativeControl(new Image());
            }

            if (e.NewElement != null)
            {
                using (var stream = e.NewElement.OpenStream())
                {
                    var dds = Dds.Create(stream);

                    byte[] data;
                    switch (dds.Format)
                    {
                        case ImageFormat.Rgb24:
                            data = new byte[(dds.Data.Length / 3) * 4];
                            var outputIndex = 0;
                            for (var i = 0; i < dds.Data.Length; i += 3)
                            {
                                data[outputIndex + 0] = dds.Data[i + 0];
                                data[outputIndex + 1] = dds.Data[i + 1];
                                data[outputIndex + 2] = dds.Data[i + 2];
                                data[outputIndex + 3] = 1;

                                outputIndex += 4;
                            }
                            break;

                        case ImageFormat.Rgba32:
                            data = dds.Data;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var outputStream = new InMemoryRandomAccessStream();
                    //var bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, outputStream);

                    //bitmapEncoder.SetPixelData(
                    //    BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight,
                    //    (uint)dds.Width, (uint)dds.Height,
                    //    96, 96,
                    //    data);

                    //bitmapEncoder.FlushAsync().AsTask().Wait();

                    //Control.Source = ImageSource.FromStream(() => outputStream.AsStreamForRead());
                }
            }
        }
    }
}
