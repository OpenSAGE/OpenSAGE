using System;
using System.IO;
using OpenSage.Graphics;
using OpenSage.IO;
using OpenSage.Tests.Data;
using OpenSage.Tools.ReplaySketch.Model;
using OpenSage.Tools.ReplaySketch.Services;
using Xunit;

namespace OpenSage.Tests.Tools.ReplaySketch;

public class ReplaySketchTests
{
    // Uncomment to run locally (requires game files):
    // private const string LocalOnlySkip = "local debug only";
    private const string? LocalOnlySkip = null;

    [Fact]
    public void DefaultScenario_UsaActions_AreInExpectedOrder()
    {
        var scenario = ReplayScenario.CreateAlpineAssaultUSAvGLA();
        var usa = scenario.Players[0];

        Assert.Equal(4, usa.Actions.Count);
        Assert.Equal(ActionType.BuildBarracks, usa.Actions[0].Type);
        Assert.Equal(ActionType.GatherResources, usa.Actions[1].Type);
        Assert.Equal(ActionType.RecruitBasicUnit, usa.Actions[2].Type);
        Assert.Equal(ActionType.AttackEnemyBase, usa.Actions[3].Type);
    }

    [Fact]
    public void DefaultScenario_GlaActions_AreInExpectedOrder()
    {
        var scenario = ReplayScenario.CreateAlpineAssaultUSAvGLA();
        var gla = scenario.Players[1];

        Assert.Equal(4, gla.Actions.Count);
        Assert.Equal(ActionType.BuildBarracks, gla.Actions[0].Type);
        Assert.Equal(ActionType.GatherResources, gla.Actions[1].Type);
        Assert.Equal(ActionType.RecruitBasicUnit, gla.Actions[2].Type);
        Assert.Equal(ActionType.AttackEnemyBase, gla.Actions[3].Type);
    }

    [Fact]
    public void DefaultScenario_GatherResources_IsScheduledAfterBarracksConstruction()
    {
        var scenario = ReplayScenario.CreateAlpineAssaultUSAvGLA();
        var rng = new Random(0);

        foreach (var player in scenario.Players)
        {
            var barracksFrame = player.Actions[0].Timing.Resolve(rng);
            var gatherOffset = player.Actions[1].Timing.Resolve(rng);

            Assert.Equal(ActionType.BuildBarracks, player.Actions[0].Type);
            Assert.Equal(ActionType.GatherResources, player.Actions[1].Type);
            Assert.True(barracksFrame > 0, $"{player.Name}: BuildBarracks must be scheduled at a non-zero frame.");
            Assert.True(gatherOffset > 0, $"{player.Name}: GatherResources must fire after BuildBarracks (positive offset).");
        }
    }

    [Fact]
    public void DefaultScenario_CumulativeFrames_AreMonotonicallyIncreasing()
    {
        var scenario = ReplayScenario.CreateAlpineAssaultUSAvGLA();
        var rng = new Random(0);

        foreach (var player in scenario.Players)
        {
            uint cumulative = 0;
            uint previous = 0;
            for (var i = 0; i < player.Actions.Count; i++)
            {
                cumulative += player.Actions[i].Timing.Resolve(rng);
                Assert.True(cumulative > previous,
                    $"{player.Name}: action {i} ({player.Actions[i].Type}) must fire at a later frame than the previous action.");
                previous = cumulative;
            }
        }
    }

    /// <summary>
    /// Exports the default Alpine Assault scenario as a replay and runs it through
    /// the full game engine from startup to the last replay frame.
    /// Uncomment <see cref="LocalOnlySkip"/> to run locally with game files installed.
    /// </summary>
    [GameFact(SageGame.CncGenerals, Skip = LocalOnlySkip)]
    public void DefaultScenario_RunsToCompletion_InFullGame()
    {
        var scenario = ReplayScenario.CreateAlpineAssaultUSAvGLA();
        var installation = InstalledFilesTestData.GetInstallation(SageGame.CncGenerals);
        var map = MapMetadataService.Load(installation, scenario.MapPath);
        var outputPath = Path.Combine(Path.GetTempPath(), $"opensage_replaysketch_{Guid.NewGuid()}.rep");

        try
        {
            var exportError = ReplayExporter.Export(scenario, map, outputPath);
            Assert.Null(exportError);

            Platform.Start();
            try
            {
                using var window = new GameWindow("OpenSAGE - ReplaySketch functional test", 100, 100, 1024, 768, false);
                using var game = new Game(installation, null, new Configuration { NoAudio = true }, window);
                using var textureCopier = new TextureCopier(game, window.Swapchain.Framebuffer.OutputDescription);

                using var repFileSystem = new DiskFileSystem(Path.GetDirectoryName(outputPath)!);
                var repEntry = repFileSystem.GetFile(Path.GetFileName(outputPath));

                game.LoadReplayFile(repEntry);
                game.StartRun();

                using var monitor = new ReplayActionMonitor(game);
                monitor.ActionDispatched += (_, e) =>
                    Console.WriteLine($"Action dispatched: {e.ActionType} (player: {e.Player?.Name ?? "?"})");
                monitor.ActionCompleted += (_, e) =>
                    Console.WriteLine($"Action completed:  {e.ActionType} (player: {e.Player?.Name ?? "?"})");

                while (game.IsRunning)
                {
                    if (!window.PumpEvents())
                    {
                        break;
                    }

                    game.Update(window.MessageQueue);
                    game.Panel.EnsureFrame(window.ClientBounds);
                    game.Render();
                    textureCopier.Execute(
                        game.Panel.Framebuffer.ColorTargets[0].Target,
                        window.Swapchain.Framebuffer);
                    window.MessageQueue.Clear();
                    game.GraphicsDevice.SwapBuffers(window.Swapchain);
                }

                // No assertion needed — reaching here without an exception is the success condition.
            }
            finally
            {
                Platform.Stop();
            }
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }
}
