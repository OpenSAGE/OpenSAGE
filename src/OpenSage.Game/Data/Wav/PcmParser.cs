using System.IO;

namespace OpenSage.Data.Wav
{
    internal class PcmParser : WavParser
    {
        public override byte[] Parse(BinaryReader reader, int size, WaveFormat format)
        {
            return reader.ReadBytes(size);
        }
    }
}
