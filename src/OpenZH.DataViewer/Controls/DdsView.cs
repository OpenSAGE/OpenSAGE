using System;
using System.IO;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Pfim;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public class DdsView : Image
    {
        public DdsView(Func<Stream> openStream)
        {
            Aspect = Aspect.AspectFill;

            HorizontalOptions = LayoutOptions.CenterAndExpand;
            VerticalOptions = LayoutOptions.CenterAndExpand;

            SetImage(openStream);
        }

#if WINDOWS_UWP
        private async void SetImage(Func<Stream> openStream)
        {
            using (var stream = openStream())
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
                var bitmapEncoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, outputStream);

                bitmapEncoder.SetPixelData(
                    BitmapPixelFormat.Rgba8, BitmapAlphaMode.Straight, 
                    (uint) dds.Width, (uint) dds.Height, 
                    96, 96,
                    data);

                await bitmapEncoder.FlushAsync();

                Source = ImageSource.FromStream(() => outputStream.AsStreamForRead());
            }
        }
#endif
    }
}
