using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Mods.BuiltIn;
using OpenSage.Utilities.Extensions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSage.Tests
{
    public sealed class GameTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        private static readonly ISet<IGameDefinition> InstalledGames;

        public GameTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var arguments = factAttribute.GetConstructorArguments().ToList();
            var game = (SageGame) arguments[0];
            var otherGames = (SageGame[]) arguments[1];

            var games = new[] { game }.Union(otherGames).Select(GameDefinition.FromGame).ToArray();

            if (!InstalledGames.Any(x => games.Contains(x)))
            {
                var gameNames = string.Join(", ", games.Select(definition => definition.DisplayName));

                _diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage($"Skipped test {testMethod.TestClass.Class.Name}.{testMethod.Method.Name}, because it requires one or more of the following games to be installed: {gameNames}.")
                );
                yield break;
            }

            yield return new XunitTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);
        }

        static GameTestDiscoverer()
        {
            var locator = new RegistryInstallationLocator();
            InstalledGames = GameDefinition.All.Where(game => locator.FindInstallations(game).Any()).ToSet();
        }
    }
}
