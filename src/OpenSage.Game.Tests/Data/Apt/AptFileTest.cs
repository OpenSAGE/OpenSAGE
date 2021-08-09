using OpenSage.Data;
using OpenSage.FileFormats.Apt;
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

                var aptFile = AptFileHelper.FromFileSystemEntry(entry);

                Assert.NotNull(aptFile);
            });
        }

        [GameFact(SageGame.Bfme2)]
        public void CheckEntryCount()
        {
            var fileSystem = new FileSystem(InstalledFilesTestData.GetInstallationDirectory(SageGame.Bfme2));
            var entry = fileSystem.GetFile(@"MainMenu.apt");

            var data = AptFileHelper.FromFileSystemEntry(entry);
            Assert.NotNull(data);
        }
    }
}
