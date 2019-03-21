using System;
using System.Collections.Generic;
using OpenSage.Data.Apt;
using OpenSage.Data.Apt.Characters;
using OpenSage.Data.Apt.FrameItems;
using OpenSage.Tools.AptEditor.Apt.Writer;

namespace OpenSage.Tools.AptEditor.Apt {
    public sealed class AptDataDump
    {
        public byte[] AptData;
        public byte[] ConstantData;
        public byte[] ImageMapData;
        public Dictionary<string, byte[]> GeometryData;
        public List<AptDataDump> ReferencedAptData;
    }

    public class AptObjectsManager
    {
        public AptFile AptFile { get; private set; }
        public AptObjectsManager(AptFile aptFile)
        {
            AptFile = aptFile;
        }

        public void AddCharacter(Character character)
        {
            AptFile.Movie.Characters.Add(character);
        }

        public void AddFrame(Frame frame)
        {
            AptFile.Movie.Frames.Add(frame);
        }

        public AptDataDump GetAptDataDump()
        {
            var dataDump = new AptDataDump();
            var aptData = AptDataWriter.Write(AptFile.Movie);
            dataDump.AptData = aptData.Data;
            dataDump.ConstantData = ConstantDataWriter.Write(aptData.EntryOffset, AptFile.Constants);
            dataDump.ImageMapData = ImageMapWriter.Write(AptFile.ImageMap);
            return dataDump;
        }

    }

}