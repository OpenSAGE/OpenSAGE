using System;
using System.IO;
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
            Assert.Equal(616, replayFile.Chunks.Count);

            WriteMessages(replayFile);
        }

        [Fact]
        public void Test_002_BuildPowerPlant()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1474, replayFile.Header.NumTimecodes);
            Assert.Equal(1735, replayFile.Chunks.Count);

            WriteMessages(replayFile);
        }

        [Fact]
        public void Test_003_BuildPowerPlantAndDozer()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1211, replayFile.Header.NumTimecodes);
            Assert.Equal(1416, replayFile.Chunks.Count);

            WriteMessages(replayFile);
        }

        [Fact]
        public void Test_004_TrainInfantry()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1693, replayFile.Header.NumTimecodes);
            Assert.Equal(1797, replayFile.Chunks.Count);

            WriteMessages(replayFile);
        }

        private static ReplayFile LoadReplayFile([CallerMemberName] string testName = null)
        {
            using (var fileSystem = new FileSystem(Path.Combine(Environment.CurrentDirectory, "Data", "Rep", "Assets")))
            {
                var entry = fileSystem.GetFile(testName + ".rep");
                return ReplayFile.FromFileSystemEntry(entry);
            }
        }

        private void WriteMessages(ReplayFile replayFile)
        {
            foreach (var chunk in replayFile.Chunks)
            {
                _output.WriteLine($"{chunk.Header.Timecode.ToString().PadLeft(5, ' ')}, {chunk.Header.Number}. {chunk.Message}");
            }
        }
    }
}
