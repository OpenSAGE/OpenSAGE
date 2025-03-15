
#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
using OpenSage.Utilities;

namespace OpenSage.Data;

// A Steam installation locator based on https://stackoverflow.com/a/34091380
// and https://developer.valvesoftware.com/wiki/KeyValues

/// <summary>
/// Finds installed Steam versions of SAGE games.
/// </summary>
public class SteamInstallationLocator : IInstallationLocator
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    // Not that it matters in the grand scheme of things, but this API is not very efficient for Steam.
    // We have to repeat registry fetch + file loading + parsing for each game.
    // Maybe we should have a different method which receives a list of game definitions and returns a list of installations?
    public IEnumerable<GameInstallation> FindInstallations(IGameDefinition game)
    {
        if (game.Steam == null)
        {
            yield break;
        }

        var libraryFoldersPath = GetSteamLibraryManifestPath();
        if (libraryFoldersPath == null)
        {
            yield break;
        }

        if (!File.Exists(libraryFoldersPath))
        {
            Logger.Warn($"Steam libraryfolders.vdf file not found at {libraryFoldersPath}.");
            yield break;
        }

        using var libraryFoldersReader = new StreamReader(libraryFoldersPath, Encoding.UTF8);
        VdfDictionary libraryManifest;
        try
        {
            libraryManifest = new VdfParser(libraryFoldersReader).Parse();
        }
        catch (Exception e)
        {
            Logger.Warn($"Failed to parse libraryfolders.vdf file at {libraryFoldersPath}: {e.Message}");
            yield break;
        }

        var gameInstallationPath = GetGameInstallationPath(libraryManifest, game.Steam);
        if (gameInstallationPath == null)
        {
            yield break;
        }

        yield return new GameInstallation(game, gameInstallationPath);
    }

    private static string? GetGameInstallationPath(VdfDictionary libraryManifest, SteamInstallationDefinition steamDefinition)
    {
        if (!libraryManifest.TryGetValue("libraryfolders", out var libraryFoldersValue))
        {
            Logger.Warn("libraryfolders key not found in libraryfolders.vdf.");
            return null;
        }

        var libraryFolders = libraryFoldersValue.ExpectDictionary("libraryfolders");

        foreach (var library in libraryFolders.Select(kv => kv.Value.ExpectDictionary(kv.Key)))
        {
            var libraryPath = library["path"].ExpectString("path");
            var apps = library["apps"].ExpectDictionary("apps");
            var containsAppId = apps.ContainsKey(steamDefinition.AppId.ToString());

            if (containsAppId)
            {
                var gamePath = Path.Combine(libraryPath, "steamapps", "common", steamDefinition.FolderName);

                if (!Directory.Exists(gamePath))
                {
                    Logger.Warn($"Game was found in libraryfolders.vdf, but directory does not exist at {gamePath}.");
                    return null;
                }

                return gamePath;
            }
        }

        return null;
    }

    private static string? GetSteamLibraryManifestPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            const string SteamRegistryKey = @"SOFTWARE\Valve\Steam";
            const string SteamRegistryValue = "InstallPath";
            var keyPath = new RegistryKeyPath(SteamRegistryKey, SteamRegistryValue);
            var installPath = RegistryUtility.GetRegistryValue(keyPath, RegistryView.Registry64) ??
                              RegistryUtility.GetRegistryValue(keyPath, RegistryView.Registry32);

            if (installPath == null)
            {
                Logger.Warn("Steam installation not found in registry.");
                return null;
            }

            return Path.Combine(
                installPath,
                "steamapps",
                "libraryfolders.vdf"
            );
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".local",
                "share",
                "Steam",
                "steamapps",
                "libraryfolders.vdf"
            );
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Steam's UI doesn't allow installing games that don't have a macOS version (which is the case for all SAGE games),
            // but there are workarounds (such as SteamCMD or -console flag) that can be used to install Windows games.
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                "Library",
                "Application Support",
                "Steam",
                "steamapps",
                "libraryfolders.vdf"
            );
        }

        return null;
    }
}

/// <summary>
/// A Valve Data Format (VDF) parser.
/// </summary>
public class VdfParser(StreamReader streamReader)
{
    private readonly VdfTokenStream _tokenStream = new(streamReader);

    public VdfDictionary Parse()
    {
        // Top level VDF object is always(?) a dictionary, but without braces.
        var result = ParseDictionary(false);
        // Ensure we've reached the end of the file.
        if (_tokenStream.Peek() != null)
        {
            throw new InvalidDataException("Unexpected data at end of file.");
        }

        return result;
    }

    private (string, VdfValue)? ParseKeyValuePair()
    {
        var keyToken = _tokenStream.Peek();

        if (keyToken == null || keyToken is RightBraceToken)
        {
            return null;
        }

        if (keyToken is not StringToken keyStringToken)
        {
            throw new InvalidDataException($"Expected string token, but got {keyToken.Name} at position {_tokenStream.Position}.");
        }

        _tokenStream.Read();

        var valueToken = _tokenStream.Peek() ?? throw new InvalidDataException("Unexpected end of file.");

        switch (valueToken)
        {
            case StringToken valueStringToken:
                _tokenStream.Read();
                return (keyStringToken.Value, new VdfValue(valueStringToken.Value));
            case LeftBraceToken:
                return (keyStringToken.Value, new VdfValue(ParseDictionary()));
            default:
                throw new InvalidDataException($"Unexpected token {valueToken.Name} at position {_tokenStream.Position}.");
        }
    }

    private VdfDictionary ParseDictionary(bool expectBraces = true)
    {
        if (expectBraces)
        {
            var token = _tokenStream.Peek() ?? throw new InvalidDataException("Unexpected end of file.");
            if (token is not LeftBraceToken)
            {
                throw new InvalidDataException($"Expected left brace, but got {token.Name} at position {_tokenStream.Position}.");
            }
            _tokenStream.Read();
        }

        var result = new VdfDictionary();

        while (true)
        {
            var keyValuePair = ParseKeyValuePair();
            if (keyValuePair == null)
            {
                break;
            }

            result.Add(keyValuePair.Value.Item1, keyValuePair.Value.Item2);
        }

        if (expectBraces)
        {
            var nextToken = _tokenStream.Peek() ?? throw new InvalidDataException("Unexpected end of file.");
            if (nextToken is RightBraceToken)
            {
                _tokenStream.Read();
            }
        }

        return result;
    }

    private abstract class VdfToken
    {
        public abstract string Name { get; }
    }

    private sealed class StringToken(string value) : VdfToken
    {
        public readonly string Value = value;
        public override string Name => "String";
    }

    private sealed class LeftBraceToken : VdfToken
    {
        public override string Name => "LeftBrace";

        private LeftBraceToken()
        {
        }

        public static readonly LeftBraceToken Instance = new();
    }

    private sealed class RightBraceToken : VdfToken
    {
        public override string Name => "RightBrace";

        private RightBraceToken()
        {
        }

        public static readonly RightBraceToken Instance = new();
    }

    private sealed class VdfTokenStream(StreamReader streamReader)
    {
        private readonly StreamReader _streamReader = streamReader;

        private VdfToken? _currentToken;
        public long Position { get; private set; }

        public VdfToken? Peek()
        {
            if (_currentToken == null)
            {
                _currentToken = ReadNextToken();
            }

            return _currentToken;
        }

        public VdfToken? Read()
        {
            var result = Peek();
            _currentToken = null;
            return result;
        }

        private VdfToken? ReadNextToken()
        {
            while (true)
            {
                var maybeNextChar = _streamReader.Read();

                if (maybeNextChar == -1)
                {
                    return null;
                }

                Position++;

                var nextChar = (char)maybeNextChar;

                if (char.IsWhiteSpace(nextChar))
                {
                    continue;
                }

                return nextChar switch
                {
                    '"' => ReadStringToken(),
                    '{' => LeftBraceToken.Instance,
                    '}' => RightBraceToken.Instance,
                    _ => throw new InvalidDataException($"Unexpected character '{nextChar}' at position {Position}."),
                };
            }
        }

        private StringToken? ReadStringToken()
        {
            var value = string.Empty;

            while (true)
            {
                var maybeNextChar = _streamReader.Read();
                if (maybeNextChar == -1)
                {
                    throw new InvalidDataException("Unexpected end of file.");
                }
                Position++;

                var nextChar = (char)maybeNextChar;

                // Allowed Escape sequences are \n, \t, \\, and \"
                if (nextChar == '\\')
                {
                    var escapeChar = (char)_streamReader.Read();
                    Position++;
                    value += escapeChar switch
                    {
                        'n' => '\n',
                        't' => '\t',
                        '\\' => '\\',
                        '"' => '"',
                        _ => throw new InvalidDataException($"Unexpected escape sequence '\\{escapeChar}' at position {Position}."),
                    };
                }
                else if (nextChar == '"')
                {
                    return new StringToken(value);
                }
                else
                {
                    value += nextChar;
                }

            }
        }
    }
}

public class VdfValue
{
    private object _value;

    public string? StringValue
    {
        get => _value as string;
    }

    public VdfDictionary? DictionaryValue
    {
        get => _value as VdfDictionary;
    }

    public VdfValue(string value)
    {
        _value = value;
    }

    public VdfValue(VdfDictionary value)
    {
        _value = value;
    }

    public VdfDictionary ExpectDictionary(string key)
    {
        if (_value is VdfDictionary dictionary)
        {
            return dictionary;
        }

        throw new InvalidDataException($"Expected dictionary for key '{key}', but got {_value.GetType().Name}.");
    }

    public string ExpectString(string key)
    {
        if (_value is string stringValue)
        {
            return stringValue;
        }

        throw new InvalidDataException($"Expected string for key '{key}', but got {_value.GetType().Name}.");
    }
}

public class VdfDictionary : IEnumerable<KeyValuePair<string, VdfValue>>
{
    private readonly Dictionary<string, VdfValue> _values = [];

    public VdfValue this[string key]
    {
        get => _values[key];
        set => _values[key] = value;
    }

    public bool TryGetValue(string key, [NotNullWhen(true)] out VdfValue? value)
    {
        return _values.TryGetValue(key, out value);
    }

    public bool ContainsKey(string key)
    {
        return _values.ContainsKey(key);
    }

    public void Add(string key, VdfValue value)
    {
        _values.Add(key, value);
    }

    public int Count => _values.Count;

    public IEnumerator<KeyValuePair<string, VdfValue>> GetEnumerator()
    {
        return _values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
