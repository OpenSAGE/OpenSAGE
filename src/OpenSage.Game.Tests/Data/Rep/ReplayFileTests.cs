using System;
using System.IO;
using System.Runtime.CompilerServices;
using OpenSage.Data;
using OpenSage.Data.Rep;
using OpenSage.IO;
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

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_002_BuildPowerPlant()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1474, replayFile.Header.NumTimecodes);
            Assert.Equal(1735, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_003_BuildPowerPlantAndDozer()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1211, replayFile.Header.NumTimecodes);
            Assert.Equal(1416, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_004_TrainInfantry()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1693, replayFile.Header.NumTimecodes);
            Assert.Equal(1797, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_005_BuildSellPowerPlant()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(2395, replayFile.Header.NumTimecodes);
            Assert.Equal(2565, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_006_TrainInfantryAttackGround()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1839, replayFile.Header.NumTimecodes);
            Assert.Equal(1998, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_007_PlayerUpgradeStartCancel()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1256, replayFile.Header.NumTimecodes);
            Assert.Equal(1376, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_008_TrainInfantryCancel()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1095, replayFile.Header.NumTimecodes);
            Assert.Equal(1170, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_009_EnterHumvee()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(3517, replayFile.Header.NumTimecodes);
            Assert.Equal(4163, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        private static ReplayFile LoadReplayFile([CallerMemberName] string testName = null)
        {
            using (var fileSystem = new DiskFileSystem(Path.Combine(Environment.CurrentDirectory, "Data", "Rep", "Assets")))
            {
                var entry = fileSystem.GetFile(testName + ".rep");
                return ReplayFile.FromFileSystemEntry(entry);
            }
        }

        private void WriteOrders(ReplayFile replayFile)
        {
            foreach (var chunk in replayFile.Chunks)
            {
                _output.WriteLine($"{chunk.Header.Timecode.ToString().PadLeft(5, ' ')}, {chunk.Header.Number}. {chunk.Order}");
            }
        }
    }
}
