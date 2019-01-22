using System;
using System.IO;
using System.Runtime.CompilerServices;
using OpenSage.Data;
using OpenSage.Data.Rep;
using OpenSage.Data.Sav;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Sav
{
    public class SaveFileTests
    {
        private readonly ITestOutputHelper _output;

        public SaveFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test_001_GLA_02_Start()
        {
            var saveFile = LoadSaveFile();

            //Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            //Assert.Equal(366, replayFile.Header.NumTimecodes);
            //Assert.Equal(616, replayFile.Chunks.Count);
        }

        private static SaveFile LoadSaveFile([CallerMemberName] string testName = null)
        {
            using (var fileSystem = new FileSystem(Path.Combine(Environment.CurrentDirectory, "Data", "Sav", "Assets")))
            {
                var entry = fileSystem.GetFile(testName + ".sav");
                return SaveFile.FromFileSystemEntry(entry);
            }
        }
    }
}
