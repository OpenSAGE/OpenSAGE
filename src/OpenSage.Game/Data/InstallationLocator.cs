using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using NLog;
using OpenSage.IO;
using OpenSage.Utilities;
using OpenSage.Utilities.Extensions;

namespace OpenSage.Data;

public sealed class RegistryKeyPath(string key, string valueName, string append = null)
{
    public readonly string Key = key;
    public readonly string ValueName = valueName;

    // This is required because one possible registry key for the Generals + ZH bundle points to the
    // root directory of the bundle.
    public readonly string Append = append;
}

public sealed class GameInstallation
{
    public static IEnumerable<GameInstallation> FindAll(IEnumerable<IGameDefinition> gameDefinitions)
    {
        return InstallationLocators
            .GetAllForPlatform()
            .SelectMany(x => gameDefinitions.SelectMany(y => x.FindInstallations(y)));
    }

    public IGameDefinition Game { get; }
    public string Path { get; }

    private readonly GameInstallation _baseGameInstallation;

    public GameInstallation(IGameDefinition game, string path, GameInstallation baseGame = null)
    {
        Game = game;
        Path = path;
        _baseGameInstallation = baseGame;
    }

    public FileSystem CreateFileSystem()
    {
        var result = new CompositeFileSystem(
            new DiskFileSystem(Path),
            new BigFileSystem(Path));

        if (_baseGameInstallation != null)
        {
            result = new CompositeFileSystem(
                result,
                new BigFileSystem(_baseGameInstallation.Path));
        }

        return result;
    }
}

public interface IInstallationLocator
{
    IEnumerable<GameInstallation> FindInstallations(IGameDefinition game);
}

public class EnvironmentInstallationLocator : IInstallationLocator
{
    public IEnumerable<GameInstallation> FindInstallations(IGameDefinition game)
    {
        var identifier = game.Identifier.ToUpperInvariant() + "_PATH";
        var path = Environment.GetEnvironmentVariable(identifier) ??
                   Environment.GetEnvironmentVariable(identifier, EnvironmentVariableTarget.User);
        if (path == null || !Directory.Exists(path))
        {
            return [];
        }

        var installations = new GameInstallation[] { new(game, path, game.BaseGame != null ? FindInstallations(game.BaseGame).First() : null) };

        return installations;
    }
}

public class RegistryInstallationLocator : IInstallationLocator
{

    // Validates paths to directories. Removes duplicates.
    private static IEnumerable<string> GetValidPaths(IEnumerable<string> paths)
    {
        return paths
            .WhereNot(string.IsNullOrWhiteSpace)
            .Distinct()
            .Where(Directory.Exists);
    }

    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public IEnumerable<GameInstallation> FindInstallations(IGameDefinition game)
    {
        GameInstallation baseGameInstallation = null;

        if (game.BaseGame != null)
        {
            // TODO: Allow selecting one of these?
            baseGameInstallation = FindInstallations(game.BaseGame).FirstOrDefault();

            if (baseGameInstallation == null)
            {
                Logger.Warn("No game installations found");
                return Enumerable.Empty<GameInstallation>();
            }
        }

        var paths = game.RegistryKeys.Select(key => RegistryUtility.GetRegistryValue(key));

        var installations = GetValidPaths(paths)
            .Select(p => new GameInstallation(game, p, baseGameInstallation))
            .ToList();

        return installations;
    }
}

public static class InstallationLocators
{
    public static IEnumerable<IInstallationLocator> GetAllForPlatform()
    {
        yield return new EnvironmentInstallationLocator();
        yield return new SteamInstallationLocator();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return new RegistryInstallationLocator();
        }
    }

    public static IEnumerable<GameInstallation> FindAllInstallations(IGameDefinition game)
    {
        var locators = GetAllForPlatform();
        var result = new List<GameInstallation>();
        foreach (var locator in locators)
        {
            var installations = locator.FindInstallations(game);
            foreach (var installation in installations)
            {
                if (!result.Contains(installation))
                {
                    result.Add(installation);
                }
            }
        }
        return result;
    }
}
