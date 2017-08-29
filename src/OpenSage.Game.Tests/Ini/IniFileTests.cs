using System.IO;
using OpenSage.Data.Ini;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Data.Tests.Ini
{
    public class IniFileTests
    {
        private readonly ITestOutputHelper _output;

        public IniFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CanReadIniFiles()
        {
            InstalledFilesTestData.ReadFiles(".ini", _output, (fileName, openFileStream) =>
            {
                if (Path.GetFileName(fileName) == "ButtonSets.ini")
                    return; // This file doesn't seem to be used?

                if (Path.GetFileName(fileName) == "Scripts.ini")
                    return; // ZH only, and only needed by World Builder?

                var dataContext = new IniDataContext();

                using (var fileStream = openFileStream())
                    dataContext.LoadIniFile(fileStream, fileName);

                Assert.NotNull(dataContext.CommandMaps);
            });
        }
    }
}
