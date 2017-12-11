using Eto.Drawing;
using Eto.Forms;
using LL.Graphics3D.Util.BlockCompression;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.DataViewer.Framework;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class DdsView : ImageView
    {
        public DdsView(FileSystemEntry entry)
        {
            var ddsFile = DdsFile.FromFileSystemEntry(entry);

            var decompressedData = BlockCompressionUtility.Decompress(
                ddsFile.PixelFormat,
                ddsFile.MipMaps[0].Data,
                (int) ddsFile.MipMaps[0].RowPitch,
                out _);

            var bmpData = BmpUtility.PrependBmpHeader(
                decompressedData,
                (int) ddsFile.Header.Width,
                (int) ddsFile.Header.Height);

            Image = new Bitmap(bmpData);
        }
    }
}
