using System;
using System.IO;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public class DdsView : Image
    {
        public Func<Stream> OpenStream { get; set; }

        public DdsView()
        {
            Aspect = Aspect.AspectFill;

            HorizontalOptions = LayoutOptions.CenterAndExpand;
            VerticalOptions = LayoutOptions.CenterAndExpand;
        }
    }
}
