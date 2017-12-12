using System;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using LL.Graphics3D.Util.BlockCompression;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.DataViewer.Framework;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class DdsView : Splitter
    {
        private readonly DdsFile _ddsFile;
        private readonly ListBox _listBox;

        public DdsView(FileSystemEntry entry)
        {
            _ddsFile = DdsFile.FromFileSystemEntry(entry);

            var mipMaps = _ddsFile.MipMaps
                .Select((x, i) => new DdsMipMapInfo($"MipMap {i}", i, x))
                .ToList();

            _listBox = new ListBox();
            _listBox.Width = 150;
            _listBox.ItemTextBinding = Binding.Property((DdsMipMapInfo v) => v.Name);
            _listBox.SelectedValueChanged += OnSelectedValueChanged;
            _listBox.DataStore = mipMaps;
            Panel1 = _listBox;

            _listBox.SelectedIndex = 0;
        }

        private sealed class DdsMipMapInfo
        {
            public string Name { get; }
            public int Level { get; }
            public DdsMipMap MipMap { get; }

            public DdsMipMapInfo(string name, int level, DdsMipMap mipMap)
            {
                Name = name;
                Level = level;
                MipMap = mipMap;
            }
        }

        private void OnSelectedValueChanged(object sender, EventArgs e)
        {
            var ddsMipMapInfo = (DdsMipMapInfo) _listBox.SelectedValue;

            var decompressedData = BlockCompressionUtility.Decompress(
                _ddsFile.PixelFormat,
                ddsMipMapInfo.MipMap.Data,
                (int) ddsMipMapInfo.MipMap.RowPitch,
                out _);

            var bmpData = BmpUtility.PrependBmpHeader(
                decompressedData,
                Math.Max(MipMapUtility.CalculateMipMapSize(ddsMipMapInfo.Level, (int) _ddsFile.Header.Width), 4),
                Math.Max(MipMapUtility.CalculateMipMapSize(ddsMipMapInfo.Level, (int) _ddsFile.Header.Height), 4));

            Panel2 = new ImageView
            {
                Style = "nearest-neighbor",
                Image = new Bitmap(bmpData)
            };
        }
    }
}
