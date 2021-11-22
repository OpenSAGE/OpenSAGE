using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenSage.Launcher;
using System.Windows.Forms;
using System.Threading;
using OpenSage.Logic.Orders;

namespace OpenSage.Game.FunctionalTests
{
    [TestClass]
    public class SkirmishTest
    {
        AutoResetEvent initComplete;

        [TestMethod]
        public void StartSkirmish()
        {
            Options options = new Options()
            {
                Game = SageGame.CncGenerals,
                Map = "maps\\alpine assault\\alpine assault.map"
            };

            this.initComplete = new AutoResetEvent(false);
            GameWrapper game = new GameWrapper();
            game.InitializationComplete += Game_InitializationComplete;
            Task hostThread = Task.Run(() => game.Initialize(options));

            this.initComplete.WaitOne();

            // do I need this sleep here? The game should be able to receive orders immediately but I'm not sure
            System.Threading.Thread.Sleep(1000);

            // Get position of all objects: command center, bulldozen, supply stash
            // Create the first order here. Build a power plant

            // Build a supply depot
            game.Shutdown();
        }

        private void Game_InitializationComplete(object? sender, System.EventArgs e)
        {
            this.initComplete.Set();
        }
    }
}
