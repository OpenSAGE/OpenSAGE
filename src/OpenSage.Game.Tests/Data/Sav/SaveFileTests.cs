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
    public class SaveFileTests
    {
        private static readonly string RootFolder = Path.Combine(Environment.CurrentDirectory, "Data", "Sav", "Assets");

        [Theory]
        [MemberData(nameof(GetSaveFiles))]
        public void CanLoadSaveFiles(string relativePath)
        {
            var rootFolder = InstalledFilesTestData.GetInstallationDirectory(SageGame.CncGenerals);
            var installation = new GameInstallation(new GeneralsDefinition(), rootFolder);

            Platform.Start();

            using (var game = new Game(installation, GraphicsBackend.Direct3D11))
            {
                var fullPath = Path.Combine(RootFolder, relativePath);
                using (var stream = File.OpenRead(fullPath))
                {
                    SaveFile.LoadFromStream(stream, game);
                }
            }

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
