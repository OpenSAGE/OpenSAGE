using System;
using System.IO;
using System.Linq;

namespace OpenSage.Data
{
    internal static class SkudefReader
    {
        public static void Read(string rootDirectory, Action<string> addBigArchive)
        {
            var skudefFiles = Directory.GetFiles(rootDirectory, "*.skudef");
            var skudefFile = skudefFiles.LastOrDefault(); // TODO: This is not the right logic.

            // If no skudef (i.e. for pre-C&C3 games), use default one.
            using (var skudefFileContents = (skudefFile != null)
                ? (TextReader) new StreamReader(skudefFile)
                : new StringReader("add-bigs-recurse ."))
            {
                Read(rootDirectory, skudefFileContents, addBigArchive);
            }
        }

        private static void Read(string skudefDirectory, TextReader skudefReader, Action<string> addBigArchive)
        {
            string line;
            while ((line = skudefReader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var spaceIndex = line.IndexOf(' ');
                var command = line.Substring(0, spaceIndex);
                var parameter = line.Substring(spaceIndex + 1);
                var fullPath = Path.Combine(skudefDirectory, parameter);

                switch (command)
                {
                    case "add-big":
                        addBigArchive(fullPath);
                        break;

                    case "add-bigs-recurse":
                        foreach (var bigPath in Directory.GetFiles(fullPath, "*.big", SearchOption.AllDirectories))
                        {
                            addBigArchive(bigPath);
                        }
                        break;

                    case "add-config":
                        using (var reader = new StreamReader(fullPath))
                        {
                            Read(Path.GetDirectoryName(fullPath), reader, addBigArchive);
                        }
                        break;
                }
            }
        }
    }
}
