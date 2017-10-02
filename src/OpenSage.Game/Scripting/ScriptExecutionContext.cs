using System.Threading.Tasks;

namespace OpenSage.Scripting
{
    public sealed class ScriptExecutionContext
    {
        private readonly Game _game;

        public ScriptExecutionContext(Game game)
        {
            _game = game;
        }

        public async Task WaitForNextFrame()
        {
            var f = _game.FrameCount;
            while (f == _game.FrameCount)
            {
                await Task.Yield();
            }
        }
    }
}
