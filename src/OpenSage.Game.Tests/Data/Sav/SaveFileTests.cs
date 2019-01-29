using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data.Sav;
using Xunit;

namespace OpenSage.Tests.Data.Sav
{
    public class SaveFileTests
    {
        private static readonly string RootFolder = Path.Combine(Environment.CurrentDirectory, "Data", "Sav", "Assets");

        [Theory]
        [MemberData(nameof(GetSaveFiles))]
        public void CanLoadSaveFiles(string relativePath)
        {
            var fullPath = Path.Combine(RootFolder, relativePath);
            using (var stream = File.OpenRead(fullPath))
            { 
                var saveFile = SaveFile.FromStream(stream);
                Assert.NotEmpty(saveFile.ChunkHeaders);
            }
        }

        public static IEnumerable<object[]> GetSaveFiles()
        {
            foreach (var file in Directory.GetFiles(RootFolder, "*.sav", SearchOption.AllDirectories))
            {
                var relativePath = file.Substring(RootFolder.Length + 1);
                yield return new object[] { relativePath };
            }
        }
    }
}
