using System;
using System.IO;
using System.Runtime.CompilerServices;
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

        [Fact]
        public void Test_010_CancelConstruction()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(408, replayFile.Header.NumTimecodes);
            Assert.Equal(482, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_011_Spydrone()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(2197, replayFile.Header.NumTimecodes);
            Assert.Equal(2280, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_012_ResumeConstruction()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(877, replayFile.Header.NumTimecodes);
            Assert.Equal(1001, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_013_SetGroup()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(207, replayFile.Header.NumTimecodes);
            Assert.Equal(273, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_014_GatherDumpSupplies()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(2806, replayFile.Header.NumTimecodes);
            Assert.Equal(3320, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_015_AttackMove()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(2508, replayFile.Header.NumTimecodes);
            Assert.Equal(2653, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_016_ClearMines()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(216, replayFile.Header.NumTimecodes);
            Assert.Equal(306, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_017_Waypoint()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(987, replayFile.Header.NumTimecodes);
            Assert.Equal(1095, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_018_SelectAcrossScreen()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(835, replayFile.Header.NumTimecodes);
            Assert.Equal(937, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_019_RepairVehicle()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(3220, replayFile.Header.NumTimecodes);
            Assert.Equal(3571, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_020_ToggleFormationMode()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(594, replayFile.Header.NumTimecodes);
            Assert.Equal(855, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_021_GuardMode()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1878, replayFile.Header.NumTimecodes);
            Assert.Equal(2092, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_022_Scatter()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(421, replayFile.Header.NumTimecodes);
            Assert.Equal(544, replayFile.Chunks.Count);

            WriteOrders(replayFile);
        }

        [Fact]
        public void Test_023_Cheer()
        {
            var replayFile = LoadReplayFile();

            Assert.Equal(ReplayGameType.Generals, replayFile.Header.GameType);
            Assert.Equal(1287, replayFile.Header.NumTimecodes);
            Assert.Equal(25, replayFile.Chunks.Count);

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
