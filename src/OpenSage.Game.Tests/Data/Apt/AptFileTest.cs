using OpenSage.Data.Apt;
using OpenSage.IO;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Apt
{
    public class AptFileTests
    {
        private readonly ITestOutputHelper _output;

        public AptFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [GameFact(SageGame.Bfme, SageGame.Bfme2, SageGame.Bfme2Rotwk, Skip = "Not all bytecode instructions are implemented yet")]
        public void CanReadAptFiles()
        {
            InstalledFilesTestData.ReadFiles(".apt", _output, entry =>
            {
                if (entry.FilePath.Contains("MOD SDK"))
                {
                    return;
                }

                var aptFile = AptFile.FromFileSystemEntry(entry);

                Assert.NotNull(aptFile);
            });
        }

        [GameFact(SageGame.Bfme2)]
        public void CheckEntryCount()
        {
            var fileSystem = new BigFileSystem(InstalledFilesTestData.GetInstallationDirectory(SageGame.Bfme2));
            var entry = fileSystem.GetFile(@"MainMenu.apt");

            var data = AptFile.FromFileSystemEntry(entry);
            Assert.NotNull(data);
        }
    }
}
