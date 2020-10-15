using System;
using System.IO;
using Xunit;
using OpenSage.FileFormats.Big;

namespace OpenSage.Tests.Data.Big
{
    public class BigAppendEntry
    {
        private static readonly string RootFolder = Path.Combine(Environment.CurrentDirectory, "Data", "Big", "Assets");

        [Fact]
        public void AppendEntry()
        {
            var fullPath = Path.Combine(RootFolder, "test.big");
            var tmpPath = Path.Combine(RootFolder, "tmp.big");
            File.Copy(fullPath, tmpPath, true);

            using (var bigArchive = new BigArchive(tmpPath, BigArchiveMode.Update))
            {
                Assert.Equal(2, bigArchive.Entries.Count);

                var entry = bigArchive.CreateEntry("b.txt");

                using (var stream = entry.Open())
                {
                    using (var sw = new StreamWriter(stream))
                    {
                        sw.Write("This is sample B");
                    }
                }
            }
        }
    }
}
