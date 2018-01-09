using System;
using System.IO;
using System.Text;

namespace OpenSage.Data.StreamFS
{
    public sealed class GameStream
    {
        public FileSystemEntry ManifestFileEntry { get; }
        public ManifestFile ManifestFile { get; }

        public GameStream(FileSystemEntry manifestFileEntry)
        {
            ManifestFileEntry = manifestFileEntry;
            ManifestFile = ManifestFile.FromFileSystemEntry(manifestFileEntry);

            ParseStreamFile(".bin", reader =>
            {
                foreach (var asset in ManifestFile.Assets)
                {
                    asset.InstanceData = reader.ReadBytes((int) asset.Header.InstanceDataSize);

                    if (asset.Name.StartsWith("GameMap:"))
                    {

                    }
                }
            });

            //ParseStreamFile(".relo", reader =>
            //{
            //    foreach (var asset in ManifestFile.Assets)
            //    {
            //        if (asset.Header.RelocationDataSize != 0)
            //        {
            //            uint reloValue;
            //            do
            //            {
            //                reloValue = reader.ReadUInt32();

            //                //asset.InstanceData[reloValue] += asset.InstanceData[0];
            //            } while (reloValue != 0xFFFFFFFF);
            //        }
            //    }
            //});
        }

        private void ParseStreamFile(string extension, Action<BinaryReader> callback)
        {
            var streamFilePath = Path.ChangeExtension(ManifestFileEntry.FilePath, extension);
            var streamFileEntry = ManifestFileEntry.FileSystem.GetFile(streamFilePath);
            if (streamFileEntry == null)
            {
                return;
            }

            using (var binaryStream = streamFileEntry.Open())
            using (var reader = new BinaryReader(binaryStream, Encoding.ASCII, true))
            {
                var checksum = reader.ReadUInt32();
                if (checksum != ManifestFile.Header.StreamChecksum)
                {
                    throw new InvalidDataException();
                }

                callback(reader);
            }
        }
    }
}
