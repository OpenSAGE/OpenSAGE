namespace OpenSage.IO
{
    internal static class SkudefReader
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly record struct SkudefVersion(string LanguageName, int VersionMajor, int VersionMinor) : IComparable<SkudefVersion>
        {
            public static SkudefVersion Parse(string fileName)
            {
                var body = Path.GetFileNameWithoutExtension(fileName);
                body = body[(body.IndexOf('_') + 1)..];
                var lastUnderscore = body.LastIndexOf('_');
                string languageName;
                string[] versions;
                if (lastUnderscore != -1)
                {
                    languageName = body.Substring(0, lastUnderscore);
                    versions = body[(lastUnderscore + 1)..].Split('.');
                }
                else
                {
                    languageName = body;
                    versions = new[] { "0", "0" };
                }

                return new SkudefVersion
                {
                    LanguageName = languageName,
                    VersionMajor = int.Parse(versions[0]),
                    VersionMinor = int.Parse(versions[1]),
                };
            }

            public int CompareTo(SkudefVersion other)
            {
                var result = VersionMajor - other.VersionMajor;
                return result == 0
                    ? VersionMinor - other.VersionMinor
                    : result;
            }
        }

        public static void Read(string rootDirectory, Action<string> addBigArchive)
        {

            var skudefFiles = Directory.GetFiles(rootDirectory, "*.skudef", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive });

            var skudefFile = skudefFiles
                .OrderBy(SkudefVersion.Parse)
                .LastOrDefault(); // TODO: This is not the right logic. needs to take into account the language.

            if (skudefFile is not null)
            {
                Logger.Info($"Selected Skudef file {skudefFile}");
            }

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
            while (skudefReader.ReadLine() is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var spaceIndex = line.IndexOf(' ');
                var command = line.Substring(0, spaceIndex);
                var parameter = line.Substring(spaceIndex + 1);
                var fullPath = FileSystem.NormalizeFilePath(Path.Combine(skudefDirectory, parameter));

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
                            Read(Path.GetDirectoryName(fullPath)!, reader, addBigArchive);
                        }
                        break;
                }
            }
        }
    }
}
