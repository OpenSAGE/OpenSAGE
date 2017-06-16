using System;
using System.IO;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public class MediaView : View
    {
        public Func<Stream> CreateStream { get; set; }
    }
}
