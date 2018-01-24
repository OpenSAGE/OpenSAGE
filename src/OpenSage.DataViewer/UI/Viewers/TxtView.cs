using System.IO;
using Eto.Forms;
using OpenSage.Data;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class TxtView : Scrollable
    {
        public TxtView(FileSystemEntry entry)
        {
            var textBox = new TextBox { ReadOnly = true };
            
            using (var fileStream = entry.Open())
            using (var streamReader = new StreamReader(fileStream))
                textBox.Text = streamReader.ReadToEnd();

            Content = textBox;
        }
    }
}
