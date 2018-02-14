using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenSage.Data;
using OpenSage.Data.Rep;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Rep
{
    public class ReplayFileTests
    {
        private readonly ITestOutputHelper _output;

        public ReplayFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Test_001_MoveDozer()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(366, replayFile.Header.NumTimecodes);
            Assert.Equal((ushort) 366, replayFile.Chunks.Last().Header.Timecode);
            Assert.Equal(616, replayFile.Chunks.Count);

            foreach (var chunk in replayFile.Chunks)
            {
                _output.WriteLine($"{chunk.Header.Timecode.ToString().PadLeft(5, ' ')}, {chunk.Header.Number}. {chunk.Message}");
            }
        }

        private static ReplayFile LoadReplayFile([CallerMemberName] string testName = null)
        {
            using (var fileSystem = new FileSystem(Path.Combine(Environment.CurrentDirectory, "Data", "Rep", "Assets")))
            {
                var entry = fileSystem.GetFile(testName + ".rep");
                return ReplayFile.FromFileSystemEntry(entry);
            }
        }
    }
}
