using System;
using System.IO;
using System.Linq;
using CommandLine;
using OpenSage.Data;
using OpenSage.Data.Sav;
using OpenSage.Mods.BuiltIn;

namespace OpenSage.Tools.Sav2Json;

public static class Program
{
    public static void Main(string[] args)
    {
        var result = Parser.Default
            .ParseArguments<Options>(args)
            .WithParsed(Run);
    }

    private static void Run(Options opts)
    {
        Platform.Start();

        var definition = GameDefinition.FromGame(opts.SageGame);
        var installation = InstallationLocators.FindAllInstallations(definition).First();
        var game = new Game(installation);

        // Read .sav file from binary file.
        using var stream = File.OpenRead(opts.SaveFilePath);
        SaveFile.LoadFromStream(stream, game);

        // Write .sav file to JSON.
        var outputPath = Path.ChangeExtension(opts.SaveFilePath, ".json");
        using var jsonWriter = new JsonSaveWriter(game, outputPath);
        SaveFile.Persist(jsonWriter);

        game.EndGame();

        Platform.Stop();
    }

    public sealed class Options
    {
        [Value(0, MetaName = "SageGame", HelpText = "The SAGE game that this .sav file is from.")]
        public SageGame SageGame { get; set; }

        [Value(1, MetaName = "SaveFilePath", HelpText = "Full path to .sav file.", Required = true)]
        public string SaveFilePath { get; set; }
    }
}
