using System;
using System.Collections.Generic;
using System.Text;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Wav;

namespace OpenSage.DataViewer.UI.Viewers
{
    class WavView : Panel
    {
        WavFile _file;

        public WavView(FileSystemEntry entry)
        {
            _file = WavFile.FromFileSystemEntry(entry);
        }
    }
}
