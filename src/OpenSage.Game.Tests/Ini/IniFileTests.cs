using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
            var tasks = new List<Task>();

            InstalledFilesTestData.ReadFiles(".ini", _output, entry =>
            {
                if (Path.GetFileName(entry.FilePath) == "ButtonSets.ini")
                    return; // This file doesn't seem to be used?

                if (Path.GetFileName(entry.FilePath) == "Scripts.ini")
                    return; // ZH only, and only needed by World Builder?

                tasks.Add(Task.Run(() =>
                {
                    var dataContext = new IniDataContext();
                    dataContext.LoadIniFile(entry);

                    Assert.NotNull(dataContext.CommandMaps);
                }));
            });

            Task.WaitAll(tasks.ToArray());
        }
    }
}
