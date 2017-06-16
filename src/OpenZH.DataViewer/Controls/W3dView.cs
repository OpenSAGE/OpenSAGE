using OpenZH.Data.W3d;
using Xamarin.Forms;

namespace OpenZH.DataViewer.Controls
{
    public class W3dView : View
    {
        public static readonly BindableProperty W3dFileProperty = BindableProperty.Create(
            nameof(W3dFile), typeof(W3dFile), typeof(W3dView));

        public W3dFile W3dFile
        {
            get { return (W3dFile) GetValue(W3dFileProperty); }
            set { SetValue(W3dFileProperty, value); }
        }
    }
}
