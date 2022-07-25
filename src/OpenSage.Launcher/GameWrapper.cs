using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenSage.Launcher.Program;
using OpenSage.Data;
using OpenSage.Diagnostics;
using OpenSage.Graphics;
using OpenSage.Input;
using OpenSage.Logic;
using OpenSage.Mathematics;
using OpenSage.Mods.BuiltIn;
using OpenSage.Network;
using System.IO;
using Veldrid;
using Veldrid.Sdl2;
using OpenSage.Logic.Orders;

namespace OpenSage.Launcher
{
    public class GameWrapper
    {
        public event EventHandler InitializationComplete;

        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private Game game;
        private GameWindow window;
        private TextureCopier textureCopier;
        private DeveloperModeView developerModeView;

        public void Initialize(Options opts)
        {
            logger.Info("Starting...");

            var DetectedGame = opts.Game;
            var GameFolder = opts.GamePath;
            var UseLocators = true;

            if (GameFolder == null)
            {
                GameFolder = Environment.CurrentDirectory;
            }

            foreach (var gameDef in GameDefinition.All)
            {
                if (gameDef.Probe(GameFolder))
                {
                    DetectedGame = gameDef.Game;
                    UseLocators = false;
                }
            }

            var definition = GameDefinition.FromGame(DetectedGame);
            GameInstallation installation;
            if (UseLocators)
            {
                installation = GameInstallation
                    .FindAll(new[] { definition })
                    .FirstOrDefault();
            }
            else
            {
                installation = new GameInstallation(definition, GameFolder);
            }

            if (installation == null)
            {
                Console.WriteLine($"OpenSAGE was unable to find any installations of {definition.DisplayName}.\n");

                Console.WriteLine("You can manually specify the installation path by setting the following environment variable:");
                Console.WriteLine($"\t{definition.Identifier.ToUpper()}_PATH=<installation path>\n");

                Console.WriteLine("OpenSAGE doesn't yet detect every released version of every game. Please report undetected versions to our GitHub page:");
                Console.WriteLine("\thttps://github.com/OpenSAGE/OpenSAGE/issues");

                Console.WriteLine("\n\n Press any key to exit.");

                Console.ReadLine();

                Environment.Exit(1);
            }

            logger.Debug($"Have installation of {definition.DisplayName}");

            Platform.Start();

            var traceEnabled = !string.IsNullOrEmpty(opts.TraceFile);
            if (traceEnabled)
            {
                GameTrace.Start(opts.TraceFile);
            }

            // TODO: Read game version from assembly metadata or .git folder
            // TODO: Set window icon.
            var config = new Configuration()
            {
                UseRenderDoc = opts.RenderDoc,
                LoadShellMap = !opts.NoShellmap,
                UseUniquePorts = opts.UseUniquePorts
            };

            UPnP.InitializeAsync(TimeSpan.FromSeconds(10)).ContinueWith(_ => logger.Info($"UPnP status: {UPnP.Status}"));

            logger.Debug($"Have configuration");

            this.window = new GameWindow($"OpenSAGE - {installation.Game.DisplayName} - master", 100, 100, 1024, 768, opts.Fullscreen);
            this.game = new Game(installation, opts.Renderer, config, window);
            this.textureCopier = new TextureCopier(game, window.Swapchain.Framebuffer.OutputDescription);
            this.developerModeView = new DeveloperModeView(game, window);
            {
                game.GraphicsDevice.SyncToVerticalBlank = !opts.DisableVsync;

                var developerModeEnabled = opts.DeveloperMode;

                if (opts.DeveloperMode)
                {
                    window.Maximized = true;
                }

                if (opts.ReplayFile != null)
                {
                    var replayFile = game.ContentManager.UserDataFileSystem?.GetFile(Path.Combine("Replays", opts.ReplayFile));
                    if (replayFile == null)
                    {
                        logger.Debug("Could not find entry for Replay " + opts.ReplayFile);
                        game.ShowMainMenu();
                    }

                    game.LoadReplayFile(replayFile);
                }
                else if (opts.Map != null)
                {
                    game.Restart = StartMap;
                    StartMap();

                    void StartMap()
                    {
                        var mapCache = game.AssetStore.MapCaches.GetByName(opts.Map);
                        if (mapCache == null)
                        {
                            logger.Debug("Could not find MapCache entry for map " + opts.Map);
                            game.ShowMainMenu();
                        }
                        else if (mapCache.IsMultiplayer)
                        {
                            var pSettings = new PlayerSetting[]
                            {
                                new(1, "FactionAmerica", new ColorRgb(255, 0, 0), 0, PlayerOwner.Player),
                                new(2, "FactionGLA", new ColorRgb(0, 255, 0), 0, PlayerOwner.EasyAi),
                            };

                            logger.Debug("Starting multiplayer game");

                            game.StartSkirmishOrMultiPlayerGame(opts.Map,
                                new EchoConnection(),
                                pSettings,
                                Environment.TickCount,
                                false);
                        }
                        else
                        {
                            logger.Debug("Starting singleplayer game");

                            game.StartSinglePlayerGame(opts.Map);
                        }
                    }
                }
                else
                {
                    logger.Debug("Showing main menu");
                    game.ShowMainMenu();
                }

                game.InputMessageBuffer.Handlers.Add(
                    new CallbackMessageHandler(
                        HandlingPriority.Window,
                        message =>
                        {
                            if (message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.Enter && (message.Value.Modifiers & ModifierKeys.Alt) != 0)
                            {
                                window.Fullscreen = !window.Fullscreen;
                                return InputMessageResult.Handled;
                            }

                            if (message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.F11)
                            {
                                game.NetworkMessageBuffer.StartOrderRecording();
                                return InputMessageResult.Handled;
                            }

                            if (message.MessageType == InputMessageType.KeyDown && message.Value.Key == Key.F12)
                            {
                                game.NetworkMessageBuffer.SaveOrderRecording();
                                return InputMessageResult.Handled;
                            }

                            return InputMessageResult.NotHandled;
                        }));

                logger.Debug("Starting game");

                game.StartRun();
                this.OnInitializationComplete(new EventArgs());

                while (game.IsRunning)
                {
                    if (!window.PumpEvents())
                    {
                        break;
                    }

                    if (developerModeEnabled)
                    {
                        developerModeView.Tick();
                    }
                    else
                    {
                        game.Update(window.MessageQueue);

                        game.Panel.EnsureFrame(window.ClientBounds);

                        game.Render();

                        textureCopier.Execute(
                            game.Panel.Framebuffer.ColorTargets[0].Target,
                            window.Swapchain.Framebuffer);
                    }

                    window.MessageQueue.Clear();

                    game.GraphicsDevice.SwapBuffers(window.Swapchain);
                }
            }

            if (traceEnabled)
            {
                GameTrace.Stop();
            }

            Platform.Stop();
        }

        public void SendKey(KeyEvent keyEvent)
        {
            //_window.KeyDown += HandleKeyDown;
            //_window.KeyUp += HandleKeyUp;

            //_window.MouseDown += HandleMouseDown;
            //_window.MouseUp += HandleMouseUp;
            //_window.MouseMove += HandleMouseMove;
            //_window.MouseWheel += HandleMouseWheel;

            if (keyEvent.Down)
            {
                this.window.HandleKeyDown(keyEvent);
            }
            else
            {
                this.window.HandleKeyUp(keyEvent);
            }
        }

        public void AddOrder(Order order)
        {
            this.game.NetworkMessageBuffer?.AddLocalOrder(order);
        }

        public void SendMouseMove(MouseState state)
        {
            // MouseMoveEventArgs mouseMove = new MouseMoveEventArgs(new MouseState()
            // this.window.handl
        }

        public void Shutdown()
        {
            this.game.Exit();
        }

        protected virtual void OnInitializationComplete(EventArgs e)
        {
            EventHandler raiseEvent = InitializationComplete;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                raiseEvent(this, e);
            }
        }
    }
}
