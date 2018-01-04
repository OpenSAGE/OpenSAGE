using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

using SharpDX.Windows;

using OpenSage.Data;
using OpenSage.LowLevel;

namespace OpenSage
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            // TODO: Get this from a launch parameter.
            const SageGame sageGame = SageGame.CncGenerals;

            // TODO: Support other locators.
            var locator = new RegistryInstallationLocator();
            var installation = locator.FindInstallations(sageGame).First();
            var fileSystem = installation.CreateFileSystem();

            HostPlatform.Start();

            var game = new Game(HostPlatform.GraphicsDevice, fileSystem, sageGame);

            SetupInitialScene(game);

            var hostView = new GameView
            {
                Game = game,
                Dock = DockStyle.Fill
            };

            // TODO: Use something other than WinForms for cross-platform compatibility.
            var window = new Form
            {
                Size = new Size(1024, 768),
                // TODO: Read game version from assembly metadata or .git folder
                Text = "OpenSAGE (master)",
                Icon = GetIcon()
            };
            window.Controls.Add(hostView);
            window.Show();

            // TODO: This only works on Windows and DX11. Implement this for other platforms.
            RenderLoop.Run(hostView, game.Tick);

            HostPlatform.Stop();
        }

        private static Icon GetIcon()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("OpenSage.Windows.Resources.AppIcon.ico"))
            {
                return new Icon(stream);
            }
        }

        // TODO: Extract this logic into a game-specific DLL, or make the scene and menu configurable.
        // TODO: Implement fast startup, where the shellmap is not loaded.
        private static void SetupInitialScene(Game game)
        {
            var mainMenuScene = game.ContentManager.Load<Scene>("maps\\ShellMap1\\ShellMap1.map");
            game.Scene = mainMenuScene;

            game.Wnd.OpenWindow("Menus\\MainMenu.wnd");

            game.Scripting.Active = true;
        }
    }
}
