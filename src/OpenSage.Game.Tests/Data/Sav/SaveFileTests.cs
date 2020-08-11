using System;
using System.Collections.Generic;
using System.IO;
using OpenSage.Data;
using OpenSage.Data.Sav;
using OpenSage.Mods.Generals;
using Veldrid;
using Xunit;

namespace OpenSage.Tests.Data.Sav
{
    public class SaveFileTests : IDisposable
    {
        private static readonly string RootFolder = Path.Combine(Environment.CurrentDirectory, "Data", "Sav", "Assets");

        private readonly Game _game;

        public SaveFileTests()
        {
            var rootFolder = InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGenerals);
            var installation = new GameInstallation(new GeneralsDefinition(), rootFolder);

            Platform.Start();

            _game = new Game(installation, GraphicsBackend.Direct3D11);
        }

        [Theory(Skip = "Doesn't work yet")]
        [MemberData(nameof(GetSaveFiles))]
        public void CanLoadSaveFiles(string relativePath)
        {
            var fullPath = Path.Combine(RootFolder, relativePath);

            using var stream = File.OpenRead(fullPath);

            SaveFile.LoadFromStream(stream, _game);

            _game.EndGame();
        }

        public void Dispose()
        {
            _game.Dispose();

            Platform.Stop();
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
