using System;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using OpenSage.Data.Dds;
using OpenSage.DataViewer.Framework;
using OpenSage.DataViewer.Framework.BlockCompression;
using OpenSage.Utilities;
using PixelFormat = Veldrid.PixelFormat;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class DdsView : Splitter
    {
        private readonly DdsFile _ddsFile;
        private readonly ListBox _listBox;

        public DdsView(DdsFile ddsFile)
        {
            _ddsFile = ddsFile;

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
            public TextureMipMapData MipMap { get; }

            public DdsMipMapInfo(string name, int level, in TextureMipMapData mipMap)
            {
                Name = name;
                Level = level;
                MipMap = mipMap;
            }
        }

        private void OnSelectedValueChanged(object sender, EventArgs e)
        {
            var ddsMipMapInfo = (DdsMipMapInfo) _listBox.SelectedValue;

            var width = ddsMipMapInfo.MipMap.Width;
            var height = ddsMipMapInfo.MipMap.Height;

            byte[] unpackedData;
            switch (_ddsFile.PixelFormat)
            {
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC3_UNorm:
                    unpackedData = BlockCompressionUtility.Decompress(
                        _ddsFile.PixelFormat,
                        ddsMipMapInfo.MipMap.Data,
                        (int) ddsMipMapInfo.MipMap.RowPitch,
                        out _);
                    break;

                case PixelFormat.R8_G8_SNorm:
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

                case PixelFormat.R8_G8_B8_A8_UNorm:
                    {
                        unpackedData = ddsMipMapInfo.MipMap.Data;
                        break;
                    }

                default:
                    throw new NotSupportedException();
            }

            var bmpData = PngUtility.ConvertToPng(
                unpackedData,
                (int) width,
                (int) height);

            Panel2 = new ImageView
            {
                Style = "nearest-neighbor",
                Image = new Bitmap(bmpData)
            };
        }
    }
}
