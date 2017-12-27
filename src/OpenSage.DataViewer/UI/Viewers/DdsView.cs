using System;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenSage.LowLevel.Graphics3D.Util.BlockCompression;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Dds;
using OpenSage.DataViewer.Framework;
using OpenSage.LowLevel.Graphics3D;

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

            var width = Texture.CalculateMipMapSize(ddsMipMapInfo.Level, (int) _ddsFile.Header.Width);
            var height = Texture.CalculateMipMapSize(ddsMipMapInfo.Level, (int) _ddsFile.Header.Height);

            byte[] unpackedData;
            switch (_ddsFile.PixelFormat)
            {
                case LowLevel.Graphics3D.PixelFormat.Bc1:
                case LowLevel.Graphics3D.PixelFormat.Bc2:
                case LowLevel.Graphics3D.PixelFormat.Bc3:
                    unpackedData = BlockCompressionUtility.Decompress(
                        _ddsFile.PixelFormat,
                        ddsMipMapInfo.MipMap.Data,
                        (int) ddsMipMapInfo.MipMap.RowPitch,
                        out _);
                    width = Math.Max(width, 4);
                    height = Math.Max(height, 4);
                    break;

                case LowLevel.Graphics3D.PixelFormat.Rg8SNorm:
                    unpackedData = new byte[width * height * 4];
                    var unpackedDataIndex = 0;
                    for (var i = 0; i < ddsMipMapInfo.MipMap.Data.Length; i += 2)
                    {
                        unpackedData[unpackedDataIndex++] = ddsMipMapInfo.MipMap.Data[i + 0];
                        unpackedData[unpackedDataIndex++] = ddsMipMapInfo.MipMap.Data[i + 1];
                        unpackedData[unpackedDataIndex++] = 0;
                        unpackedData[unpackedDataIndex++] = 255;
                    }
                    break;

                default:
                    throw new NotSupportedException();
            }

            var bmpData = BmpUtility.PrependBmpHeader(
                unpackedData,
                width,
                height);

            Panel2 = new ImageView
            {
                Style = "nearest-neighbor",
                Image = new Bitmap(bmpData)
            };
        }
    }
}
