using System.Collections.Generic;
using System.IO;

namespace OpenSage.Data.Str
{
    public class StrFile
    {
        public Dictionary<string, Dictionary<string, string>> Content { get; private set; }

        public static StrFile FromFileSystemEntry(FileSystemEntry entry)
        {
            var content = new Dictionary<string, Dictionary<string, string>>();

            using (var stream = entry.Open())
            using (var streamReader = new StreamReader(stream))
            {
                var blockType = "";
                var blockName = "";

                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine().Trim();
                    if (line.Length == 0
                        || line.StartsWith("//")
                        || line.StartsWith(";")
                        || line.StartsWith("END"))
                    {
                        continue;
                    }
                    else if (line.StartsWith("\""))
                    {
                        if (!content.ContainsKey(blockType))
                        {
                            content.Add(blockType, new Dictionary<string, string>());
                        }

                        var value = "";
                        do
                        {
                            value += line;
                            line = streamReader.ReadLine().Trim();
                        } while (!streamReader.EndOfStream && !line.StartsWith("END"));

                        content[blockType][blockName] = value;
                    }
                    else
                    {
                        var tokens = line.Split(':');
                        if (tokens.Length < 2)
                        {
                            throw new InvalidDataException();
                        }

                        blockType = tokens[0];
                        blockName = tokens[1];
                    }
                }
            }

            return new StrFile
            {
                Content = content
            };
        }
    }
}
