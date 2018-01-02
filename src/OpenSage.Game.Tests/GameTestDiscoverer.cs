using System.Collections.Generic;
using System.Linq;
using OpenSage.Data;
using OpenSage.Utilities.Extensions;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OpenSage.Game.Tests
{
    public sealed class GameTestDiscoverer : IXunitTestCaseDiscoverer
    {
        private readonly IMessageSink _diagnosticMessageSink;

        private static readonly ISet<SageGame> InstalledGames;

        public GameTestDiscoverer(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }

        public IEnumerable<IXunitTestCase> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod, IAttributeInfo factAttribute)
        {
            var arguments = factAttribute.GetConstructorArguments().ToList();
            var game = (SageGame) arguments[0];
            var otherGames = (SageGame[]) arguments[1];

            var games = new[] { game }.Union(otherGames).ToArray();

            if (!InstalledGames.Any(x => games.Contains(x)))
            {
                _diagnosticMessageSink.OnMessage(
                    new DiagnosticMessage($"Skipped test {testMethod.TestClass.Class.Name}.{testMethod.Method.Name}, because it requires one or more of the following games to be installed: {string.Join(", ", games)}.")
                );
                yield break;
            }

            yield return new XunitTestCase(_diagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), testMethod);
        }

        static GameTestDiscoverer()
        {
            var locator = new RegistryInstallationLocator();
            InstalledGames = SageGames.GetAll().Where(game => locator.FindInstallations(game).Any()).ToSet();
        }
    }
}
