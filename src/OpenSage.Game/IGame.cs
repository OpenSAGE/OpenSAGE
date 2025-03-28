using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Audio;
using OpenSage.Client;
using OpenSage.Content;
using OpenSage.Data.Sav;
using OpenSage.Graphics;
using OpenSage.Gui;
using OpenSage.Gui.Apt;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Input;
using OpenSage.Input.Cursors;
using OpenSage.IO;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Network;
using OpenSage.Scripting;
using Veldrid;

namespace OpenSage;

public interface IGame
{
    CursorManager Cursors { get; }
    internal GraphicsLoadContext GraphicsLoadContext { get; }
    AssetStore AssetStore { get; }
    ContentManager ContentManager { get; }
    GraphicsDevice GraphicsDevice { get; }
    InputMessageBuffer InputMessageBuffer { get; }
    SkirmishManager SkirmishManager { get; set; }
    LobbyManager LobbyManager { get; }
    List<GameSystem> GameSystems { get; }

    /// <summary>
    /// Gets the graphics system.
    /// </summary>
    GraphicsSystem Graphics { get; }

    /// <summary>
    /// Gets the scripting system.
    /// </summary>
    ScriptingSystem Scripting { get; }

    /// <summary>
    /// Load lua script engine.
    /// </summary>
    LuaScriptEngine Lua { get; set; }

    /// <summary>
    /// Gets the selection system.
    /// </summary>
    SelectionSystem Selection { get; }

    /// <summary>
    /// Gets the order generator system.
    /// </summary>
    OrderGeneratorSystem OrderGenerator { get; }

    /// <summary>
    /// Gets the audio system
    /// </summary>
    AudioSystem Audio { get; }

    GameState GameState { get; }
    internal GameStateMap GameStateMap { get; }
    CampaignManager CampaignManager { get; }
    Terrain.TerrainLogic TerrainLogic { get; }
    Terrain.TerrainVisual TerrainVisual { get; }
    GhostObjectManager GhostObjectManager { get; }

    /// <summary>
    /// Is the game running?
    /// This is only false when the game is shutting down.
    /// </summary>
    bool IsRunning { get; }

    Action Restart { get; set; }

    /// <summary>
    /// Are we currently in a skirmish game?
    /// </summary>
    bool InGame { get; }

    float LogicUpdateScaleFactor { get; set; }

    /// <summary>
    /// Is the game running logic updates?
    /// Automatically starts and stops the map timer.
    /// </summary>
    bool IsLogicRunning { get; set; }

    /// <summary>
    /// The amount of time the game has been in this map while running logic updates.
    /// </summary>
    TimeInterval MapTime { get; }

    TimeInterval CurrentGameTime { get; }

    /// <summary>
    /// The amount of time the game has been rendering frames.
    /// </summary>
    TimeInterval RenderTime { get; }

    TimeSpan CumulativeLogicUpdateError { get; }
    IGameDefinition Definition { get; }
    SageGame SageGame { get; }
    Configuration Configuration { get; }
    string UserDataLeafName { get; }
    string UserDataFolder { get; }
    string UserAppDataFolder { get; }
    GamePanel Panel { get; }
    Viewport Viewport { get; }
    Scene2D Scene2D { get; }
    Scene3D Scene3D { get; set; }

    NetworkMessageBuffer NetworkMessageBuffer { get; set; }

    Texture LauncherImage { get; }
    internal GameLogic GameLogic { get; }
    internal GameClient GameClient { get; }
    PlayerManager PlayerManager { get; }
    TeamFactory TeamFactory { get; }
    PartitionCellManager PartitionCellManager { get; }
    GameEngine GameEngine { get; }

    event EventHandler<GameUpdatingEventArgs> Updating;

    /// <summary>
    /// Fired when a <see cref="Game.Render"/> completes, but before
    /// <see cref="Game.Panel"/>'s <see cref="GamePanel.Framebuffer"/>
    /// is copied to <see cref="GraphicsDevice.SwapchainFramebuffer"/>.
    /// Useful for drawing additional overlays.
    /// </summary>
    event EventHandler RenderCompleted;

    void LoadSaveFile(FileSystemEntry entry);
    void LoadReplayFile(FileSystemEntry replayFileEntry);
    void ShowMainMenu();
    Window LoadWindow(string wndFileName);
    AptWindow LoadAptWindow(string aptFileName);
    void StartCampaign(string side);
    void StartCampaign(string campaignName, string missionName);

    void StartSkirmishOrMultiPlayerGame(
        string mapFileName,
        IConnection connection,
        PlayerSetting[] playerSettings,
        int seed,
        bool isMultiPlayer);

    void StartSinglePlayerGame(string mapFileName);
    void EndGame();
    void StartRun();
    void Update(IEnumerable<InputMessage> messages);
    void Step();
    void Render();

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources
    /// </summary>
    void Dispose();

    Vector2 GetTopLeftUV();
    Vector2 GetBottomRightUV();
    IEnumerable<PlayerTemplate> GetPlayableSides();
    MappedImage GetMappedImage(string name);
    void Exit();

    // TODO: Move this somewhere where it could be configured for specific games;
    // e.g. for a replay we might want to use SageRandom.
    IRandom CreateRandom() => new SystemRandom();
}
