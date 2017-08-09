using OpenZH.Data.Ini;
using Xunit;
using Xunit.Abstractions;

namespace OpenZH.Data.Tests.Ini
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
                var dataContext = new IniDataContext();

                using (var fileStream = openFileStream())
                    dataContext.LoadIniFile(fileStream, fileName);

                Assert.NotNull(dataContext.CommandMaps);
            });
        }
    }
}
