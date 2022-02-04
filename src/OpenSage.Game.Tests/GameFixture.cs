using System;
using System.Collections.Generic;
using OpenSage.Tests.Data;

namespace OpenSage.Tests
{
    public class GameFixture : IDisposable
    {
        private readonly Dictionary<SageGame, Game> _games = new();

        public GameFixture()
        {
            Platform.Start();
        }

        public Game GetGame(SageGame sageGame)
        {
            if (!_games.TryGetValue(sageGame, out var game))
            {
                var installation = InstalledFilesTestData.GetInstallation(sageGame);

                game = new Game(installation);

                _games.Add(sageGame, game);
            }

            return game;
        }

        public void Dispose()
        {
            foreach (var kvp in _games)
            {
                kvp.Value.Dispose();
            }

            Platform.Stop();
        }
    }
}
