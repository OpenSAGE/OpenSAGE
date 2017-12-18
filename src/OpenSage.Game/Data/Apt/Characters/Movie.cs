using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data.Utilities.Extensions;

namespace OpenSage.Data.Apt.Characters
{
    public class Movie : Character
    {
        public List<Frame> Frames   { get; private set; }
        public uint CharacterCount   { get; private set; }
        public uint ScreenWidth      { get; private set; }
        public uint ScreenHeight     { get; private set; }
        public uint ImportCount      { get; private set; }
        public uint ExportCount      { get; private set; }

        public Movie(BinaryReader br)
        {
            Frames = br.ReadListAtOffset<Frame>();
            var unknown = br.ReadUInt32();

            CharacterCount = br.ReadUInt32();
            var charOffset = br.ReadUInt32();

            ScreenWidth = br.ReadUInt32();
            ScreenHeight = br.ReadUInt32();
            var unknown2 = br.ReadUInt32();

            ImportCount = br.ReadUInt32();
            var importOffset = br.ReadUInt32();

            ExportCount = br.ReadUInt32();
            var exportOffset = br.ReadUInt32();
        }
    }
}
