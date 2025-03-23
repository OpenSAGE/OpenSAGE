namespace OpenSage.IO;

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
                versions = ["0", "0"];
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
            ? (TextReader)new StreamReader(skudefFile)
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
                    foreach (var bigPath in GetBigFiles(fullPath))
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


    /// <summary>
    /// Enumerates all .big files in the specified directory and its subdirectories, in the order they should be loaded.
    /// </summary>
    private static IEnumerable<string> GetBigFiles(string directory)
    {
        // The OSS version of Generals / ZH loads .big files with FindFirstFile / FindNextFile, which means they are returned
        // in whatever order the file system decides to return them. In practice on Windows & NTFS this is case-insensitive
        // alphabetical order. For cross-platform compatibility, we need to sort the files ourselves.
        // However, it seems that even that is not enough, as the Steam Workshop update made some changes which add other
        // sorting criteria. We don't actually know what those criteria are as the source code for the update is not available.
        // So this is a guess based on the behavior of the Steam version of the game.
        var entries = Directory
            .GetFileSystemEntries(directory)
            // In the CD / Origin release of ZH, .big files from Generals were included in the same directory as the other .big files.
            // In the current Steam version, they are in a subdirectory. We need to make sure that the Generals .big files are loaded first,
            // so that Zero Hour can override them.
            .OrderByDescending(entry => entry.Contains("ZH_Generals"))
            .ThenBy(entry =>
            {
                var fileName = Path.GetFileNameWithoutExtension(entry);
                if (fileName == null)
                {
                    return 0;
                }
                if (fileName.EndsWith("ZH", StringComparison.OrdinalIgnoreCase))
                {
                    // The Zero Hour .big files need to be loaded after the Generals .big files.
                    // This can be a problem with pre-Steam versions.
                    return 1;
                }
                if (fileName.StartsWith("Patch", StringComparison.OrdinalIgnoreCase))
                {
                    // The Steam Workshop update added a couple of new PatchX.big files, which need to be loaded after the main .big files.
                    // Older versions of the game also had patch .big files, but it seems they either didn't override the main .big files
                    // or they happened to accidentally be loaded in the right order thanks to their names (& NTFS).
                    return 2;
                }
                return 0;
            })
            // And finally we sort alphabetically & case-insensitively to match the Windows behavior.
            .ThenBy(entry => entry, StringComparer.OrdinalIgnoreCase);

        // The final order for Zero Hour should be:
        // 1. Generals .big files
        // 2. Generals Patch .big files
        // 3. Zero Hour .big files
        // 4. Zero Hour Patch .big files
        foreach (var entry in entries)
        {
            if (Directory.Exists(entry))
            {
                // Handle directories recursively to ensure the correct order in subdirectories
                foreach (var bigFile in GetBigFiles(entry))
                {
                    yield return bigFile;
                }
            }
            else if (Path.GetExtension(entry) == ".big")
            {
                yield return entry;
            }
        }
    }
}
