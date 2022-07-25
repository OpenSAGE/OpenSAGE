using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenSage.Content;
using OpenSage.Core.Graphics;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.IO;
using OpenSage.Mods.BuiltIn;
using OpenSage.Rendering;
using OpenSage.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace OpenSage.Tests.Data.Ini
{
    public class IniFileTests
    {
        private static readonly Encoding LocaleSpecificEncoding;

        static IniFileTests()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            LocaleSpecificEncoding = Encoding.GetEncoding(CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
        }

        private readonly ITestOutputHelper _output;

        public IniFileTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact, Trait("Category", "Interactive")]
        public void CanReadIniFiles()
        {
            var gameDefinitions = new[]
            {
                GameDefinition.FromGame(SageGame.CncGenerals),
                GameDefinition.FromGame(SageGame.CncGeneralsZeroHour),
                GameDefinition.FromGame(SageGame.Bfme),
                GameDefinition.FromGame(SageGame.Bfme2),
                GameDefinition.FromGame(SageGame.Bfme2Rotwk),
            };

            using var graphicsDevice = GraphicsDeviceUtility.CreateGraphicsDevice(null, null);
            using var graphicsDeviceManager = new GraphicsDeviceManager(graphicsDevice);

            using var shaderSetStore = new ShaderSetStore(graphicsDeviceManager, RenderPipeline.GameOutputDescription);
            using var shaderResources = new ShaderResourceManager(graphicsDeviceManager, shaderSetStore);
            var graphicsLoadContext = new GraphicsLoadContext(graphicsDeviceManager, shaderResources, shaderSetStore);

            foreach (var gameDefinition in gameDefinitions)
            {
                foreach (var installation in InstallationLocators.FindAllInstallations(gameDefinition))
                {
                    using var fileSystem = installation.CreateFileSystem();

                    var assetStore = new AssetStore(
                        gameDefinition.Game,
                        fileSystem,
                        LanguageUtility.ReadCurrentLanguage(gameDefinition, fileSystem),
                        graphicsDeviceManager,
                        shaderResources,
                        shaderSetStore,
                        gameDefinition.CreateAssetLoadStrategy());

                    assetStore.PushScope();

                    var dataContext = new IniDataContext();

                    void LoadIniFile(FileSystemEntry entry)
                    {
                        var parser = new IniParser(
                            entry,
                            assetStore,
                            gameDefinition.Game,
                            dataContext,
                            LocaleSpecificEncoding);

                        parser.ParseFile();
                    }

                    switch (gameDefinition.Game)
                    {
                        case SageGame.Bfme:
                        case SageGame.Bfme2:
                        case SageGame.Bfme2Rotwk:
                            LoadIniFile(fileSystem.GetFile(@"Data\INI\GameData.ini"));
                            break;
                    }

                    foreach (var file in fileSystem.GetFilesInDirectory("", $"*.ini", SearchOption.AllDirectories))
                    {
                        var filename = Path.GetFileName(file.FilePath).ToLowerInvariant();

                        switch (filename)
                        {
                            case "webpages.ini": // Don't care about this

                            case "buttonsets.ini": // Doesn't seem to be used?
                            case "scripts.ini": // Only needed by World Builder?
                            case "commandmapdebug.ini": // Only applies to DEBUG and INTERNAL builds
                            case "fxparticlesystemcustom.ini": // Don't know if this is used, it uses Emitter property not used elsewhere
                            case "lightpoints.ini": // Don't know if this is used.

                            //added in BFME and subsequent games
                            case "optionregistry.ini": // Don't know if this is used
                            case "localization.ini": // Don't know if we need this
                                continue;

                            case "credits.ini":
                                if (gameDefinition.Game == SageGame.Bfme2Rotwk) //corrupted in rotwk (start of the block is commented out)
                                {
                                    continue;
                                }
                                break;

                            //mods specific

                            //edain mod
                            case "einstellungen.ini":
                            case "einstellungendeakt.ini":
                            case "einstellungenedain.ini":
                            case "news.ini":
                                continue;

                            //unofficial patch 2.02
                            case "desktop.ini": //got into a big file somehow
                            case "2.01.ini":
                            case "disable timer.ini":
                            case "enable timer.ini":
                            case "old music.ini":
                                continue;
                            default:
                                if (filename.StartsWith("2.02")) { continue; }
                                break;
                        }

                        _output.WriteLine($"Reading file {file.FilePath}.");

                        LoadIniFile(file);
                    }
                }
            }
        }
    }
}
